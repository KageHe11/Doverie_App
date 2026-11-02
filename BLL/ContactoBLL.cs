using System;
using System.Net;
using System.Text.RegularExpressions;
using BE;
using SERVICE;

namespace BLL
{
    /// Formulario "Contactanos" de la página de inicio: el mensaje llega por
    /// correo a la casilla de Doverie (ContactoEmail de AppSettings), con el
    /// email del visitante para responderle.
    public class ContactoBLL
    {
        public const int LargoMaximoMensaje = 2000;

        public static string EmailContacto
        {
            get { return EmailService.DireccionContacto; }
        }
        public ResultadoContacto Enviar(string nombre, string email, string mensaje)
        {
            nombre = (nombre ?? "").Trim();
            email = (email ?? "").Trim();
            mensaje = (mensaje ?? "").Trim();
            if (nombre.Length == 0 || nombre.Length > 100 || mensaje.Length == 0 || mensaje.Length > LargoMaximoMensaje ||
                email.Length > 100 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ||
                string.IsNullOrWhiteSpace(EmailContacto))
            {
                return ResultadoContacto.DatosInvalidos;
            }
            string Cuerpo =
                "<p><b>Nuevo mensaje desde el formulario de contacto de Doverie</b></p>" +
                "<p>Nombre: " + WebUtility.HtmlEncode(nombre) + "<br/>Email: " + WebUtility.HtmlEncode(email) + "</p>" +
                "<p style=\"white-space:pre-wrap\">" + WebUtility.HtmlEncode(mensaje) + "</p>";
            try
            {
                new EmailService().EnviarEmail(EmailContacto, "Contacto web: " + nombre, Cuerpo);
            }
            catch (Exception)
            {
                new BitacoraBLL().Registrar("Error Envío Email", "Mensaje de contacto de " + nombre, null);
                return ResultadoContacto.ErrorEnvio;
            }
            // Sin el email del visitante: la Bitácora no guarda datos personales.
            new BitacoraBLL().Registrar("Mensaje de Contacto", "De: " + nombre, null);
            return ResultadoContacto.Exitoso;
        }
    }
}
