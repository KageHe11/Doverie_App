using BE;

namespace BLL
{
    /// Observer (patrón Observer): contrato común de cada canal de
    /// notificación. GestorEventos le pasa el evento solo si el
    /// destinatario está suscripto a ese evento por este canal.
    public interface IObservador
    {
        /// Canal que atiende (CatalogoEventos.CanalEmail / CanalPush).
        string Canal { get; }
        void Notificar(string evento, DatosEventoBE datos);
    }
}
