using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace SERVICE
{
    public class EmailService
    {
        public void EnviarEmail(string destinatario, string asunto, string cuerpoHtml)
        {
            EnviarEmail(destinatario, asunto, cuerpoHtml, null, null);
        }
        /// Con un archivo adjunto (por ejemplo, la factura en PDF).
        /// Si AppSettings tiene SmtpCarpetaPrueba, el correo no sale: se
        /// guarda como .eml en esa carpeta (para probar o hacer una demo sin
        /// enviar correos reales).
        public void EnviarEmail(string destinatario, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto)
        {
            string Host = ConfigurationManager.AppSettings["SmtpHost"];
            int Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
            string UsuarioSmtp = ConfigurationManager.AppSettings["SmtpUser"];
            string PasswordSmtp = ConfigurationManager.AppSettings["SmtpPassword"];
            string NombreRemitente = ConfigurationManager.AppSettings["SmtpFromName"];
            string CarpetaPrueba = ConfigurationManager.AppSettings["SmtpCarpetaPrueba"];
            using (SmtpClient Cliente = new SmtpClient(Host, Port))
            {
                if (string.IsNullOrWhiteSpace(CarpetaPrueba))
                {
                    Cliente.EnableSsl = true;
                    Cliente.UseDefaultCredentials = false;
                    Cliente.Credentials = new NetworkCredential(UsuarioSmtp, PasswordSmtp);
                }
                else
                {
                    Directory.CreateDirectory(CarpetaPrueba);
                    Cliente.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    Cliente.PickupDirectoryLocation = CarpetaPrueba;
                }
                using (MailMessage Mensaje = new MailMessage())
                {
                    Mensaje.From = new MailAddress(UsuarioSmtp, NombreRemitente);
                    Mensaje.To.Add(destinatario);
                    Mensaje.Subject = asunto;
                    Mensaje.Body = cuerpoHtml;
                    Mensaje.IsBodyHtml = true;
                    if (adjunto != null)
                    {
                        Mensaje.Attachments.Add(new Attachment(new MemoryStream(adjunto), nombreAdjunto, "application/pdf"));
                    }
                    Cliente.Send(Mensaje);
                }
            }
        }
    }
}
