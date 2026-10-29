using System;
using System.Web.Services;
using BE;
using BLL;

/// WS-002 WS_Usuarios.asmx: métricas de usuarios y suscripciones.
/// Lo consume Estadisticas.aspx. Fechas en formato yyyy-MM-dd.
[WebService(Namespace = "http://doverie.com/ws/", Description = "Métricas de usuarios y suscripciones")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class WS_Usuarios : ServicioProtegido
{
    [WebMethod(EnableSession = true, Description = "Cantidad total de usuarios registrados en el período (salida: WS_UsuariosPorPeriodo.xml)")]
    public int ObtenerUsuariosPorPeriodo(string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return 0;
        return new EstadisticaBLL().ObtenerUsuariosPorPeriodo(Desde, Hasta);
    }

    [WebMethod(EnableSession = true, Description = "Plan de suscripción más contratado en el período, o vacío si no hay suscripciones (salida: WS_PlanMasContratado.xml)")]
    public PlanBE ObtenerPlanMasContratado(string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return null;
        return new EstadisticaBLL().ObtenerPlanMasContratado(Desde, Hasta);
    }
}
