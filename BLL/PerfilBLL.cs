using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace BLL
{
    public class PerfilBLL
    {
        public List<PerfilBE> Listar()
        {
            PerfilDAL Dal = new PerfilDAL();
            return Dal.Listar();
        }
        public void Registrar(PerfilBE perfil)
        {
            PerfilDAL Dal = new PerfilDAL();
            Dal.Registrar(perfil);
        }
        public void ActualizarDatos(PerfilBE perfil)
        {
            PerfilDAL Dal = new PerfilDAL();
            Dal.ActualizarDatos(perfil);
        }
        public void ActualizarEstado(PerfilBE perfil)
        {
            PerfilDAL Dal = new PerfilDAL();
            Dal.ActualizarEstado(perfil);
        }
        public List<PerfilBE> ListarAsignablesPorEmpresa()
        {
            DAL.PerfilDAL PerfilDal = new DAL.PerfilDAL();
            return PerfilDal.ListarAsignablesPorEmpresa();
        }
        public PerfilBE ObtenerDatos(int id)
        {
            PerfilDAL PerfilDal = new PerfilDAL();
            return PerfilDal.ObtenerDatos(id);
        }
        public void ActualizarPermisos(PerfilBE perfil)
        {
            DAL.PerfilDAL PerfilDal = new DAL.PerfilDAL();
            PerfilDal.ActualizarPermisos(perfil);
        }
        public PerfilBE ObtenerPorNombre(string nombre)
        {
            PerfilDAL PerfilDal = new PerfilDAL();
            return PerfilDal.ObtenerPorNombre(nombre);
        }
    }
}
