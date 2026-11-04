using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL
{
    /// CU-001-005 Compartir Documento y CU-001-018 Acceder a Documento
    /// Compartido: enlaces de solo lectura para destinatarios externos.
    public class DocumentoCompartidoBLL
    {
        /// Vigencias que se pueden elegir al compartir, en días.
        public static readonly int[] VigenciasDias = { 1, 7, 30 };

        /// CU-001-005. Genera el enlace y lo envía por email. Si el envío
        /// falla, devuelve EnlaceNoEnviado [ERROR-013] pero el enlace igual
        /// queda creado y se devuelve en "enlace", para copiarlo a mano.
        public ResultadoDocumento Compartir(UsuarioBE usuario, int documentoId, string emailDestinatario, int dias, string urlBase, out string enlace)
        {
            enlace = null;
            DocumentoBE Documento;
            ResultadoDocumento Acceso = new DocumentoBLL().ObtenerAccesible(usuario, documentoId, out Documento);
            if (Acceso != ResultadoDocumento.Exitoso) return Acceso;
            // 4. Validar el email [ERROR-004]
            string Email = (emailDestinatario ?? "").Trim();
            if (Email.Length == 0 || Email.Length > 100 || !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") || !VigenciasDias.Contains(dias))
            {
                return ResultadoDocumento.DatosInvalidos;
            }
            // 5. Generar el enlace seguro de solo lectura [ERROR-013]
            string Token;
            DocumentoCompartidoBE Compartido;
            try
            {
                Token = new TokenService().GenerarTokenEnlace();
                Compartido = new DocumentoCompartidoBE
                {
                    DocumentoId = Documento.Id,
                    UsuarioId = usuario.Id,
                    EmailDestinatario = Email,
                    FechaCreacion = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddDays(dias)
                };
                DocumentoCompartidoDAL CompartidoDal = new DocumentoCompartidoDAL();
                CompartidoDal.Registrar(Compartido, HashToken(Token));
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorEnlace;
            }
            enlace = urlBase.TrimEnd('/') + "/DocumentoCompartido.aspx?token=" + Token;
            DocumentoBLL.RegistrarEvento("Compartir Documento", Documento, DocumentoBLL.ReferenciaEnlace + Compartido.Id, usuario.Id);
            // 6. Enviar el enlace por email [ERROR-013]
            try
            {
                EnviarEmail(usuario, Documento, Compartido, enlace);
            }
            catch (Exception)
            {
                return ResultadoDocumento.EnlaceNoEnviado;
            }
            return ResultadoDocumento.Exitoso;
        }
        /// Enlaces generados para un documento de la cuenta del Usuario.
        public List<DocumentoCompartidoBE> Listar(UsuarioBE usuario, int documentoId)
        {
            DocumentoBE Documento;
            if (new DocumentoBLL().ObtenerAccesible(usuario, documentoId, out Documento) == ResultadoDocumento.SinAcceso)
            {
                return new List<DocumentoCompartidoBE>();
            }
            DocumentoCompartidoDAL CompartidoDal = new DocumentoCompartidoDAL();
            return CompartidoDal.ListarPorDocumento(documentoId);
        }
        /// Revoca un enlace: deja de dar acceso aunque no haya vencido.
        public ResultadoDocumento Revocar(UsuarioBE usuario, int compartidoId)
        {
            DocumentoCompartidoDAL CompartidoDal = new DocumentoCompartidoDAL();
            DocumentoCompartidoBE Compartido = CompartidoDal.ObtenerDatos(compartidoId);
            if (Compartido == null) return ResultadoDocumento.SinAcceso;
            DocumentoBE Documento;
            if (new DocumentoBLL().ObtenerAccesible(usuario, Compartido.DocumentoId, out Documento) == ResultadoDocumento.SinAcceso)
            {
                return ResultadoDocumento.SinAcceso;
            }
            CompartidoDal.Revocar(compartidoId);
            DocumentoBLL.RegistrarEvento("Revocar Enlace Compartido", new DocumentoDAL().ObtenerDatos(Compartido.DocumentoId), DocumentoBLL.ReferenciaEnlace + Compartido.Id, usuario.Id);
            return ResultadoDocumento.Exitoso;
        }
        /// CU-001-018 pasos 2 y 3: valida el token del enlace. Un token que
        /// no existe, venció o fue revocado da siempre el mismo resultado
        /// [ERROR-044], para no revelar cuál de las tres cosas pasó.
        public ResultadoDocumento Acceder(string token, out DocumentoCompartidoBE compartido, out DocumentoBE documento)
        {
            compartido = null;
            documento = null;
            if (string.IsNullOrWhiteSpace(token) || token.Length > 100) return ResultadoDocumento.EnlaceInvalido;
            DocumentoCompartidoDAL CompartidoDal = new DocumentoCompartidoDAL();
            compartido = CompartidoDal.ObtenerPorTokenHash(HashToken(token));
            if (compartido == null || !compartido.Vigente)
            {
                compartido = null;
                return ResultadoDocumento.EnlaceInvalido;
            }
            DocumentoDAL DocumentoDal = new DocumentoDAL();
            documento = DocumentoDal.ObtenerDatos(compartido.DocumentoId);
            if (documento == null) return ResultadoDocumento.EnlaceInvalido;
            if (documento.Estado == DocumentoBE.EstadoEliminado) return ResultadoDocumento.YaEliminado;
            return ResultadoDocumento.Exitoso;
        }
        /// CU-001-018 paso 4: el archivo del documento [ERROR-017].
        public ResultadoDocumento ObtenerContenido(DocumentoBE documento, out byte[] contenido)
        {
            contenido = null;
            try
            {
                contenido = ProveedorServicios.Almacenamiento().Obtener(documento.UrlDocumento);
                return ResultadoDocumento.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoDocumento.ErrorObtener;
            }
        }
        /// CU-001-018 pasos 7 y 8: registra el acceso (contador del enlace y
        /// Bitácora, con el destinatario) y avisa al titular de la cuenta.
        public void RegistrarAcceso(DocumentoCompartidoBE compartido, DocumentoBE documento, bool descarga, bool notificarTitular)
        {
            if (!descarga)
            {
                DocumentoCompartidoDAL CompartidoDal = new DocumentoCompartidoDAL();
                CompartidoDal.RegistrarAcceso(compartido.Id);
            }
            DocumentoBLL.RegistrarEvento(descarga ? "Descarga Documento Compartido" : "Acceso Documento Compartido", documento, DocumentoBLL.ReferenciaEnlace + compartido.Id, null);
            if (notificarTitular)
            {
                GestorEventos.Notificar(CatalogoEventos.DocumentoAccedido, documento.CuentaId, documento.Nombre + " (" + compartido.EmailDestinatario + ")");
            }
        }
        private static string HashToken(string token)
        {
            return new HashService().CalcularHashArchivo(Encoding.UTF8.GetBytes(token));
        }
        /// El email sale en el idioma preferido de quien comparte (el
        /// destinatario es externo y no tiene preferencia guardada).
        private static void EnviarEmail(UsuarioBE usuario, DocumentoBE documento, DocumentoCompartidoBE compartido, string enlace)
        {
            ContextoIdioma Idioma = new IdiomaBLL().CrearContexto(usuario.IdiomaId);
            string Remitente = (usuario.Nombre + " " + usuario.Apellido).Trim();
            string Asunto = Idioma.Traducir("compartir.email.asunto", Remitente);
            string Cuerpo =
                "<p>" + WebUtility.HtmlEncode(Idioma.Traducir("compartir.email.cuerpo", Remitente, documento.Nombre)) + "</p>" +
                "<p><a href=\"" + WebUtility.HtmlEncode(enlace) + "\">" + WebUtility.HtmlEncode(Idioma.Traducir("compartir.email.ver")) + "</a></p>" +
                "<p style=\"color:#888\">" + WebUtility.HtmlEncode(Idioma.Traducir("compartir.email.vence", compartido.FechaVencimiento.ToString("dd/MM/yyyy HH:mm"))) + "</p>";
            new EmailService().EnviarEmail(compartido.EmailDestinatario, Asunto, Cuerpo);
        }
    }
}
