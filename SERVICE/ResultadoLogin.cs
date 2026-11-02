using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SERVICE
{
    public enum ResultadoLogin
    {
        Exitoso,
        CredencialesInvalidas,
        UsuarioBloqueado,
        UsuarioSuspendido,
        /// Los dígitos verificadores no coinciden y el Usuario no puede
        /// gestionar la integridad: no entra nadie salvo el Administrador.
        BaseInconsistente
    }
}
