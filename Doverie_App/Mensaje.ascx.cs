using System;
using System.Web.UI;

/// Mensaje para el usuario con el estilo de alerta de Bootstrap. El texto
/// se codifica (no se interpreta como HTML) y se muestra solo en la
/// respuesta en la que se pidió: no queda en el ViewState, así un postback
/// posterior no repite un mensaje viejo.
public partial class Mensaje : UserControl
{
    public void Exito(string texto)
    {
        Mostrar(texto, "success");
    }
    public void Error(string texto)
    {
        Mostrar(texto, "danger");
    }
    public void Aviso(string texto)
    {
        Mostrar(texto, "warning");
    }
    public void Ocultar()
    {
        pnlMensaje.Visible = false;
    }
    private void Mostrar(string texto, string tipo)
    {
        litTexto.Text = texto;
        pnlMensaje.CssClass = "alert alert-" + tipo + " alert-dismissible fade show";
        pnlMensaje.Visible = true;
    }
}
