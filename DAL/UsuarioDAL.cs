using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace DAL
{
    public class UsuarioDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Usuario_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(UsuarioBE Pusuario)
        {
            DAO Dao = new DAO();
            Pusuario.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pusuario.Id },
                { "@Nombre", Pusuario.Nombre },
                { "@Apellido", Pusuario.Apellido },
                { "@NombreUsuario", Pusuario.NombreUsuario },
                { "@Contrasena", Pusuario.Contrasena },
                { "@Gmail", Pusuario.Gmail },
                { "@Dni", Pusuario.Dni },
                { "@Telefono", Pusuario.Telefono },
                { "@Empleado", Pusuario.Empleado },
                { "@Empresa", Pusuario.Empresa },
                { "@Independiente", Pusuario.Independiente },
                { "@Local", Pusuario.Local },
                { "@PerfilId", Pusuario.Perfil.Id },
                { "@EmpresaId", Pusuario.EmpresaId.HasValue ? (object)Pusuario.EmpresaId.Value : DBNull.Value },
                { "@IdiomaId", DBNull.Value },
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Usuario_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarDatos(UsuarioBE Pusuario)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pusuario.Id },
                { "@Nombre", Pusuario.Nombre },
                { "@Apellido", Pusuario.Apellido },
                { "@Contrasena", Pusuario.Contrasena },
                { "@Gmail", Pusuario.Gmail },
                { "@Dni", Pusuario.Dni },
                { "@Telefono", Pusuario.Telefono },
                { "@Empleado", Pusuario.Empleado },
                { "@Empresa", Pusuario.Empresa },
                { "@Independiente", Pusuario.Independiente },
                { "@Local", Pusuario.Local },
                { "@Suspendido", Pusuario.Suspendido },
                { "@Motivo", (object)Pusuario.Motivo ?? DBNull.Value },
                { "@Intentos", Pusuario.Intentos },
                { "@PerfilId", Pusuario.Perfil.Id },
                { "@EmpresaId", Pusuario.EmpresaId.HasValue ? (object)Pusuario.EmpresaId.Value : DBNull.Value },
                { "@IdiomaId", DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Usuario_ActualizarDatos", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarEstado(UsuarioBE Pusuario)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pusuario.Id },
                { "@Suspendido", Pusuario.Suspendido },
                { "@Motivo", (object)Pusuario.Motivo ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Usuario_ActualizarEstado", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarBloqueo(UsuarioBE Pusuario)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pusuario.Id },
                { "@Intentos", Pusuario.Intentos },
                { "@Suspendido", Pusuario.Suspendido },
                { "@Motivo", (object)Pusuario.Motivo ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Usuario_ActualizarBloqueo", Parametros, CommandType.StoredProcedure);
        }
        public int ContarActivos()
        {
            DAO Dao = new DAO();
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Usuario_ContarActivos", null, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado);
        }
        public UsuarioBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Usuario_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public List<UsuarioBE> ListarActivos()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Usuario_ListarActivos", null, CommandType.StoredProcedure);
            List<UsuarioBE> Usuarios = new List<UsuarioBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Usuarios.Add(MapearFila(Fila));
            }
            return Usuarios;
        }
        public void RevocarAcceso(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Usuario_RevocarAcceso", Parametros, CommandType.StoredProcedure);
        }
        public List<UsuarioBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Usuario_Listar", null, CommandType.StoredProcedure);
            List<UsuarioBE> Usuarios = new List<UsuarioBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Usuarios.Add(MapearFila(Fila));
            }
            return Usuarios;
        }
        /// Busca un Usuario por NombreUsuario. Trae todos sus campos, incluida
        /// la Contraseña (hasheada) — necesaria para que la BLL pueda verificarla.
        public UsuarioBE ObtenerPorNombreUsuario(string PnombreUsuario)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@NombreUsuario", PnombreUsuario } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Usuario_ObtenerPorNombreUsuario", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public List<UsuarioBE> ListarPorEmpresa(int empresaId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@EmpresaId", empresaId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Usuario_ListarPorEmpresa", Parametros, CommandType.StoredProcedure);
            List<UsuarioBE> Usuarios = new List<UsuarioBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Usuarios.Add(MapearFila(Fila));
            }
            return Usuarios;
        }
        public bool ExistePorGmailExcluyendo(string gmail, int idExcluir)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Gmail", gmail },
                { "@IdExcluir", idExcluir }
            };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Usuario_ExistePorGmailExcluyendo", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        public bool ExistePorGmail(string gmail)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Gmail", gmail } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Usuario_ExistePorGmail", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        public bool ExistePorNombreUsuario(string nombreUsuario)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@NombreUsuario", nombreUsuario } };
            object Resultado = Dao.ExecuteScalarStoredProcedure("SP_Usuario_ExistePorNombreUsuario", Parametros, CommandType.StoredProcedure);
            return Convert.ToInt32(Resultado) > 0;
        }
        /// Convierte una fila del DataSet en un UsuarioBE. El Perfil queda
        /// armado solo con el Id (no el árbol completo de permisos) — eso
        /// se completa aparte, cuando hace falta, vía PerfilBLL.
        private UsuarioBE MapearFila(DataRow Fila)
        {
            return new UsuarioBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                Nombre = Fila["Nombre"].ToString(),
                Apellido = Fila["Apellido"].ToString(),
                NombreUsuario = Fila["NombreUsuario"].ToString(),
                Contrasena = Fila.Table.Columns.Contains("Contrasena") ? Fila["Contrasena"].ToString() : null,
                Gmail = Fila["Gmail"].ToString(),
                Dni = Fila["Dni"].ToString(),
                Telefono = Fila["Telefono"] == DBNull.Value ? null : Fila["Telefono"].ToString(),
                Empleado = Convert.ToBoolean(Fila["Empleado"]),
                Empresa = Convert.ToBoolean(Fila["Empresa"]),
                Independiente = Convert.ToBoolean(Fila["Independiente"]),
                Local = Convert.ToBoolean(Fila["Local"]),
                Suspendido = Convert.ToBoolean(Fila["Suspendido"]),
                Motivo = Fila["Motivo"] == DBNull.Value ? null : Fila["Motivo"].ToString(),
                Intentos = Convert.ToInt32(Fila["Intentos"]),
                Perfil = Fila["PerfilId"] == DBNull.Value ? null : new PerfilBE { Id = Convert.ToInt32(Fila["PerfilId"]) },
                EmpresaId = Fila["EmpresaId"] == DBNull.Value ? (int?)null : Convert.ToInt32(Fila["EmpresaId"]),
            };
        }
    }
}