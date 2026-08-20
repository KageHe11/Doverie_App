using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PerfilDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Perfil_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(PerfilBE Pperfil)
        {
            DAO Dao = new DAO();
            Pperfil.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pperfil.Id },
                { "@NombrePerfil", Pperfil.NombrePerfil }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Perfil_Registrar", Parametros, CommandType.StoredProcedure);
            foreach (FamiliaBE Familia in Pperfil.Familias)
            {
                InsertarFamiliaPerfil(Pperfil.Id, Familia.Id);
            }
        }
        public void ActualizarPermisos(PerfilBE Pperfil)
        {
            List<int> FamiliasActuales = ObtenerFamiliasActuales(Pperfil.Id);
            List<int> FamiliasNuevas = new List<int>();
            foreach (FamiliaBE Familia in Pperfil.Familias) FamiliasNuevas.Add(Familia.Id);
            foreach (FamiliaBE Familia in Pperfil.Familias)
            {
                if (!FamiliasActuales.Contains(Familia.Id))
                    InsertarFamiliaPerfil(Pperfil.Id, Familia.Id);
            }
            foreach (int IdActual in FamiliasActuales)
            {
                if (!FamiliasNuevas.Contains(IdActual))
                    EliminarFamiliaPerfil(Pperfil.Id, IdActual);
            }
        }
        public void Eliminar(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Perfil_Eliminar", Parametros, CommandType.StoredProcedure);
        }
        public List<PerfilBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Perfil_Listar", null, CommandType.StoredProcedure);
            List<PerfilBE> Perfiles = new List<PerfilBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Perfiles.Add(new PerfilBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    NombrePerfil = Fila["NombrePerfil"].ToString()
                });
            }
            return Perfiles;
        }
        public PerfilBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Perfil_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            PerfilBE Perfil = new PerfilBE
            {
                Id = Convert.ToInt32(Ds.Tables[0].Rows[0]["Id"]),
                NombrePerfil = Ds.Tables[0].Rows[0]["NombrePerfil"].ToString()
            };
            FamiliaDAL FamiliaDal = new FamiliaDAL();
            Dictionary<int, ComponentePermiso> Arbol = FamiliaDal.ArmarArbol();
            foreach (int IdFamilia in ObtenerFamiliasActuales(Pid))
            {
                Perfil.Familias.Add((FamiliaBE)Arbol[IdFamilia]);
            }
            return Perfil;
        }
        public bool ExistePorNombre(string Pnombre)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@NombrePerfil", Pnombre } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Perfil_ExistePorNombre", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        private List<int> ObtenerFamiliasActuales(int PerfilId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@PerfilId", PerfilId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_PerfilFamilia_ListarPorPerfil", Parametros, CommandType.StoredProcedure);
            List<int> Ids = new List<int>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
                Ids.Add(Convert.ToInt32(Fila["FamiliaId"]));
            return Ids;
        }
        private void InsertarFamiliaPerfil(int PerfilId, int FamiliaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@PerfilId", PerfilId },
                { "@FamiliaId", FamiliaId }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_PerfilFamilia_Registrar", Parametros, CommandType.StoredProcedure);
        }
        private void EliminarFamiliaPerfil(int PerfilId, int FamiliaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@PerfilId", PerfilId },
                { "@FamiliaId", FamiliaId }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_PerfilFamilia_Eliminar", Parametros, CommandType.StoredProcedure);
        }
    }
}
