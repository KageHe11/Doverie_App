<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CerrarSesion.ascx.cs" Inherits="CerrarSesion" %>

<%-- Botón de cierre de sesión (CU-002-002). No dispara las validaciones
     de la página: se puede salir con un formulario a medio completar. --%>
<asp:LinkButton ID="btnCerrarSesion" runat="server" CssClass="btn btn-outline-light btn-sm"
    CausesValidation="false" OnClick="btnCerrarSesion_Click" />
