using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
    public class PermisoBLL
    {
        public List<PermisoBE> Listar()
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            return PermisoDal.Listar();
        }
        public PermisoBE ObtenerPorNombre(string nombre)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            return PermisoDal.ObtenerPorNombre(nombre);
        }
        public PermisoBE ObtenerDatos(int id)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            return PermisoDal.ObtenerDatos(id);
        }
        /// CU-002-004 Alta Permiso: nombre obligatorio [ERROR-004] y sin
        /// repetir [ERROR-022].
        public ResultadoAbm Registrar(PermisoBE permiso)
        {
            ResultadoAbm Validacion = ValidarNombre(permiso);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new DAL.PermisoDAL().Registrar(permiso);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-006 Modificar Permiso [ERROR-004 / ERROR-022 / ERROR-024].
        public ResultadoAbm ActualizarDatos(PermisoBE permiso)
        {
            ResultadoAbm Validacion = ValidarNombre(permiso);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new DAL.PermisoDAL().ActualizarDatos(permiso);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-005 Baja Permiso [ERROR-023]. No se puede eliminar un
        /// Permiso que alguna Familia contiene: dejaría el árbol de permisos
        /// con referencias rotas.
        public ResultadoAbm Eliminar(int id)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            if (PermisoDal.ContarUsos(id) > 0) return ResultadoAbm.EnUso;
            try
            {
                PermisoDal.Eliminar(id);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        private static ResultadoAbm ValidarNombre(PermisoBE permiso)
        {
            permiso.Nombre = (permiso.Nombre ?? "").Trim();
            if (permiso.Nombre.Length == 0 || permiso.Nombre.Length > 50) return ResultadoAbm.DatosInvalidos;
            if (new DAL.PermisoDAL().ExistePorNombreExcluyendo(permiso.Nombre, permiso.Id)) return ResultadoAbm.Duplicado;
            return ResultadoAbm.Exitoso;
        }
        public bool ExistePorNombre(string nombre)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            return PermisoDal.ExistePorNombre(nombre);
        }
    }
}
