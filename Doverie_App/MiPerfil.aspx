<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="MiPerfil.aspx.cs" Inherits="MiPerfil" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("miPerfil.titulo") %></h2>

                    <uc:Mensaje ID="Mensaje" runat="server" />

                    <div class="mb-2">
                        <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                        <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" Enabled="false"></asp:TextBox>
                    </div>

                    <div class="row">
                        <div class="col-6 mb-2">
                            <label class="form-label"><%: Traductor.T("comun.nombre") %></label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" ValidationGroup="MiPerfil" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                        </div>
                        <div class="col-6 mb-2">
                            <label class="form-label"><%: Traductor.T("comun.apellido") %></label>
                            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvApellido" runat="server" ControlToValidate="txtApellido" ValidationGroup="MiPerfil" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                        </div>
                    </div>

                    <div class="mb-2">
                        <label class="form-label"><%: Traductor.T("comun.email") %></label>
                        <asp:TextBox ID="txtGmail" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvGmail" runat="server" ControlToValidate="txtGmail" ValidationGroup="MiPerfil" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                        <asp:RegularExpressionValidator ID="revGmail" runat="server" ControlToValidate="txtGmail" ValidationGroup="MiPerfil" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="<%$ Traduccion:valid.email %>" />
                    </div>

                    <div class="mb-2">
                        <label class="form-label"><%: Traductor.T("comun.nuevaContrasena") %></label>
                        <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="revContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="MiPerfil" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[\s\S]{6,}$" ErrorMessage="<%$ Traduccion:valid.contrasena %>" />
                        <small class="text-muted"><%: Traductor.T("miPerfil.ayudaContrasena") %></small>
                    </div>

                    <div class="d-grid mt-3">
                        <asp:Button ID="btnGuardar" runat="server" ValidationGroup="MiPerfil" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                    </div>

                </div>
            </div>
        </div>
    </div>

</asp:Content>
