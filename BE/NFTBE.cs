using System;

namespace BE
{
    /// NFT de utilidad de un documento (CU-001-007 y CU-001-013): un token
    /// que certifica el hash del documento en la red blockchain.
    public class NFTBE
    {
        /// Lo que se guarda en Documento.NFTId: "<contrato>:<token>", o
        /// "local:<token>" para el registro local.
        public string Identificador { get; set; }
        public string Contrato { get; set; }
        public long TokenId { get; set; }
        /// Hash del documento que certifica el token.
        public string Hash { get; set; }
        public DateTime FechaEmision { get; set; }
        /// Wallet dueña del token (la de Doverie).
        public string Propietario { get; set; }
        /// Red del token ("Ethereum Sepolia", o null para el registro local).
        public string Red { get; set; }
        /// Transacción que lo emitió; solo se conoce al generarlo.
        public string Transaccion { get; set; }
        public string UrlExplorador { get; set; }
        /// Lo calcula la BLL al consultar: el hash del token coincide con el
        /// del documento.
        public bool Vigente { get; set; }
    }
}
