namespace BE
{
    /// Plan de suscripción (tabla Plan del diccionario de datos). Por ahora
    /// solo lo usa WS_Usuarios.ObtenerPlanMasContratado; la gestión de
    /// planes llega con los casos de uso de Suscripción.
    public class PlanBE
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public string Tipo { get; set; }
    }
}
