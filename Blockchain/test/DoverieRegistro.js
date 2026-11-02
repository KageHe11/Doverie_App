const { expect } = require("chai");
const { ethers } = require("hardhat");
const { time } = require("@nomicfoundation/hardhat-network-helpers");

describe("DoverieRegistro", function () {
  const HASH = "0x" + "ab".repeat(32);

  async function desplegar() {
    const [propietario, otro] = await ethers.getSigners();
    const Registro = await ethers.getContractFactory("DoverieRegistro");
    const registro = await Registro.deploy();
    return { registro, propietario, otro };
  }

  it("guarda como propietario a quien lo despliega", async function () {
    const { registro, propietario } = await desplegar();
    expect(await registro.propietario()).to.equal(propietario.address);
  });

  it("registra un hash con la fecha del bloque y emite el evento", async function () {
    const { registro } = await desplegar();
    const tx = await registro.registrar(HASH);
    const bloque = await ethers.provider.getBlock((await tx.wait()).blockNumber);
    await expect(tx).to.emit(registro, "DocumentoRegistrado").withArgs(HASH, bloque.timestamp);
    expect(await registro.consultar(HASH)).to.equal(bloque.timestamp);
  });

  it("devuelve 0 para un hash que nunca se registró", async function () {
    const { registro } = await desplegar();
    expect(await registro.consultar(HASH)).to.equal(0);
  });

  it("conserva la fecha original si el mismo hash se registra de nuevo", async function () {
    const { registro } = await desplegar();
    await registro.registrar(HASH);
    const original = await registro.consultar(HASH);
    await time.increase(3600);
    await expect(registro.registrar(HASH)).to.emit(registro, "DocumentoRegistrado").withArgs(HASH, original);
    expect(await registro.consultar(HASH)).to.equal(original);
  });

  it("rechaza registros de otra cuenta", async function () {
    const { registro, otro } = await desplegar();
    await expect(registro.connect(otro).registrar(HASH)).to.be.revertedWithCustomError(registro, "NoAutorizado");
  });

  it("rechaza el hash vacío", async function () {
    const { registro } = await desplegar();
    await expect(registro.registrar(ethers.ZeroHash)).to.be.revertedWithCustomError(registro, "HashVacio");
  });
});
