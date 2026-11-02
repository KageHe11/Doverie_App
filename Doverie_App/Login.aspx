<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 80vh;">
        <div class="col-md-5 col-lg-4">

            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-4 text-center"><%: Traductor.T("login.titulo") %></h2>

                    <uc:Mensaje ID="Mensaje" runat="server" />

                    <div class="mb-3">
                        <label for="<%= txtUsuario.ClientID %>" class="form-label"><%: Traductor.T("login.usuario") %></label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario" ValidationGroup="Login" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                    </div>

                    <div class="mb-3">
                        <label for="<%= txtContrasena.ClientID %>" class="form-label"><%: Traductor.T("login.contrasena") %></label>
                        <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Login" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                    </div>

                    <div class="d-grid">
                        <asp:Button ID="btnIngresar" runat="server" ValidationGroup="Login" CssClass="btn btn-primary" OnClick="btnIngresar_Click" />
                    </div>

                    <div class="text-center mt-3">
                        <a href="/RecuperarContrasena.aspx"><%: Traductor.T("login.olvideContrasena") %></a>
                    </div>
                    <div class="text-center mt-2">
                        <a href="/Registro.aspx"><%: Traductor.T("login.registrarse") %></a>
                </div>
                </div>
            </div>

        </div>
    </div>

</asp:Content>