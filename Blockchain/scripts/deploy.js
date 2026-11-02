const { desplegar } = require("./desplegar");

// Registro de hashes de documentos (Proof of Existence).
desplegar("DoverieRegistro", "BlockchainContrato").catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
