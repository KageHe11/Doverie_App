namespace SERVICE
{
    /// Strategy (patrón Strategy, RC-004 Traducir): contrato común para
    /// traducir una clave de texto de la interfaz. Devuelve null si la
    /// estrategia no conoce la clave.
    public interface ITraduccion
    {
        string Traducir(string clave);
    }
}
