require("@nomicfoundation/hardhat-toolbox");
require("dotenv").config();

// La URL del proveedor (Alchemy) y la clave privada de la wallet se leen de
// .env, que no se sube al repositorio. Ver .env.example.
const { SEPOLIA_RPC_URL, ETHERSCAN_API_KEY } = process.env;
// MetaMask exporta la clave sin el prefijo 0x; Hardhat lo necesita.
const PRIVATE_KEY = process.env.PRIVATE_KEY && !process.env.PRIVATE_KEY.startsWith("0x")
  ? "0x" + process.env.PRIVATE_KEY
  : process.env.PRIVATE_KEY;

module.exports = {
  solidity: {
    version: "0.8.28",
    settings: { optimizer: { enabled: true, runs: 200 } },
  },
  networks: {
    sepolia: {
      url: SEPOLIA_RPC_URL || "",
      accounts: PRIVATE_KEY ? [PRIVATE_KEY] : [],
      chainId: 11155111,
    },
  },
  etherscan: {
    apiKey: ETHERSCAN_API_KEY || "",
  },
};
