using BE;

namespace SERVICE
{
    /// Contrato del registro de hashes en blockchain (sellado de tiempo e
    /// integridad). La implementación actual es un registro local
    /// encadenado (RegistroBlockchainLocal); la de Nethereum contra un
    /// smart contract se agrega como otra implementación.
    public interface IRegistroBlockchain
    {
        /// Registra el hash y devuelve el sello con la transacción generada.
        SelloBlockchainBE Registrar(string hash);
        /// Devuelve el sello de una transacción, o null si no existe.
        SelloBlockchainBE Consultar(string transaccion);
    }
}
