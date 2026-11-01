using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class SuscripcionDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Suscripcion_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(SuscripcionBE Psuscripcion)
        {
            DAO Dao = new DAO();
            Psuscripcion.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Psuscripcion.Id },
                { "@UsuarioId", Psuscripcion.UsuarioId },
                { "@PlanId", Psuscripcion.PlanId },
                { "@Estado", Psuscripcion.Estado },
                { "@FechaInicio", Psuscripcion.FechaInicio },
                { "@FechaRenovacion", Psuscripcion.FechaRenovacion }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Suscripcion_Registrar", Parametros, CommandType.StoredProcedure);
        }
        /// Suscripción Activa o Cancelada más reciente de la cuenta, o null.
        public SuscripcionBE ObtenerEnCurso(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Suscripcion_ObtenerEnCurso", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public SuscripcionBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Suscripcion_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public void ActualizarEstado(int Pid, int Pestado)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@Estado", Pestado }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Suscripcion_ActualizarEstado", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarRenovacion(int Pid, DateTime PfechaRenovacion)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@FechaRenovacion", PfechaRenovacion }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Suscripcion_ActualizarRenovacion", Parametros, CommandType.StoredProcedure);
        }
        private SuscripcionBE MapearFila(DataRow Fila)
        {
            return new SuscripcionBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                PlanId = Convert.ToInt32(Fila["PlanId"]),
                Estado = Convert.ToInt32(Fila["Estado"]),
                FechaInicio = Convert.ToDateTime(Fila["FechaInicio"]),
                FechaRenovacion = Convert.ToDateTime(Fila["FechaRenovacion"])
            };
        }
    }
}
