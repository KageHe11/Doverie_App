using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace DAL
{
    public class BitacoraDAL
    {
        public void Registrar(string accion, string detalle, int? usuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Accion", accion },
                { "@Detalle", (object)detalle ?? DBNull.Value },
                { "@UsuarioId", usuarioId.HasValue ? (object)usuarioId.Value : DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Bitacora_Registrar", Parametros, CommandType.StoredProcedure);
        }
        /// Filas que exceden las 30 más recientes (para archivar).
        public List<BitacoraBE> ObtenerExcedente()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Bitacora_ObtenerExcedente", null, CommandType.StoredProcedure);
            List<BitacoraBE> Eventos = new List<BitacoraBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Eventos.Add(MapearFila(Fila));
            }
            return Eventos;
        }
        public void EliminarPorId(int id)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", id } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Bitacora_EliminarPorId", Parametros, CommandType.StoredProcedure);
        }
        public List<BitacoraBE> ListarUltimos30()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Bitacora_ListarUltimos30", null, CommandType.StoredProcedure);
            List<BitacoraBE> Eventos = new List<BitacoraBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Eventos.Add(MapearFila(Fila));
            }
            return Eventos;
        }
        public List<string> ListarAcciones()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Bitacora_ListarAcciones", null, CommandType.StoredProcedure);
            List<string> Acciones = new List<string>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Acciones.Add(Fila["Accion"].ToString());
            }
            return Acciones;
        }
        private BitacoraBE MapearFila(DataRow Fila)
        {
            return new BitacoraBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                Accion = Fila["Accion"].ToString(),
                Fecha = Convert.ToDateTime(Fila["Fecha"]),
                Detalle = Fila["Detalle"] == DBNull.Value ? null : Fila["Detalle"].ToString(),
                UsuarioId = Fila["UsuarioId"] == DBNull.Value ? (int?)null : Convert.ToInt32(Fila["UsuarioId"]),
                NombreUsuario = Fila["NombreUsuario"] == DBNull.Value ? "(sistema)" : Fila["NombreUsuario"].ToString()
            };
        }
    }
}