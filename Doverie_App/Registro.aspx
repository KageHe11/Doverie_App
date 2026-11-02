<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Registro.aspx.cs" Inherits="Registro" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("registro.titulo") %></h2>

                    <uc:Mensaje ID="Mensaje" runat="server" />

                    <asp:Panel ID="pnlFormulario" runat="server">

                        <div class="mb-3">
                            <label class="form-label d-block"><%: Traductor.T("registro.tipoCuenta") %></label>
                            <asp:RadioButtonList ID="rblTipo" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Value="Freelance" Selected="True" />
                                <asp:ListItem Value="Empresa" />
                            </asp:RadioButtonList>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.nombre") %></label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.apellido") %></label>
                                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvApellido" runat="server" ControlToValidate="txtApellido" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvNombreUsuario" runat="server" ControlToValidate="txtNombreUsuario" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.contrasena") %></label>
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                <asp:RegularExpressionValidator ID="revContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[\s\S]{6,}$" ErrorMessage="<%$ Traduccion:valid.contrasena %>" />
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.confirmarContrasena") %></label>
                                <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvConfirmar" runat="server" ControlToValidate="txtConfirmar" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                <asp:CompareValidator ID="cmpConfirmar" runat="server" ControlToValidate="txtConfirmar" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtContrasena" ErrorMessage="<%$ Traduccion:comun.contrasenasNoCoinciden %>" />
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label"><%: Traductor.T("comun.email") %></label>
                            <asp:TextBox ID="txtGmail" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvGmail" runat="server" ControlToValidate="txtGmail" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            <asp:RegularExpressionValidator ID="revGmail" runat="server" ControlToValidate="txtGmail" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="<%$ Traduccion:valid.email %>" />
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.dni") %></label>
                                <asp:TextBox ID="txtDni" runat="server" CssClass="form-control" MaxLength="8"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvDni" runat="server" ControlToValidate="txtDni" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                <asp:RegularExpressionValidator ID="revDni" runat="server" ControlToValidate="txtDni" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^\d{7,8}$" ErrorMessage="<%$ Traduccion:valid.dni %>" />
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.telefono") %></label>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" MaxLength="20"></asp:TextBox>
                                <asp:RegularExpressionValidator ID="revTelefono" runat="server" ControlToValidate="txtTelefono" ValidationGroup="Registro" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[0-9+\-\s()]{6,20}$" ErrorMessage="<%$ Traduccion:valid.telefono %>" />
                            </div>
                        </div>

                        <div class="d-grid mt-3">
                            <asp:Button ID="btnRegistrar" runat="server" ValidationGroup="Registro" CssClass="btn btn-primary" OnClick="btnRegistrar_Click" />
                        </div>

                    </asp:Panel>

                    <div class="text-center mt-3">
                        <a href="/Login.aspx"><%: Traductor.T("registro.yaTenesCuenta") %></a>
                    </div>

                </div>
            </div>
        </div>
    </div>

</asp:Content>
