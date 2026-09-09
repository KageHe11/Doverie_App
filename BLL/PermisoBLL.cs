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
        public void Registrar(PermisoBE permiso)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            PermisoDal.Registrar(permiso);
        }
        public void ActualizarDatos(PermisoBE permiso)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            PermisoDal.ActualizarDatos(permiso);
        }
        public void Eliminar(int id)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            PermisoDal.Eliminar(id);
        }
        public bool ExistePorNombre(string nombre)
        {
            DAL.PermisoDAL PermisoDal = new DAL.PermisoDAL();
            return PermisoDal.ExistePorNombre(nombre);
        }
    }
}
