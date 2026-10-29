using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class DocumentoCompartidoDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_DocumentoCompartido_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(DocumentoCompartidoBE Pcompartido, string PtokenHash)
        {
            DAO Dao = new DAO();
            Pcompartido.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pcompartido.Id },
                { "@DocumentoId", Pcompartido.DocumentoId },
                { "@UsuarioId", Pcompartido.UsuarioId },
                { "@EmailDestinatario", Pcompartido.EmailDestinatario },
                { "@TokenHash", PtokenHash },
                { "@FechaCreacion", Pcompartido.FechaCreacion },
                { "@FechaVencimiento", Pcompartido.FechaVencimiento }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_DocumentoCompartido_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public DocumentoCompartidoBE ObtenerPorTokenHash(string PtokenHash)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@TokenHash", PtokenHash } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_DocumentoCompartido_ObtenerPorTokenHash", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public DocumentoCompartidoBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_DocumentoCompartido_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public List<DocumentoCompartidoBE> ListarPorDocumento(int PdocumentoId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@DocumentoId", PdocumentoId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_DocumentoCompartido_ListarPorDocumento", Parametros, CommandType.StoredProcedure);
            List<DocumentoCompartidoBE> Compartidos = new List<DocumentoCompartidoBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Compartidos.Add(MapearFila(Fila));
            }
            return Compartidos;
        }
        public void Revocar(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_DocumentoCompartido_Revocar", Parametros, CommandType.StoredProcedure);
        }
        public void RegistrarAcceso(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_DocumentoCompartido_RegistrarAcceso", Parametros, CommandType.StoredProcedure);
        }
        private DocumentoCompartidoBE MapearFila(DataRow Fila)
        {
            return new DocumentoCompartidoBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                DocumentoId = Convert.ToInt32(Fila["DocumentoId"]),
                UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                EmailDestinatario = Fila["EmailDestinatario"].ToString(),
                FechaCreacion = Convert.ToDateTime(Fila["FechaCreacion"]),
                FechaVencimiento = Convert.ToDateTime(Fila["FechaVencimiento"]),
                Revocado = Convert.ToBoolean(Fila["Revocado"]),
                CantidadAccesos = Convert.ToInt32(Fila["CantidadAccesos"]),
                UltimoAcceso = Fila["UltimoAcceso"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(Fila["UltimoAcceso"]),
                NombreUsuario = Fila["NombreUsuario"].ToString()
            };
        }
    }
}
