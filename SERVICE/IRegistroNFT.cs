using System;
using BE;

namespace SERVICE
{
    /// Contrato de los NFT de utilidad de los documentos (CU-001-007 y
    /// CU-001-013). Se elige con la misma clave "Blockchain" de AppSettings
    /// que el registro de hashes: "Local" o "Nethereum".
    public interface IRegistroNFT
    {
        /// Emite el NFT que certifica el hash. Lanza NFTExistenteException si
        /// ese contenido ya tiene un NFT.
        NFTBE Generar(string hash);
        /// Lee el estado actual del NFT, o null si no existe.
        NFTBE Consultar(string identificador);
    }

    /// El contenido ya fue tokenizado: un NFT por contenido, aunque se haya
    /// subido como otro documento.
    public class NFTExistenteException : Exception
    {
        public NFTExistenteException(string mensaje) : base(mensaje) { }
    }
}
