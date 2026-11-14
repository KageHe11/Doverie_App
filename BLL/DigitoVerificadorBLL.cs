using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using DAL;

namespace BLL
{
    /// Integridad de la base con dígitos verificadores (registro, columna y
    /// tabla; el cálculo está en DigitoVerificadorDAL). Si algún dígito no
    /// coincide, solo puede iniciar sesión quien tiene el permiso
    /// GestionarIntegridad (el Administrador), que revisa qué se alteró y
    /// recalcula los dígitos o restaura un backup.
    public class DigitoVerificadorBLL
    {
        public const string PermisoGestion = "GestionarIntegridad";
        /// Perfil que puede entrar en modo recuperación.
        public const string PerfilAdministrador = "Administrador";
        /// Lo único que se habilita en modo recuperación: Integridad y Backup.
        public static readonly string[] PermisosRecuperacion = { PermisoGestion, "GenerarBackup", "RestaurarBackup" };

        /// Lo que sabe la aplicación sin volver a verificar: lo usa PaginaBase
        /// en cada request para sacar a los Usuarios si la base se compromete.
        public static bool BaseComprometida
        {
            get { return DigitoVerificadorDAL.BaseComprometida; }
        }
        /// Para los resguardos ante una tabla eliminada: si algo falló y la
        /// base no se sabía alterada, se verifica en ese momento. Si está
        /// íntegra, la falla es otra y se deja pasar el error.
        public static bool HayAlteracion()
        {
            if (BaseComprometida) return true;
            try
            {
                return new DigitoVerificadorBLL().Verificar().Count > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// Modo recuperación: la base está alterada y el árbol de permisos no
        /// alcanza para reconocer al Administrador (se borraron Permiso,
        /// FamiliaComponente o PerfilFamilia, o sus filas). Se lo reconoce por
        /// su Perfil, y solo puede ir a Integridad y Backup para restaurar.
        public static bool EsModoRecuperacion(UsuarioBE usuario)
        {
            return BaseComprometida && usuario != null && usuario.Perfil != null &&
                usuario.Perfil.NombrePerfil == PerfilAdministrador && !UsuarioBLL.TienePermiso(usuario, PermisoGestion);
        }
        /// Verifica todas las tablas. Al detectar una alteración la registra en
        /// la Bitácora (una vez, hasta que la base vuelva a estar íntegra).
        public List<ProblemaIntegridadBE> Verificar()
        {
            bool YaComprometida = DigitoVerificadorDAL.BaseComprometida;
            List<ProblemaIntegridadBE> Problemas = new DigitoVerificadorDAL().VerificarTodo();
            if (Problemas.Count > 0 && !YaComprometida)
            {
                new BitacoraBLL().Registrar("Integridad Comprometida", Resumir(Problemas), null);
            }
            return Problemas;
        }
        /// Recalcula los dígitos de una tabla, aceptando sus datos actuales.
        /// Si la tabla fue eliminada, acepta que ya no está.
        public List<ProblemaIntegridadBE> RecalcularTabla(UsuarioBE administrador, string tabla)
        {
            DigitoVerificadorDAL Dal = new DigitoVerificadorDAL();
            if (!Dal.RecalcularTabla(tabla)) return Verificar();
            List<ProblemaIntegridadBE> Problemas = Dal.VerificarTodo();
            new BitacoraBLL().Registrar("Recalcular Dígitos Verificadores", "Tabla " + tabla, administrador.Id);
            return Problemas;
        }
        public List<ProblemaIntegridadBE> RecalcularTodo(UsuarioBE administrador)
        {
            DigitoVerificadorDAL Dal = new DigitoVerificadorDAL();
            Dal.RecalcularTodo();
            List<ProblemaIntegridadBE> Problemas = Dal.VerificarTodo();
            new BitacoraBLL().Registrar("Recalcular Dígitos Verificadores", "Todas las tablas", administrador.Id);
            return Problemas;
        }
        private static string Resumir(List<ProblemaIntegridadBE> problemas)
        {
            return string.Join(" | ", problemas.GroupBy(P => P.Tabla).Select(G => G.Key + ": " +
                string.Join(", ", G.Select(P => P.Nivel + (string.IsNullOrEmpty(P.Detalle) ? "" : " (" + P.Detalle + ")")))));
        }
    }
}
