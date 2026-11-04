using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// Preferencias de notificación (Notificacion / NotificacionEvento) y
    /// bandeja de notificaciones recibidas (NotificacionRecibida).
    public class NotificacionDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Notificacion_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        /// Id de la configuración del Usuario, o null si nunca la guardó.
        public int? ObtenerIdPorUsuario(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Notificacion_ObtenerPorUsuario", Parametros, CommandType.StoredProcedure);
            return Resultado == null || Resultado == DBNull.Value ? (int?)null : Convert.ToInt32(Resultado);
        }
        public int Registrar(int PusuarioId)
        {
            DAO Dao = new DAO();
            int Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Id },
                { "@UsuarioId", PusuarioId }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Notificacion_Registrar", Parametros, CommandType.StoredProcedure);
            return Id;
        }
        /// Pares (Evento, Canal) habilitados en una configuración.
        public List<KeyValuePair<string, string>> ListarEventos(int PnotificacionId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@NotificacionId", PnotificacionId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_NotificacionEvento_ListarPorNotificacion", Parametros, CommandType.StoredProcedure);
            List<KeyValuePair<string, string>> Eventos = new List<KeyValuePair<string, string>>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Eventos.Add(new KeyValuePair<string, string>(Fila["Evento"].ToString(), Fila["Canal"].ToString()));
            }
            return Eventos;
        }
        public void EliminarEventos(int PnotificacionId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@NotificacionId", PnotificacionId } };
            Dao.ExecuteNonQueryStoredProcedure("SP_NotificacionEvento_Eliminar", Parametros, CommandType.StoredProcedure);
        }
        public void RegistrarEvento(int PnotificacionId, string Pevento, string Pcanal)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@NotificacionId", PnotificacionId },
                { "@Evento", Pevento },
                { "@Canal", Pcanal }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_NotificacionEvento_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public void RegistrarRecibida(int PusuarioId, string Pevento, string Pdetalle)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@UsuarioId", PusuarioId },
                { "@Evento", Pevento },
                { "@Detalle", (object)CifradoDatos.Cifrar(Pdetalle) ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_NotificacionRecibida_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public List<NotificacionRecibidaBE> ListarRecibidas(int PusuarioId, int Pcantidad)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@UsuarioId", PusuarioId },
                { "@Cantidad", Pcantidad }
            };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_NotificacionRecibida_ListarPorUsuario", Parametros, CommandType.StoredProcedure);
            List<NotificacionRecibidaBE> Recibidas = new List<NotificacionRecibidaBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Recibidas.Add(new NotificacionRecibidaBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                    Evento = Fila["Evento"].ToString(),
                    Detalle = Fila["Detalle"] == DBNull.Value ? null : CifradoDatos.Descifrar(Fila["Detalle"].ToString()),
                    Fecha = Convert.ToDateTime(Fila["Fecha"]),
                    Leida = Convert.ToBoolean(Fila["Leida"])
                });
            }
            return Recibidas;
        }
        public int ContarNoLeidas(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_NotificacionRecibida_ContarNoLeidas", Parametros, CommandType.StoredProcedure));
        }
        public void MarcarLeidas(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            Dao.ExecuteNonQueryStoredProcedure("SP_NotificacionRecibida_MarcarLeidas", Parametros, CommandType.StoredProcedure);
        }
    }
}
