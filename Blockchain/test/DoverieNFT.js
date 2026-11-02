const { expect } = require("chai");
const { ethers } = require("hardhat");

describe("DoverieNFT", function () {
  const HASH = "0x" + "ab".repeat(32);
  const OTRO_HASH = "0x" + "cd".repeat(32);

  async function desplegar() {
    const [propietario, otro] = await ethers.getSigners();
    const NFT = await ethers.getContractFactory("DoverieNFT");
    const nft = await NFT.deploy();
    return { nft, propietario, otro };
  }

  it("emite el token 1 en la wallet de Doverie con el hash y la fecha", async function () {
    const { nft, propietario } = await desplegar();
    const tx = await nft.generar(HASH);
    const bloque = await ethers.provider.getBlock((await tx.wait()).blockNumber);
    await expect(tx).to.emit(nft, "NFTGenerado").withArgs(1, HASH, bloque.timestamp);
    expect(await nft.ownerOf(1)).to.equal(propietario.address);
    const [hash, fecha, dueno] = await nft.consultar(1);
    expect(hash).to.equal(HASH);
    expect(fecha).to.equal(bloque.timestamp);
    expect(dueno).to.equal(propietario.address);
    expect(await nft.tokenDeHash(HASH)).to.equal(1);
    expect(await nft.totalGenerados()).to.equal(1);
  });

  it("numera los tokens en orden", async function () {
    const { nft } = await desplegar();
    await nft.generar(HASH);
    await nft.generar(OTRO_HASH);
    expect(await nft.tokenDeHash(OTRO_HASH)).to.equal(2);
  });

  it("no permite tokenizar dos veces el mismo contenido", async function () {
    const { nft } = await desplegar();
    await nft.generar(HASH);
    await expect(nft.generar(HASH)).to.be.revertedWithCustomError(nft, "HashYaTokenizado").withArgs(HASH, 1);
  });

  it("solo Doverie puede emitir", async function () {
    const { nft, otro } = await desplegar();
    await expect(nft.connect(otro).generar(HASH)).to.be.revertedWithCustomError(nft, "OwnableUnauthorizedAccount");
  });

  it("rechaza el hash vacío", async function () {
    const { nft } = await desplegar();
    await expect(nft.generar(ethers.ZeroHash)).to.be.revertedWithCustomError(nft, "HashVacio");
  });

  it("consultar un token inexistente revierte", async function () {
    const { nft } = await desplegar();
    await expect(nft.consultar(7)).to.be.revertedWithCustomError(nft, "ERC721NonexistentToken");
  });

  it("arma los metadatos en la cadena con el hash del documento", async function () {
    const { nft } = await desplegar();
    await nft.generar(HASH);
    const uri = await nft.tokenURI(1);
    expect(uri.startsWith("data:application/json;base64,")).to.equal(true);
    const json = JSON.parse(Buffer.from(uri.split(",")[1], "base64").toString("utf8"));
    expect(json.name).to.equal("Doverie #1");
    expect(json.attributes[0].value).to.equal(HASH);
  });
});
