using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BLL
{
    /// Casos de uso de Documentos. El almacenamiento y la blockchain se
    /// obtienen de ProveedorServicios a través de sus interfaces, así que
    /// esta clase no cambia al pasar de la implementación local a S3 y
    /// Nethereum.
    public class DocumentoBLL
    {
        public const int TamanoMaximoBytes = 10 * 1024 * 1024;
        public static readonly string[] ExtensionesPermitidas =
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".odt", ".txt", ".png", ".jpg", ".jpeg"
        };

        /// Cuenta a la que pertenece un Usuario: la Empresa para ella misma y
        /// sus empleados, o el propio Usuario si es independiente.
        public static int CuentaDe(UsuarioBE usuario)
        {
            return usuario.EmpresaId ?? usuario.Id;
        }
        /// CU-001-012 Ver Listado de Documentos (con filtros opcionales).
        public List<DocumentoBE> Listar(UsuarioBE usuario, string nombre, string tipo, DateTime? desde, DateTime? hasta)
        {
            DocumentoDAL DocumentoDal = new DocumentoDAL();
            IEnumerable<DocumentoBE> Resultado = DocumentoDal.ListarPorCuenta(CuentaDe(usuario));
            if (!string.IsNullOrWhiteSpace(nombre)) Resultado = Resultado.Where(D => D.Nombre.IndexOf(nombre.Trim(), StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(tipo)) Resultado = Resultado.Where(D => D.Tipo == tipo);
            if (desde.HasValue) Resultado = Resultado.Where(D => D.FechaSubida >= desde.Value);
            if (hasta.HasValue) Resultado = Resultado.Where(D => D.FechaSubida <= hasta.Value);
            return Resultado.ToList();
        }
        /// Tipos presentes en la cuenta, para el filtro por tipo.
        public List<string> ListarTipos(UsuarioBE usuario)
        {
            DocumentoDAL DocumentoDal = new DocumentoDAL();
            return DocumentoDal.ListarPorCuenta(CuentaDe(usuario)).Select(D => D.Tipo).Where(T => T != "").Distinct().OrderBy(T => T).ToList();
        }
        /// CU-001-002 Subir Documento.
        public ResultadoDocumento Subir(UsuarioBE usuario, string nombreArchivo, byte[] contenido, out DocumentoBE documento)
        {
            documento = null;
            // 4. Validar el archivo [ERROR-010]
            string Nombre = Path.GetFileName(nombreArchivo ?? "");
            if (contenido == null || contenido.Length == 0 || contenido.Length > TamanoMaximoBytes ||
                Nombre.Length == 0 || Nombre.Length > 255 ||
                !ExtensionesPermitidas.Contains(Path.GetExtension(Nombre).ToLowerInvariant()))
            {
                return ResultadoDocumento.ArchivoInvalido;
            }
            // 5. Almacenar el archivo [ERROR-011]
            IAlmacenamientoArchivos Almacenamiento = ProveedorServicios.Almacenamiento();
            string Ubicacion;
            try
            {
                Ubicacion = Almacenamiento.Guardar(contenido);
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorAlmacenamiento;
            }
            // 6. Generar el hash del documento
            HashService HashServ = new HashService();
            string Hash = HashServ.CalcularHashArchivo(contenido);
            // 7. Registrar el hash y el sellado de tiempo en la blockchain
            // [ERROR-012]. Si falla, el archivo ya guardado se descarta:
            // un documento sin registro en blockchain no tiene valor.
            SelloBlockchainBE Sello;
            try
            {
                Sello = ProveedorServicios.Blockchain().Registrar(Hash);
            }
            catch (Exception)
            {
                DescartarArchivo(Almacenamiento, Ubicacion);
                return ResultadoDocumento.ErrorBlockchain;
            }
            documento = new DocumentoBE
            {
                UsuarioId = usuario.Id,
                Nombre = Nombre,
                UrlDocumento = Ubicacion,
                Hash = Hash,
                TransaccionBlockchain = Sello.Transaccion,
                Estado = DocumentoBE.EstadoActivo,
                FechaSubida = Sello.FechaSellado
            };
            try
            {
                DocumentoDAL DocumentoDal = new DocumentoDAL();
                DocumentoDal.Registrar(documento);
            }
            catch (Exception)
            {
                DescartarArchivo(Almacenamiento, Ubicacion);
                documento = null;
                return ResultadoDocumento.ErrorAlmacenamiento;
            }
            RegistrarEvento("Subir Documento", documento, null, usuario.Id);
            // Se avisa al titular de la cuenta (la Empresa, si quien subió
            // es un empleado, o el propio Usuario independiente).
            GestorEventos.Notificar(CatalogoEventos.DocumentoSubido, CuentaDe(usuario), documento.Nombre);
            return ResultadoDocumento.Exitoso;
        }
        /// CU-001-003 Descargar Documento.
        public ResultadoDocumento Descargar(UsuarioBE usuario, int id, out DocumentoBE documento, out byte[] contenido)
        {
            contenido = null;
            ResultadoDocumento Acceso = ObtenerAccesible(usuario, id, out documento);
            if (Acceso != ResultadoDocumento.Exitoso) return Acceso;
            // 4. Obtener el archivo [ERROR-017]
            try
            {
                contenido = ProveedorServicios.Almacenamiento().Obtener(documento.UrlDocumento);
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorObtener;
            }
            RegistrarEvento("Descargar Documento", documento, null, usuario.Id);
            // Acceso de otro Usuario de la cuenta (ej: un empleado) a un
            // documento: se avisa al titular. El titular no se avisa a sí mismo.
            if (usuario.Id != documento.CuentaId)
            {
                GestorEventos.Notificar(CatalogoEventos.DocumentoAccedido, documento.CuentaId, documento.Nombre + " (" + usuario.NombreUsuario + ")");
            }
            return ResultadoDocumento.Exitoso;
        }
        /// CU-001-004 Eliminar Documento. El archivo se borra del
        /// almacenamiento; el registro en la base y en la blockchain queda.
        public ResultadoDocumento Eliminar(UsuarioBE usuario, int id)
        {
            DocumentoBE Documento;
            ResultadoDocumento Acceso = ObtenerAccesible(usuario, id, out Documento);
            if (Acceso != ResultadoDocumento.Exitoso) return Acceso;
            // 6. y 7. Eliminar el archivo y actualizar el estado [ERROR-006]
            try
            {
                ProveedorServicios.Almacenamiento().Eliminar(Documento.UrlDocumento);
                DocumentoDAL DocumentoDal = new DocumentoDAL();
                DocumentoDal.ActualizarEstado(Documento.Id, DocumentoBE.EstadoEliminado);
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorEliminar;
            }
            RegistrarEvento("Eliminar Documento", Documento, null, usuario.Id);
            return ResultadoDocumento.Exitoso;
        }
        /// CU-001-006 Verificar Integridad. Devuelve Modificado si el hash
        /// del archivo actual no coincide con el registrado en blockchain;
        /// en ese caso verificacion igual trae los dos hashes para mostrar.
        public ResultadoDocumento VerificarIntegridad(UsuarioBE usuario, int id, out DocumentoBE documento, out VerificacionDocumentoBE verificacion)
        {
            verificacion = null;
            ResultadoDocumento Acceso = ObtenerAccesible(usuario, id, out documento);
            if (Acceso != ResultadoDocumento.Exitoso) return Acceso;
            // 4. Obtener el archivo [ERROR-017]
            byte[] Contenido;
            try
            {
                Contenido = ProveedorServicios.Almacenamiento().Obtener(documento.UrlDocumento);
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorObtener;
            }
            // 5. Hash actual
            HashService HashServ = new HashService();
            string HashActual = HashServ.CalcularHashArchivo(Contenido);
            // 6. Hash original registrado en la blockchain [ERROR-012]
            SelloBlockchainBE Sello;
            try
            {
                Sello = ProveedorServicios.Blockchain().Consultar(documento.TransaccionBlockchain);
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorBlockchain;
            }
            if (Sello == null) return ResultadoDocumento.ErrorBlockchain;
            // 7. y 8. Comparar [ERROR-018]
            verificacion = new VerificacionDocumentoBE
            {
                HashActual = HashActual,
                Sello = Sello,
                Integro = string.Equals(HashActual, Sello.Hash, StringComparison.OrdinalIgnoreCase)
            };
            RegistrarEvento("Verificar Integridad", documento, verificacion.Integro ? "íntegro" : "MODIFICADO", usuario.Id);
            new DocumentoDAL().RegistrarVerificacion(documento.Id, usuario.Id, verificacion.Integro);
            if (!verificacion.Integro)
            {
                GestorEventos.Notificar(CatalogoEventos.ModificacionDetectada, documento.CuentaId, documento.Nombre);
            }
            return verificacion.Integro ? ResultadoDocumento.Exitoso : ResultadoDocumento.Modificado;
        }
        /// Historial de accesos y modificaciones de un documento (CU-001-006
        /// paso 9), tomado de la Bitácora completa (base + archivo XML).
        public List<BitacoraBE> Historial(int id)
        {
            string Prefijo = PrefijoBitacora(id);
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            return BitacoraBll.ObtenerHistorialCompleto()
                .Where(B => B.Detalle != null && B.Detalle.StartsWith(Prefijo, StringComparison.Ordinal))
                .ToList();
        }
        /// Trae el documento solo si pertenece a la cuenta del Usuario y
        /// sigue activo. Un documento de otra cuenta se informa igual que
        /// uno inexistente, para no revelar que existe.
        public ResultadoDocumento ObtenerAccesible(UsuarioBE usuario, int id, out DocumentoBE documento)
        {
            DocumentoDAL DocumentoDal = new DocumentoDAL();
            documento = DocumentoDal.ObtenerDatos(id);
            if (documento == null || documento.CuentaId != CuentaDe(usuario))
            {
                documento = null;
                return ResultadoDocumento.SinAcceso;
            }
            if (documento.Estado == DocumentoBE.EstadoEliminado)
            {
                return ResultadoDocumento.YaEliminado;
            }
            return ResultadoDocumento.Exitoso;
        }
        private static void DescartarArchivo(IAlmacenamientoArchivos almacenamiento, string ubicacion)
        {
            try { almacenamiento.Eliminar(ubicacion); } catch (Exception) { }
        }
        private static string PrefijoBitacora(int id)
        {
            return "Documento #" + id + ":";
        }
        /// Registra en la Bitácora un evento sobre un documento, con el
        /// prefijo que usa Historial() para encontrarlo. usuarioId es null
        /// cuando quien accede no es un Usuario (enlace compartido).
        internal static void RegistrarEvento(string accion, DocumentoBE documento, string resultado, int? usuarioId)
        {
            string Detalle = PrefijoBitacora(documento.Id) + " " + documento.Nombre + (resultado != null ? " (" + resultado + ")" : "");
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar(accion, Detalle, usuarioId);
        }
    }
}
