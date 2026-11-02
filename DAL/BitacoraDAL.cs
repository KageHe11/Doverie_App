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
        /// Eventos con más de "dias" días, para pasarlos al archivo histórico.
        public List<BitacoraBE> ObtenerVencidos(int dias)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Dias", dias } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Bitacora_ObtenerVencidos", Parametros, CommandType.StoredProcedure);
            List<BitacoraBE> Eventos = new List<BitacoraBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Eventos.Add(MapearFila(Fila));
            }
            return Eventos;
        }
        /// Saca de la tabla un evento vencido que ya quedó en el archivo.
        public void QuitarArchivado(int id, int dias)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", id }, { "@Dias", dias } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Bitacora_QuitarArchivado", Parametros, CommandType.StoredProcedure);
        }
        public void EliminarLogico(int id, int usuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", id }, { "@UsuarioId", usuarioId } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Bitacora_EliminarLogico", Parametros, CommandType.StoredProcedure);
        }
        public void Restaurar(int id)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", id } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Bitacora_Restaurar", Parametros, CommandType.StoredProcedure);
        }
        /// Los eventos de la tabla (últimos 30 días), eliminados incluidos.
        public List<BitacoraBE> ListarRecientes()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Bitacora_ListarRecientes", null, CommandType.StoredProcedure);
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
                NombreUsuario = Fila["NombreUsuario"] == DBNull.Value ? "(sistema)" : Fila["NombreUsuario"].ToString(),
                Eliminado = Convert.ToBoolean(Fila["Eliminado"]),
                FechaEliminacion = Fila["FechaEliminacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(Fila["FechaEliminacion"]),
                NombreEliminadoPor = Fila["NombreEliminadoPor"] == DBNull.Value ? null : Fila["NombreEliminadoPor"].ToString()
            };
        }
    }
}