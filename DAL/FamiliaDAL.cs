using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class FamiliaDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Familia_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(FamiliaBE Pfamilia)
        {
            DAO Dao = new DAO();
            Pfamilia.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pfamilia.Id },
                { "@Nombre", Pfamilia.Nombre }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Familia_Registrar", Parametros, CommandType.StoredProcedure);
            foreach (ComponentePermiso Hijo in Pfamilia.Hijos)
            {
                InsertarComponente(Pfamilia.Id, Hijo);
            }
        }
        public void ActualizarDatos(FamiliaBE Pfamilia)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pfamilia.Id },
                { "@Nombre", Pfamilia.Nombre }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Familia_ActualizarDatos", Parametros, CommandType.StoredProcedure);
            List<int> ComponentesActuales = ObtenerComponentesActuales(Pfamilia.Id);
            List<int> ComponentesNuevos = new List<int>();
            foreach (ComponentePermiso Hijo in Pfamilia.Hijos)
            {
                ComponentesNuevos.Add(Hijo.Id);
            }
            foreach (ComponentePermiso Hijo in Pfamilia.Hijos)
            {
                if (!ComponentesActuales.Contains(Hijo.Id))
                {
                    InsertarComponente(Pfamilia.Id, Hijo);
                }
            }
            foreach (int IdActual in ComponentesActuales)
            {
                if (!ComponentesNuevos.Contains(IdActual))
                {
                    EliminarComponente(Pfamilia.Id, IdActual);
                }
            }
        }
        /// Perfiles que tienen asignada la Familia + Familias que la contienen.
        public int ContarUsos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Familia_ContarUsos", Parametros, CommandType.StoredProcedure));
        }
        public bool ExistePorNombre(string Pnombre, int PidExcluir)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Nombre", Pnombre },
                { "@IdExcluir", PidExcluir }
            };
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure("SP_Familia_ExistePorNombre", Parametros, CommandType.StoredProcedure)) > 0;
        }
        public void Eliminar(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Familia_Eliminar", Parametros, CommandType.StoredProcedure);
        }
        public List<FamiliaBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Familia_Listar", null, CommandType.StoredProcedure);
            List<FamiliaBE> Familias = new List<FamiliaBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Familias.Add(new FamiliaBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    Nombre = Fila["Nombre"].ToString()
                });
            }
            return Familias;
        }
        public Dictionary<int, ComponentePermiso> ArmarArbol()
        {
            DAO Dao = new DAO();
            Dictionary<int, PermisoBE> Permisos = new Dictionary<int, PermisoBE>();
            PermisoDAL PermisoDal = new PermisoDAL();
            foreach (PermisoBE Permiso in PermisoDal.Listar())
            {
                Permisos[Permiso.Id] = Permiso;
            }
            Dictionary<int, FamiliaBE> Familias = new Dictionary<int, FamiliaBE>();
            DataSet DsFamilias = Dao.ExecuteDataSetStoredProcedure("SP_Familia_Listar", null, CommandType.StoredProcedure);
            foreach (DataRow Fila in DsFamilias.Tables[0].Rows)
            {
                FamiliaBE Familia = new FamiliaBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    Nombre = Fila["Nombre"].ToString()
                };
                Familias[Familia.Id] = Familia;
            }
            DataSet DsComponentes = Dao.ExecuteDataSetStoredProcedure("SP_FamiliaComponente_Listar", null, CommandType.StoredProcedure);
            foreach (DataRow Fila in DsComponentes.Tables[0].Rows)
            {
                int FamiliaId = Convert.ToInt32(Fila["FamiliaId"]);
                FamiliaBE FamiliaPadre = Familias[FamiliaId];

                if (Fila["PermisoId"] != DBNull.Value)
                {
                    int PermisoId = Convert.ToInt32(Fila["PermisoId"]);
                    FamiliaPadre.AgregarHijo(Permisos[PermisoId]);
                }
                else
                {
                    int SubFamiliaId = Convert.ToInt32(Fila["SubFamiliaId"]);
                    FamiliaPadre.AgregarHijo(Familias[SubFamiliaId]);
                }
            }
            // El diccionario final solo necesita tener las Familias (nadie externo
            // busca un Permiso suelto por Id acá — PerfilDAL, por ejemplo, solo
            // busca Familias, ya que PerfilFamilia únicamente vincula Perfiles con Familias).
            Dictionary<int, ComponentePermiso> Nodos = new Dictionary<int, ComponentePermiso>();
            foreach (var Par in Familias)
            {
                Nodos[Par.Key] = Par.Value;
            }
            return Nodos;
        }
        private List<int> ObtenerComponentesActuales(int FamiliaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@FamiliaId", FamiliaId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_FamiliaComponente_ListarPorFamilia", Parametros, CommandType.StoredProcedure);
            List<int> Ids = new List<int>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                if (Fila["PermisoId"] != DBNull.Value)
                    Ids.Add(Convert.ToInt32(Fila["PermisoId"]));
                else
                    Ids.Add(Convert.ToInt32(Fila["SubFamiliaId"]));
            }
            return Ids;
        }
        private void InsertarComponente(int FamiliaId, ComponentePermiso Hijo)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@FamiliaId", FamiliaId },
                { "@PermisoId", Hijo.EsHoja ? (object)Hijo.Id : DBNull.Value },
                { "@SubFamiliaId", Hijo.EsHoja ? (object)DBNull.Value : Hijo.Id }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_FamiliaComponente_Registrar", Parametros, CommandType.StoredProcedure);
        }
        private void EliminarComponente(int FamiliaId, int IdHijo)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@FamiliaId", FamiliaId },
                { "@IdHijo", IdHijo }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_FamiliaComponente_Eliminar", Parametros, CommandType.StoredProcedure);
        }
    }
}
