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
        /// CU-002-007 Alta Perfil [ERROR-004 / ERROR-025 / ERROR-026].
        public ResultadoAbm Registrar(PerfilBE perfil)
        {
            ResultadoAbm Validacion = ValidarNombre(perfil);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new PerfilDAL().Registrar(perfil);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-009 Modificar Perfil [ERROR-004 / ERROR-025 / ERROR-029].
        public ResultadoAbm ActualizarDatos(PerfilBE perfil)
        {
            ResultadoAbm Validacion = ValidarNombre(perfil);
            if (Validacion != ResultadoAbm.Exitoso) return Validacion;
            try
            {
                new PerfilDAL().ActualizarDatos(perfil);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        /// CU-002-008 Baja Perfil (y su reactivación). No se puede dar de
        /// baja un Perfil asignado a usuarios activos [ERROR-027].
        public ResultadoAbm ActualizarEstado(PerfilBE perfil)
        {
            PerfilDAL Dal = new PerfilDAL();
            if (!perfil.Activo && Dal.ContarUsuariosActivos(perfil.Id) > 0) return ResultadoAbm.EnUso;
            try
            {
                Dal.ActualizarEstado(perfil);
                return ResultadoAbm.Exitoso;
            }
            catch (Exception)
            {
                return ResultadoAbm.Error;
            }
        }
        private static ResultadoAbm ValidarNombre(PerfilBE perfil)
        {
            perfil.NombrePerfil = (perfil.NombrePerfil ?? "").Trim();
            if (perfil.NombrePerfil.Length == 0 || perfil.NombrePerfil.Length > 50) return ResultadoAbm.DatosInvalidos;
            if (new PerfilDAL().ExistePorNombre(perfil.NombrePerfil, perfil.Id)) return ResultadoAbm.Duplicado;
            return ResultadoAbm.Exitoso;
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
