const { ethers, network } = require("hardhat");

// Despliega un contrato con la cuenta configurada y muestra la clave de
// AppSettings.config de Doverie donde va su dirección.
async function desplegar(nombreContrato, claveAppSettings) {
  const [cuenta] = await ethers.getSigners();
  const saldo = await ethers.provider.getBalance(cuenta.address);
  console.log(`Red: ${network.name}`);
  console.log(`Cuenta: ${cuenta.address} (saldo ${ethers.formatEther(saldo)} ETH)`);

  const Fabrica = await ethers.getContractFactory(nombreContrato);
  const contrato = await Fabrica.deploy();
  await contrato.waitForDeployment();
  const direccion = await contrato.getAddress();

  console.log(`${nombreContrato} desplegado en: ${direccion}`);
  console.log(`Transacción: ${contrato.deploymentTransaction().hash}`);
  console.log("");
  console.log("En AppSettings.config de Doverie:");
  console.log(`  <add key="${claveAppSettings}" value="${direccion}" />`);
}

module.exports = { desplegar };
