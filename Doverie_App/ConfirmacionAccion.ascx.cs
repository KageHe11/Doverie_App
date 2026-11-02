using System.Web;
using System.Web.UI;

/// Confirmación de acciones irreversibles (eliminar, restaurar, generar un
/// NFT...): muestra el mensaje de advertencia con las opciones "Confirmar" y
/// "Cancelar" y devuelve la respuesta a la página. Si se confirma, el
/// postback del botón sigue y la página ejecuta la acción; si se cancela,
/// no pasa nada. Se agrega una vez por página y cada botón la usa con
/// OnClientClick = Confirmacion.Para("clave.del.mensaje").
public partial class ConfirmacionAccion : UserControl
{
    /// Script para el OnClientClick del botón, con el mensaje traducido.
    public string Para(string clave)
    {
        return "return ConfirmacionAccion.pedir(this, " + HttpUtility.JavaScriptStringEncode(Traductor.T(clave), true) + ");";
    }
}
