using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class UsuarioBLL
    {
        public void Registrar(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.Registrar(Pusuario);
        }
        public void ActualizarEstado(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarEstado(Pusuario);
        }
        public void ActualizarDatos(UsuarioBE Pusuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Pusuario);
        }
        public UsuarioBE ObtenerDatos(int Pid)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ObtenerDatos(Pid);
        }
        public List<UsuarioBE> Listar()
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.Listar();
        }
        public UsuarioBE ObtenerPorNombreUsuario(string PnombreUsuario)
        {
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            return UsuarioDal.ObtenerPorNombreUsuario(PnombreUsuario);
        }
        public bool EstaBloqueado(UsuarioBE Pusuario)
        {
            return Pusuario.Suspendido;
        }
        public void ReiniciarIntentos(UsuarioBE Pusuario)
        {
            Pusuario.Intentos = 0;
            UsuarioDAL UsuarioDal = new UsuarioDAL();
            UsuarioDal.ActualizarDatos(Pusuario);
        }
        public ResultadoLogin LogIn(string PnombreUsuario, string Pcontrasena, out UsuarioBE UsuarioLogueado, out string Token)
        {
            UsuarioLogueado = null;
            Token = null;
            UsuarioBE Usuario = ObtenerPorNombreUsuario(PnombreUsuario);
            if (Usuario == null)
            {
                return ResultadoLogin.CredencialesInvalidas;
            }
            if (EstaBloqueado(Usuario))
            {
                return ResultadoLogin.UsuarioBloqueado;
            }
            HashService Hasheo = new HashService();
            if(!Hasheo.VerificarContrasena(Pcontrasena, Usuario.Contrasena))
            {
                return ResultadoLogin.CredencialesInvalidas;
            }
            ReiniciarIntentos(Usuario);
            //Obtenr Perfil
            TokenService ServioToken = new TokenService();
            Token = ServioToken.GenerarToken(Usuario);
            UsuarioLogueado = Usuario;
            return ResultadoLogin.Exitoso;
        }
    }
}
