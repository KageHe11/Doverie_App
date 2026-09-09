using System;
using System.Collections.Generic;
using System.Data;
using BE;

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
        }
        public void ActualizarDatos(PerfilBE Pperfil)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pperfil.Id },
                { "@NombrePerfil", Pperfil.NombrePerfil }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Perfil_ActualizarDatos", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarEstado(PerfilBE Pperfil)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pperfil.Id },
                { "@Activo", Pperfil.Activo }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Perfil_ActualizarEstado", Parametros, CommandType.StoredProcedure);
        }
        public PerfilBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Perfil_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            DataRow Fila = Ds.Tables[0].Rows[0];
            PerfilBE Perfil = new PerfilBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                NombrePerfil = Fila["NombrePerfil"].ToString(),
                Activo = Convert.ToBoolean(Fila["Activo"])
            };
            FamiliaDAL FamiliaDal = new FamiliaDAL();
            Dictionary<int, ComponentePermiso> Arbol = FamiliaDal.ArmarArbol();
            foreach (int IdFamilia in ObtenerFamiliasActuales(Pid))
            {
                if (Arbol.ContainsKey(IdFamilia))
                {
                    Perfil.Familias.Add((FamiliaBE)Arbol[IdFamilia]);
                }
            }

            return Perfil;
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
                    NombrePerfil = Fila["NombrePerfil"].ToString(),
                    Activo = Convert.ToBoolean(Fila["Activo"])
                });
            }
            return Perfiles;
        }
        public List<int> ObtenerFamiliasActuales(int perfilId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@PerfilId", perfilId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_PerfilFamilia_ListarPorPerfil", Parametros, CommandType.StoredProcedure);

            List<int> Ids = new List<int>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Ids.Add(Convert.ToInt32(Fila["FamiliaId"]));
            }
            return Ids;
        }
        public void ActualizarPermisos(PerfilBE perfil)
        {
            List<int> FamiliasActuales = ObtenerFamiliasActuales(perfil.Id);
            List<int> FamiliasNuevas = new List<int>();
            foreach (FamiliaBE Familia in perfil.Familias)
            {
                FamiliasNuevas.Add(Familia.Id);
            }
            foreach (FamiliaBE Familia in perfil.Familias)
            {
                if (!FamiliasActuales.Contains(Familia.Id))
                {
                    InsertarFamiliaPerfil(perfil.Id, Familia.Id);
                }
            }
            foreach (int IdActual in FamiliasActuales)
            {
                if (!FamiliasNuevas.Contains(IdActual))
                {
                    EliminarFamiliaPerfil(perfil.Id, IdActual);
                }
            }
        }
        private void InsertarFamiliaPerfil(int perfilId, int familiaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@PerfilId", perfilId },
                { "@FamiliaId", familiaId }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_PerfilFamilia_Registrar", Parametros, CommandType.StoredProcedure);
        }
        private void EliminarFamiliaPerfil(int perfilId, int familiaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@PerfilId", perfilId },
                { "@FamiliaId", familiaId }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_PerfilFamilia_Eliminar", Parametros, CommandType.StoredProcedure);
        }
    }
}