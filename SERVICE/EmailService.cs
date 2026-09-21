using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace SERVICE
{
    public class EmailService
    {
        public void EnviarEmail(string destinatario, string asunto, string cuerpoHtml)
        {
            string Host = ConfigurationManager.AppSettings["SmtpHost"];
            int Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
            string UsuarioSmtp = ConfigurationManager.AppSettings["SmtpUser"];
            string PasswordSmtp = ConfigurationManager.AppSettings["SmtpPassword"];
            string NombreRemitente = ConfigurationManager.AppSettings["SmtpFromName"];
            using (SmtpClient Cliente = new SmtpClient(Host, Port))
            {
                Cliente.EnableSsl = true;
                Cliente.UseDefaultCredentials = false;
                Cliente.Credentials = new NetworkCredential(UsuarioSmtp, PasswordSmtp);
                using (MailMessage Mensaje = new MailMessage())
                {
                    Mensaje.From = new MailAddress(UsuarioSmtp, NombreRemitente);
                    Mensaje.To.Add(destinatario);
                    Mensaje.Subject = asunto;
                    Mensaje.Body = cuerpoHtml;
                    Mensaje.IsBodyHtml = true;
                    Cliente.Send(Mensaje);
                }
            }
        }
    }
}