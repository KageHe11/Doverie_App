using System;
using System.Collections.Generic;
using BE;
using DAL;

namespace BLL
{
    /// Subject (patrón Observer). La lógica de negocio le avisa que ocurrió
    /// un evento y a quién le concierne; GestorEventos consulta a qué
    /// canales está suscripto ese Usuario y despacha a cada observador
    /// correspondiente. Quien dispara el evento no sabe cuántos
    /// observadores hay ni cómo se entrega cada aviso.
    public static class GestorEventos
    {
        private static readonly List<IObservador> Observadores = new List<IObservador>();
        private static readonly object Bloqueo = new object();

        static GestorEventos()
        {
            Suscribir(new NotificadorEmail());
            Suscribir(new NotificadorPush());
        }
        public static void Suscribir(IObservador observador)
        {
            lock (Bloqueo)
            {
                if (!Observadores.Contains(observador)) Observadores.Add(observador);
            }
        }
        public static void Desuscribir(IObservador observador)
        {
            lock (Bloqueo)
            {
                Observadores.Remove(observador);
            }
        }
        /// Notifica un evento (CatalogoEventos) al Usuario indicado. Nunca
        /// lanza excepciones: si un canal falla (ej: SMTP caído) se registra
        /// en la Bitácora y la operación que disparó el evento sigue adelante.
        public static void Notificar(string evento, int usuarioDestinoId, string detalle)
        {
            try
            {
                UsuarioDAL UsuarioDal = new UsuarioDAL();
                UsuarioBE Destinatario = UsuarioDal.ObtenerDatos(usuarioDestinoId);
                if (Destinatario == null) return;
                NotificacionBLL NotificacionBll = new NotificacionBLL();
                ICollection<string> Canales = NotificacionBll.CanalesSuscriptos(usuarioDestinoId, evento);
                if (Canales.Count == 0) return;
                DatosEventoBE Datos = new DatosEventoBE
                {
                    Destinatario = Destinatario,
                    Detalle = detalle,
                    Fecha = DateTime.Now
                };
                IObservador[] Copia;
                lock (Bloqueo)
                {
                    Copia = Observadores.ToArray();
                }
                foreach (IObservador Observador in Copia)
                {
                    if (!Canales.Contains(Observador.Canal)) continue;
                    try
                    {
                        Observador.Notificar(evento, Datos);
                    }
                    catch (Exception)
                    {
                        RegistrarFalla(evento, Observador.Canal, usuarioDestinoId);
                    }
                }
            }
            catch (Exception)
            {
                RegistrarFalla(evento, "-", usuarioDestinoId);
            }
        }
        private static void RegistrarFalla(string evento, string canal, int usuarioDestinoId)
        {
            try
            {
                BitacoraBLL BitacoraBll = new BitacoraBLL();
                BitacoraBll.Registrar("Error Notificación", "Evento: " + evento + " | Canal: " + canal + " | Usuario Id: " + usuarioDestinoId, null);
            }
            catch (Exception)
            {
                // Si tampoco se puede registrar la falla, no hay nada más que
                // hacer: la notificación es secundaria a la operación.
            }
        }
    }
}
