using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class PlanDAL
    {
        public List<PlanBE> ListarActivos()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Plan_ListarActivos", null, CommandType.StoredProcedure);
            List<PlanBE> Planes = new List<PlanBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Planes.Add(MapearFila(Fila));
            }
            return Planes;
        }
        public PlanBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Plan_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        internal static PlanBE MapearFila(DataRow Fila)
        {
            return new PlanBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                Nombre = Fila["Nombre"].ToString(),
                Activo = Convert.ToBoolean(Fila["Activo"]),
                Tipo = Fila["Tipo"].ToString(),
                PrecioMensual = Convert.ToDecimal(Fila["PrecioMensual"]),
                Meses = Convert.ToInt32(Fila["Meses"]),
                MaxUsuarios = Fila["MaxUsuarios"] == DBNull.Value ? (int?)null : Convert.ToInt32(Fila["MaxUsuarios"])
            };
        }
    }
}
