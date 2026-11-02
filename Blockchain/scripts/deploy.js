const { ethers, network } = require("hardhat");

// Despliega DoverieRegistro con la cuenta configurada y muestra los valores
// que van en AppSettings.config de Doverie.
async function main() {
  const [cuenta] = await ethers.getSigners();
  const saldo = await ethers.provider.getBalance(cuenta.address);
  console.log(`Red: ${network.name}`);
  console.log(`Cuenta: ${cuenta.address} (saldo ${ethers.formatEther(saldo)} ETH)`);

  const Registro = await ethers.getContractFactory("DoverieRegistro");
  const registro = await Registro.deploy();
  await registro.waitForDeployment();
  const direccion = await registro.getAddress();
  const despliegue = registro.deploymentTransaction();

  console.log(`Contrato desplegado en: ${direccion}`);
  console.log(`Transacción: ${despliegue.hash}`);
  console.log("");
  console.log("En AppSettings.config de Doverie:");
  console.log(`  <add key="BlockchainContrato" value="${direccion}" />`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
