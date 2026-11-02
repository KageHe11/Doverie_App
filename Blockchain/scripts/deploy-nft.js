const { desplegar } = require("./desplegar");

// NFT de utilidad de los documentos (CU-001-007 y CU-001-013).
desplegar("DoverieNFT", "BlockchainContratoNFT").catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
