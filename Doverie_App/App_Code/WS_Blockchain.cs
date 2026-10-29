using System;
using System.Web.Services;
using BLL;

/// WS-003 WS_Blockchain.asmx: métricas de las operaciones registradas en la
/// red blockchain. Lo consume Estadisticas.aspx. Fechas en formato yyyy-MM-dd.
[WebService(Namespace = "http://doverie.com/ws/", Description = "Métricas de operaciones en la red blockchain")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class WS_Blockchain : ServicioProtegido
{
    [WebMethod(EnableSession = true, Description = "Cantidad total de operaciones registradas en la blockchain en el período (salida: WS_OperacionesPorPeriodo.xml)")]
    public int ObtenerOperacionesPorPeriodo(string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return 0;
        return new EstadisticaBLL().ObtenerOperacionesPorPeriodo(Desde, Hasta);
    }

    [WebMethod(EnableSession = true, Description = "Cantidad total de transacciones generadas por un usuario en el período (salida: WS_TransaccionesPorUsuario.xml)")]
    public int ObtenerTransaccionesPorUsuario(int usuarioId, string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return 0;
        return new EstadisticaBLL().ObtenerTransaccionesPorUsuario(usuarioId, Desde, Hasta);
    }
}
