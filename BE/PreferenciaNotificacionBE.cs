namespace BE
{
    /// Preferencia de un Usuario para un evento: si lo quiere recibir y por
    /// qué canales.
    public class PreferenciaNotificacionBE
    {
        public string Evento { get; set; }
        public string Categoria { get; set; }
        public bool Habilitado { get; set; }
        public bool Email { get; set; }
        public bool Push { get; set; }
    }
}
