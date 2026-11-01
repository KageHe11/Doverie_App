using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class MedioPagoDAL
    {
        public List<MedioPagoBE> ListarActivos()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_MedioPago_ListarActivos", null, CommandType.StoredProcedure);
            List<MedioPagoBE> Medios = new List<MedioPagoBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Medios.Add(new MedioPagoBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    Nombre = Fila["Nombre"].ToString(),
                    Activo = Convert.ToBoolean(Fila["Activo"])
                });
            }
            return Medios;
        }
    }
}
