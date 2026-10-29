using System;
using System.Web.Services;
using BLL;

/// WS-001 WS_Documentos.asmx: métricas de la gestión documental.
/// Lo consume Estadisticas.aspx. Fechas en formato yyyy-MM-dd.
[WebService(Namespace = "http://doverie.com/ws/", Description = "Métricas de documentos y verificaciones de integridad")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class WS_Documentos : ServicioProtegido
{
    [WebMethod(EnableSession = true, Description = "Cantidad total de documentos subidos en el período (salida: WS_DocumentosPorPeriodo.xml)")]
    public int ObtenerDocumentosPorPeriodo(string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return 0;
        return new EstadisticaBLL().ObtenerDocumentosPorPeriodo(Desde, Hasta);
    }

    [WebMethod(EnableSession = true, Description = "Cantidad total de verificaciones de integridad realizadas en el período (salida: WS_VerificacionesPorPeriodo.xml)")]
    public int ObtenerVerificacionesPorPeriodo(string desde, string hasta)
    {
        DateTime Desde, Hasta;
        if (!Preparar(desde, hasta, out Desde, out Hasta)) return 0;
        return new EstadisticaBLL().ObtenerVerificacionesPorPeriodo(Desde, Hasta);
    }
}
