// SPDX-License-Identifier: MIT
pragma solidity 0.8.28;

/// @title Registro de documentos de Doverie (Proof of Existence)
/// @notice Guarda el hash SHA-256 de cada documento junto con la fecha del
/// bloque en que se registró. Sirve para demostrar que un archivo existía
/// en esa fecha y que no fue modificado desde entonces.
/// @dev Solo la cuenta de Doverie que desplegó el contrato puede registrar.
/// Un hash ya registrado conserva su fecha original: volver a subir el mismo
/// archivo no la pisa, pero igual emite el evento para que la nueva
/// transacción quede asociada a ese hash.
contract DoverieRegistro {
    address public immutable propietario;

    /// hash del documento => fecha (timestamp UNIX) del primer registro
    mapping(bytes32 => uint256) private registros;

    event DocumentoRegistrado(bytes32 indexed hash, uint256 fecha);

    error NoAutorizado();
    error HashVacio();

    constructor() {
        propietario = msg.sender;
    }

    function registrar(bytes32 hash) external {
        if (msg.sender != propietario) revert NoAutorizado();
        if (hash == bytes32(0)) revert HashVacio();
        uint256 fecha = registros[hash];
        if (fecha == 0) {
            fecha = block.timestamp;
            registros[hash] = fecha;
        }
        emit DocumentoRegistrado(hash, fecha);
    }

    /// @return fecha del primer registro del hash, o 0 si nunca se registró
    function consultar(bytes32 hash) external view returns (uint256) {
        return registros[hash];
    }
}
