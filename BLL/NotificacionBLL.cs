using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;
using SERVICE;

namespace BLL
{
    /// CU-001-015 Configurar Notificaciones y bandeja de notificaciones
    /// recibidas dentro de la plataforma.
    public class NotificacionBLL
    {
        /// Texto de una notificación en el idioma dado. En los eventos de
        /// suscripción el detalle es el nivel del plan, que también se traduce.
        public static string ArmarMensaje(ContextoIdioma idioma, string evento, string detalle)
        {
            string Valor = detalle ?? "";
            bool EsPlan = evento == CatalogoEventos.SuscripcionVencimiento || evento == CatalogoEventos.SuscripcionRenovacion ||
                evento == CatalogoEventos.RenovacionPendiente || evento == CatalogoEventos.PagoRechazado;
            if (EsPlan && Valor.Length > 0) Valor = idioma.Traducir("plan.nivel." + Valor);
            return idioma.Traducir("notif.msg." + evento, Valor);
        }
        public const int CantidadBandeja = 50;

        /// Preferencias del Usuario para cada evento del catálogo. Si nunca
        /// las configuró, todos los eventos llegan dentro de la plataforma.
        public List<PreferenciaNotificacionBE> ObtenerPreferencias(int usuarioId)
        {
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            int? NotificacionId = NotificacionDal.ObtenerIdPorUsuario(usuarioId);
            List<KeyValuePair<string, string>> Guardadas = NotificacionId.HasValue
                ? NotificacionDal.ListarEventos(NotificacionId.Value)
                : null;
            List<PreferenciaNotificacionBE> Preferencias = new List<PreferenciaNotificacionBE>();
            foreach (KeyValuePair<string, string> Evento in CatalogoEventos.Eventos)
            {
                PreferenciaNotificacionBE Preferencia = new PreferenciaNotificacionBE { Evento = Evento.Key, Categoria = Evento.Value };
                if (Guardadas == null)
                {
                    Preferencia.Push = true;
                }
                else
                {
                    Preferencia.Email = Guardadas.Any(G => G.Key == Evento.Key && G.Value == CatalogoEventos.CanalEmail);
                    Preferencia.Push = Guardadas.Any(G => G.Key == Evento.Key && G.Value == CatalogoEventos.CanalPush);
                }
                Preferencia.Habilitado = Preferencia.Email || Preferencia.Push;
                Preferencias.Add(Preferencia);
            }
            return Preferencias;
        }
        /// CU-001-015. Devuelve false si algún evento habilitado quedó sin
        /// canal [ERROR-004]; en ese caso no se guarda nada y sinCanal trae
        /// esos eventos.
        public bool GuardarPreferencias(int usuarioId, List<PreferenciaNotificacionBE> preferencias, out List<string> sinCanal)
        {
            // 8. Cada evento habilitado tiene que tener al menos un canal.
            sinCanal = preferencias.Where(P => P.Habilitado && !P.Email && !P.Push).Select(P => P.Evento).ToList();
            if (sinCanal.Count > 0) return false;
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            int? NotificacionId = NotificacionDal.ObtenerIdPorUsuario(usuarioId);
            int Id = NotificacionId ?? NotificacionDal.Registrar(usuarioId);
            NotificacionDal.EliminarEventos(Id);
            foreach (PreferenciaNotificacionBE Preferencia in preferencias.Where(P => P.Habilitado))
            {
                // Solo se aceptan eventos del catálogo.
                if (!CatalogoEventos.Eventos.Any(E => E.Key == Preferencia.Evento)) continue;
                if (Preferencia.Email) NotificacionDal.RegistrarEvento(Id, Preferencia.Evento, CatalogoEventos.CanalEmail);
                if (Preferencia.Push) NotificacionDal.RegistrarEvento(Id, Preferencia.Evento, CatalogoEventos.CanalPush);
            }
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Configurar Notificaciones", "Eventos habilitados: " + preferencias.Count(P => P.Habilitado) + " de " + preferencias.Count, usuarioId);
            return true;
        }
        /// Canales por los que el Usuario quiere recibir un evento.
        public ICollection<string> CanalesSuscriptos(int usuarioId, string evento)
        {
            PreferenciaNotificacionBE Preferencia = ObtenerPreferencias(usuarioId).FirstOrDefault(P => P.Evento == evento);
            List<string> Canales = new List<string>();
            if (Preferencia == null) return Canales;
            if (Preferencia.Email) Canales.Add(CatalogoEventos.CanalEmail);
            if (Preferencia.Push) Canales.Add(CatalogoEventos.CanalPush);
            return Canales;
        }
        public List<NotificacionRecibidaBE> ListarRecibidas(int usuarioId)
        {
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            return NotificacionDal.ListarRecibidas(usuarioId, CantidadBandeja);
        }
        public int ContarNoLeidas(int usuarioId)
        {
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            return NotificacionDal.ContarNoLeidas(usuarioId);
        }
        public void MarcarLeidas(int usuarioId)
        {
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            NotificacionDal.MarcarLeidas(usuarioId);
        }
    }
}
