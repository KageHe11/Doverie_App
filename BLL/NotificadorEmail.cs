using System.Net;
using BE;
using SERVICE;

namespace BLL
{
    /// ConcreteObserver: notificación por correo electrónico, delegando el
    /// envío en EmailService. El mensaje se arma en el idioma preferido del
    /// destinatario, no en el de quien disparó el evento.
    public class NotificadorEmail : IObservador
    {
        public string Canal
        {
            get { return CatalogoEventos.CanalEmail; }
        }
        public void Notificar(string evento, DatosEventoBE datos)
        {
            UsuarioBE Destinatario = datos.Destinatario;
            if (string.IsNullOrWhiteSpace(Destinatario.Gmail)) return;
            IdiomaBLL IdiomaBll = new IdiomaBLL();
            ContextoIdioma Idioma = IdiomaBll.CrearContexto(Destinatario.IdiomaId);
            string Asunto = Idioma.Traducir("notif.email.asunto", Idioma.Traducir("notif.evento." + evento));
            string Mensaje = NotificacionBLL.ArmarMensaje(Idioma, evento, datos.Detalle);
            string Cuerpo = "<p>" + WebUtility.HtmlEncode(Idioma.Traducir("notif.email.saludo", Destinatario.Nombre)) + "</p>" +
                            "<p>" + WebUtility.HtmlEncode(Mensaje) + "</p>" +
                            "<p style=\"color:#888\">" + WebUtility.HtmlEncode(Idioma.Traducir("notif.email.pie")) + "</p>";
            EmailService EmailServ = new EmailService();
            EmailServ.EnviarEmail(Destinatario.Gmail, Asunto, Cuerpo);
        }
    }
}
