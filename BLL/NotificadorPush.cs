using BE;
using DAL;

namespace BLL
{
    /// ConcreteObserver: notificación dentro de la plataforma. La deja en
    /// la bandeja del destinatario (campana de la navbar).
    public class NotificadorPush : IObservador
    {
        public string Canal
        {
            get { return CatalogoEventos.CanalPush; }
        }
        public void Notificar(string evento, DatosEventoBE datos)
        {
            NotificacionDAL NotificacionDal = new NotificacionDAL();
            NotificacionDal.RegistrarRecibida(datos.Destinatario.Id, evento, datos.Detalle);
        }
    }
}
