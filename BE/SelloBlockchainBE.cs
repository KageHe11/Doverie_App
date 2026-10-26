using System;

namespace BE
{
    /// Registro de un hash en la red blockchain (Proof of Existence): qué
    /// hash se registró, en qué transacción y con qué sellado de tiempo.
    public class SelloBlockchainBE
    {
        public string Transaccion { get; set; }
        public string Hash { get; set; }
        public DateTime FechaSellado { get; set; }
    }
}
