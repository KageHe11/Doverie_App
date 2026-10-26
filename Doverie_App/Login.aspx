<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 80vh;">
        <div class="col-md-5 col-lg-4">

            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-4 text-center"><%: Traductor.T("login.titulo") %></h2>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

                    <div class="mb-3">
                        <label for="<%= txtUsuario.ClientID %>" class="form-label"><%: Traductor.T("login.usuario") %></label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label for="<%= txtContrasena.ClientID %>" class="form-label"><%: Traductor.T("login.contrasena") %></label>
                        <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                    </div>

                    <div class="d-grid">
                        <asp:Button ID="btnIngresar" runat="server" CssClass="btn btn-primary" OnClick="btnIngresar_Click" />
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