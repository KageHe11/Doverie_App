<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="RecuperarContrasena.aspx.cs" Inherits="RecuperarContrasena" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 70vh;">
        <div class="col-md-5 col-lg-4">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("recuperar.titulo") %></h2>
                    <p class="text-muted text-center"><%: Traductor.T("recuperar.descripcion") %></p>

                    <uc:Mensaje ID="Mensaje" runat="server" />

                    <asp:Panel ID="pnlFormulario" runat="server">
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario" ValidationGroup="Recuperar" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.email") %></label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="Recuperar" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ValidationGroup="Recuperar" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="<%$ Traduccion:valid.email %>" />
                        </div>
                        <div class="d-grid">
                            <asp:Button ID="btnEnviar" runat="server" ValidationGroup="Recuperar" CssClass="btn btn-primary" OnClick="btnEnviar_Click" />
                        </div>
                    </asp:Panel>

                    <div class="text-center mt-3">
                        <a href="/Login.aspx"><%: Traductor.T("comun.volverLogin") %></a>
                    </div>

                </div>
            </div>
        </div>
    </div>

</asp:Content>
