// SPDX-License-Identifier: MIT
pragma solidity 0.8.28;

import {ERC721} from "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import {Ownable} from "@openzeppelin/contracts/access/Ownable.sol";
import {Base64} from "@openzeppelin/contracts/utils/Base64.sol";
import {Strings} from "@openzeppelin/contracts/utils/Strings.sol";

/// @title NFT de utilidad de Doverie (CU-001-007 y CU-001-013)
/// @notice Cada token es un certificado de autenticidad de un documento:
/// guarda su hash SHA-256 y la fecha de emisión. Un mismo contenido no
/// puede tokenizarse dos veces, así que cada NFT es irrepetible.
/// @dev Solo Doverie (el propietario del contrato) emite tokens, y quedan
/// en su wallet: los clientes de Doverie no necesitan tener una wallet.
/// Los metadatos (tokenURI) se arman en la propia cadena, sin servidores
/// externos.
contract DoverieNFT is ERC721, Ownable {
    using Strings for uint256;

    struct Certificado {
        bytes32 hash;
        uint64 fecha;
    }

    uint256 public totalGenerados;
    mapping(uint256 => Certificado) private certificados;
    /// hash del documento => token que lo certifica (0 si no tiene)
    mapping(bytes32 => uint256) public tokenDeHash;

    event NFTGenerado(uint256 indexed tokenId, bytes32 indexed hash, uint256 fecha);

    error HashVacio();
    error HashYaTokenizado(bytes32 hash, uint256 tokenId);

    constructor() ERC721("Doverie Documento", "DOVDOC") Ownable(msg.sender) {}

    function generar(bytes32 hash) external onlyOwner returns (uint256 tokenId) {
        if (hash == bytes32(0)) revert HashVacio();
        uint256 existente = tokenDeHash[hash];
        if (existente != 0) revert HashYaTokenizado(hash, existente);
        tokenId = ++totalGenerados;
        certificados[tokenId] = Certificado(hash, uint64(block.timestamp));
        tokenDeHash[hash] = tokenId;
        _mint(owner(), tokenId);
        emit NFTGenerado(tokenId, hash, block.timestamp);
    }

    /// @notice Datos del certificado. Revierte si el token no existe.
    function consultar(uint256 tokenId) external view returns (bytes32 hash, uint256 fecha, address propietario) {
        propietario = ownerOf(tokenId);
        Certificado memory c = certificados[tokenId];
        return (c.hash, c.fecha, propietario);
    }

    function tokenURI(uint256 tokenId) public view override returns (string memory) {
        _requireOwned(tokenId);
        Certificado memory c = certificados[tokenId];
        string memory atributos = string.concat(
            '[{"trait_type":"Hash SHA-256","value":"', uint256(c.hash).toHexString(32),
            '"},{"display_type":"date","trait_type":"Fecha de emision","value":', uint256(c.fecha).toString(), "}]"
        );
        string memory json = string.concat(
            '{"name":"Doverie #', tokenId.toString(),
            '","description":"Certificado de autenticidad de un documento registrado en Doverie. ',
            'El hash SHA-256 identifica el contenido exacto del documento.","attributes":', atributos, "}"
        );
        return string.concat("data:application/json;base64,", Base64.encode(bytes(json)));
    }
}
