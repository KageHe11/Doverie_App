using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BE;
using DAL;
using SERVICE;

namespace BLL
{
    /// CU-002-013 Generar Backup Manual: copia de la base de datos y de los
    /// archivos de los documentos (AWS S3 o disco, según el almacenamiento
    /// configurado), y su restore. Cada intento de backup queda en la tabla
    /// BackUp y en la Bitácora, salga bien o mal.
    public class BackupBLL
    {
        /// Un backup o restore a la vez: dos administradores no pueden pisarse.
        private static readonly object Bloqueo = new object();
        private const string PrefijoArchivo = "DB_Doverie_";

        public ResultadoBackup Generar(UsuarioBE administrador, out BackupBE backup)
        {
            lock (Bloqueo)
            {
                // Sin milisegundos: la etiqueta del backup sale de la fecha y
                // tiene que poder reconstruirse igual desde la base.
                DateTime Ahora = DateTime.Now;
                DateTime Fecha = new DateTime(Ahora.Year, Ahora.Month, Ahora.Day, Ahora.Hour, Ahora.Minute, Ahora.Second);
                string Etiqueta = EtiquetaDe(Fecha);
                backup = new BackupBE { UsuarioId = administrador.Id, NombreUsuario = administrador.NombreUsuario, Fecha = Fecha };
                // 4. Copia de la base de datos [ERROR-041]
                string RutaBak;
                long TamanoBak;
                try
                {
                    RutaBak = new BackupDAL().GenerarBaseDatos(PrefijoArchivo + Etiqueta + ".bak", out TamanoBak);
                }
                catch (Exception)
                {
                    return Finalizar(backup, ResultadoBackup.ErrorBaseDatos, "Falló la copia de la base de datos.");
                }
                // El .bak lo escribe SQL Server; si corre en otro equipo, la app
                // no lo ve y la copia queda solo en ese servidor.
                bool BakAccesible = File.Exists(RutaBak);
                string DetalleBase = "Base: " + Path.GetFileName(RutaBak) + " (" + FormatearTamano(TamanoBak) + ")" +
                    (BakAccesible ? "" : ", solo en el servidor de SQL Server");
                // 5. Copia de los archivos (AWS S3) [ERROR-042]
                RespaldoArchivosBE Archivos;
                try
                {
                    Archivos = ProveedorServicios.Almacenamiento().Respaldar(Etiqueta, BakAccesible ? RutaBak : null);
                }
                catch (Exception)
                {
                    return Finalizar(backup, ResultadoBackup.ErrorArchivos, DetalleBase + ". Falló la copia de los archivos.");
                }
                return Finalizar(backup, ResultadoBackup.Exitoso, DetalleBase + ". Archivos: " + Archivos.CantidadArchivos +
                    " (" + FormatearTamano(Archivos.TamanoBytes) + ") en " + Archivos.Ubicacion);
            }
        }
        public List<BackupBE> Listar()
        {
            return new BackupDAL().Listar();
        }

        // ---------- Restore ----------

        /// Restaura un backup exitoso del historial. Su .bak se toma de la
        /// carpeta de backups o, si ya no está ahí, del almacenamiento (S3).
        public ResultadoRestauracion RestaurarDesdeHistorial(UsuarioBE administrador, int backupId, out BackupBE previo)
        {
            previo = null;
            lock (Bloqueo)
            {
                BackupBE Elegido = new BackupDAL().ObtenerDatos(backupId);
                if (Elegido == null || Elegido.Estado != BackupBE.EstadoExitoso) return ResultadoRestauracion.NoEncontrado;
                string Etiqueta = EtiquetaDe(Elegido.Fecha);
                string Archivo = PrefijoArchivo + Etiqueta + ".bak";
                string Ruta;
                try
                {
                    Ruta = Path.Combine(new BackupDAL().ObtenerCarpeta(), Archivo);
                    if (!File.Exists(Ruta) && !ProveedorServicios.Almacenamiento().ObtenerBaseDatos(Etiqueta, Archivo, Ruta))
                    {
                        return ResultadoRestauracion.ArchivoNoDisponible;
                    }
                }
                catch (Exception)
                {
                    return ResultadoRestauracion.ArchivoNoDisponible;
                }
                return Restaurar(administrador, Ruta, Etiqueta, "backup del " + Elegido.Fecha.ToString("dd/MM/yyyy HH:mm:ss"), out previo);
            }
        }
        /// Restaura un .bak subido por el Administrador. Se guarda en la carpeta
        /// de backups (SQL Server tiene que poder leerlo) y solo se acepta si
        /// es de esta base. Si es un backup de Doverie, también se restauran
        /// los archivos de ese backup.
        public ResultadoRestauracion RestaurarDesdeArchivo(UsuarioBE administrador, string nombreArchivo, byte[] contenido, out BackupBE previo)
        {
            previo = null;
            if (contenido == null || contenido.Length == 0 ||
                !string.Equals(Path.GetExtension(nombreArchivo ?? ""), ".bak", StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoRestauracion.ArchivoInvalido;
            }
            lock (Bloqueo)
            {
                string Ruta;
                try
                {
                    Ruta = Path.Combine(new BackupDAL().ObtenerCarpeta(), PrefijoArchivo + "subido_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".bak");
                    File.WriteAllBytes(Ruta, contenido);
                }
                catch (Exception)
                {
                    return ResultadoRestauracion.ArchivoNoDisponible;
                }
                return Restaurar(administrador, Ruta, BuscarEtiqueta(nombreArchivo), "archivo " + Path.GetFileName(nombreArchivo), out previo);
            }
        }
        /// Pasos del restore: validar el .bak, generar un backup de seguridad
        /// del estado actual (para poder volver atrás), restaurar la base y
        /// después los archivos.
        private ResultadoRestauracion Restaurar(UsuarioBE administrador, string rutaBak, string etiqueta, string origen, out BackupBE previo)
        {
            previo = null;
            BackupDAL BackupDal = new BackupDAL();
            // Validar: tiene que ser un .bak legible de esta misma base.
            try
            {
                string BaseDatos, NombreBackup;
                BackupDal.LeerEncabezado(rutaBak, out BaseDatos, out NombreBackup);
                if (!string.Equals(BaseDatos, BackupDal.NombreBase(), StringComparison.OrdinalIgnoreCase)) return ResultadoRestauracion.ArchivoInvalido;
                BackupDal.Verificar(rutaBak);
                // El nombre que Doverie le pone al backup trae su fecha.
                etiqueta = BuscarEtiqueta(NombreBackup) ?? etiqueta;
            }
            catch (Exception)
            {
                return ResultadoRestauracion.ArchivoInvalido;
            }
            // Backup de seguridad del estado actual.
            if (Generar(administrador, out previo) != ResultadoBackup.Exitoso) return ResultadoRestauracion.ErrorRespaldoPrevio;
            // El historial de backups está en la misma base que se restaura:
            // se guarda antes, porque esos backups siguen existiendo.
            List<BackupBE> Historial = BackupDal.Listar();
            // Base de datos
            try
            {
                BackupDal.RestaurarBaseDatos(rutaBak);
            }
            catch (Exception)
            {
                RegistrarRestore(administrador, previo, Historial, "Falló el restore de la base desde " + origen + ". La base quedó como estaba.");
                return ResultadoRestauracion.ErrorBaseDatos;
            }
            // Archivos
            RespaldoArchivosBE Archivos = null;
            try
            {
                if (etiqueta != null) Archivos = ProveedorServicios.Almacenamiento().RestaurarArchivos(etiqueta);
            }
            catch (Exception)
            {
                RegistrarRestore(administrador, previo, Historial, "Base restaurada desde " + origen + ", pero falló el restore de los archivos.");
                return ResultadoRestauracion.ErrorArchivos;
            }
            string Detalle = "Base restaurada desde " + origen + ". " + (Archivos == null
                ? "Sin copia de archivos asociada: los archivos quedaron como estaban."
                : "Archivos: " + Archivos.CantidadArchivos + " restaurados y " + Archivos.CantidadEliminados + " quitados, desde " + Archivos.Ubicacion + ".");
            RegistrarRestore(administrador, previo, Historial, Detalle);
            return Archivos == null ? ResultadoRestauracion.ExitosoSinArchivos : ResultadoRestauracion.Exitoso;
        }
        /// La base restaurada trae el historial de backups de su momento: se
        /// vuelven a agregar los que había antes del restore (incluido el de
        /// seguridad), y se registra el restore en la Bitácora.
        private static void RegistrarRestore(UsuarioBE administrador, BackupBE previo, List<BackupBE> historial, string detalle)
        {
            BackupDAL BackupDal = new BackupDAL();
            List<BackupBE> Actual = BackupDal.Listar();
            bool PrevioAgregado = false;
            // Del más viejo al más nuevo, para que los Id sigan el orden de las fechas.
            foreach (BackupBE Backup in historial.OrderBy(B => B.Fecha))
            {
                if (Actual.Exists(B => B.Fecha == Backup.Fecha && B.Estado == Backup.Estado)) continue;
                BackupDal.Registrar(Backup);
                if (Backup.Fecha == previo.Fecha) PrevioAgregado = true;
            }
            if (PrevioAgregado)
            {
                new BitacoraBLL().Registrar("Generar Backup Manual", previo.Estado + " (antes del restore). " + previo.Detalle, previo.UsuarioId);
            }
            new BitacoraBLL().Registrar("Restaurar Backup", detalle + " Backup de seguridad previo: " + EtiquetaDe(previo.Fecha) + ".", administrador.Id);
        }
        private static ResultadoBackup Finalizar(BackupBE backup, ResultadoBackup resultado, string detalle)
        {
            backup.Estado = resultado == ResultadoBackup.Exitoso ? BackupBE.EstadoExitoso : BackupBE.EstadoFallido;
            backup.Detalle = detalle.Length > 500 ? detalle.Substring(0, 500) : detalle;
            new BackupDAL().Registrar(backup);
            new BitacoraBLL().Registrar("Generar Backup Manual", backup.Estado + ". " + backup.Detalle, backup.UsuarioId);
            return resultado;
        }
        /// "yyyyMMdd-HHmmss": nombre de la carpeta del backup y parte del .bak.
        public static string EtiquetaDe(DateTime fecha)
        {
            return fecha.ToString("yyyyMMdd-HHmmss");
        }
        private static string BuscarEtiqueta(string texto)
        {
            Match Coincidencia = Regex.Match(texto ?? "", @"\d{8}-\d{6}");
            return Coincidencia.Success ? Coincidencia.Value : null;
        }
        private static string FormatearTamano(long bytes)
        {
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("0.0") + " MB";
        }
    }
}
