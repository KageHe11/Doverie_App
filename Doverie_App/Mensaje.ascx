<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Mensaje.ascx.cs" Inherits="Mensaje" %>

<%-- Mensaje para el usuario (éxito, error o aviso) con el estilo de alerta
     de Bootstrap. La página lo muestra con Exito(), Error() o Aviso(). --%>
<asp:Panel ID="pnlMensaje" runat="server" Visible="false" EnableViewState="false" role="alert">
    <asp:Literal ID="litTexto" runat="server" Mode="Encode" EnableViewState="false" />
    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="<%: Traductor.T("comun.cerrar") %>"></button>
</asp:Panel>
