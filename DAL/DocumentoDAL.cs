using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class DocumentoDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Documento_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(DocumentoBE Pdocumento)
        {
            DAO Dao = new DAO();
            Pdocumento.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pdocumento.Id },
                { "@UsuarioId", Pdocumento.UsuarioId },
                { "@Nombre", Pdocumento.Nombre },
                { "@UrlDocumento", Pdocumento.UrlDocumento },
                { "@Hash", Pdocumento.Hash },
                { "@TransaccionBlockchain", Pdocumento.TransaccionBlockchain },
                { "@FechaSubida", Pdocumento.FechaSubida }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Documento_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarEstado(int Pid, string Pestado)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@Estado", Pestado }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Documento_ActualizarEstado", Parametros, CommandType.StoredProcedure);
        }
        public List<DocumentoBE> ListarPorCuenta(int PcuentaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@CuentaId", PcuentaId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Documento_ListarPorCuenta", Parametros, CommandType.StoredProcedure);
            List<DocumentoBE> Documentos = new List<DocumentoBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Documentos.Add(MapearFila(Fila));
            }
            return Documentos;
        }
        public DocumentoBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Documento_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        private DocumentoBE MapearFila(DataRow Fila)
        {
            return new DocumentoBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                Nombre = Fila["Nombre"].ToString(),
                UrlDocumento = Fila["UrlDocumento"].ToString(),
                Hash = Fila["Hash"].ToString(),
                TransaccionBlockchain = Fila["TransaccionBlockchain"].ToString(),
                NFTId = Fila["NFTId"] == DBNull.Value ? null : Fila["NFTId"].ToString(),
                Estado = Fila["Estado"].ToString(),
                FechaSubida = Convert.ToDateTime(Fila["FechaSubida"]),
                NombreUsuario = Fila["NombreUsuario"].ToString(),
                CuentaId = Convert.ToInt32(Fila["CuentaId"])
            };
        }
    }
}
