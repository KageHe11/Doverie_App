using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PermisoDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Permiso_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(PermisoBE Ppermiso)
        {
            DAO Dao = new DAO();
            Ppermiso.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Ppermiso.Id },
                { "@Nombre", Ppermiso.Nombre },
                { "@Descripcion", Ppermiso.Descripcion }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Permiso_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarDatos(PermisoBE Ppermiso)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Ppermiso.Id },
                { "@Nombre", Ppermiso.Nombre },
                { "@Descripcion", Ppermiso.Descripcion }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Permiso_ActualizarDatos", Parametros, CommandType.StoredProcedure);
        }
        public void Eliminar(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Permiso_Eliminar", Parametros, CommandType.StoredProcedure);
        }
        public List<PermisoBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Permiso_Listar", null, CommandType.StoredProcedure);
            List<PermisoBE> Permisos = new List<PermisoBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Permisos.Add(MapearFila(Fila));
            }
            return Permisos;
        }
        public PermisoBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Permiso_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public bool ExistePorNombre(string Pnombre)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Nombre", Pnombre } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Permiso_ExistePorNombre", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        /// Familias que contienen el Permiso.
        public int ContarUsos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Permiso_ContarUsos", Parametros, CommandType.StoredProcedure));
        }
        public bool ExistePorNombreExcluyendo(string Pnombre, int PidExcluir)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Nombre", Pnombre },
                { "@IdExcluir", PidExcluir }
            };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Permiso_ExistePorNombreExcluyendo", Parametros, CommandType.StoredProcedure)) > 0;
        }
        private PermisoBE MapearFila(DataRow Fila)
        {
            return new PermisoBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                Nombre = Fila["Nombre"].ToString(),
                Descripcion = Fila["Descripcion"].ToString()
            };
        }
        public PermisoBE ObtenerPorNombre(string nombre)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Nombre", nombre } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Permiso_ObtenerPorNombre", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
    }
}
