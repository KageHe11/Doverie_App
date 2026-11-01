namespace BE
{
    /// Plan de suscripción (tabla Plan): un nivel de servicio (Nombre:
    /// Plata, Oro, Platino) en una modalidad de pago (Tipo: Mensual,
    /// Trimestral, Semestral, Anual).
    public class PlanBE
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public string Tipo { get; set; }
        /// Precio por mes en ARS, ya con el descuento de la modalidad.
        public decimal PrecioMensual { get; set; }
        /// Meses que se abonan por adelantado en cada período.
        public int Meses { get; set; }
        /// Usuarios de la cuenta, incluido el titular. Null = sin límite.
        public int? MaxUsuarios { get; set; }

        /// Lo que se cobra en cada período.
        public decimal Total
        {
            get { return PrecioMensual * Meses; }
        }
    }
}
