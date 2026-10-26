namespace BE
{
    /// Resultado de CU-001-006 Verificar Integridad: el hash del archivo
    /// actual contra el sello registrado en la blockchain.
    public class VerificacionDocumentoBE
    {
        public bool Integro { get; set; }
        public string HashActual { get; set; }
        public SelloBlockchainBE Sello { get; set; }
    }
}
