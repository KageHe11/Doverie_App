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
        /// Red donde está la transacción ("Ethereum Sepolia", o null para
        /// el registro local), su bloque y el enlace a un explorador público.
        public string Red { get; set; }
        public long? Bloque { get; set; }
        public string UrlExplorador { get; set; }
    }
}
