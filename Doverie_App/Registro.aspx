<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Registro.aspx.cs" Inherits="Registro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center">Registrarse</h2>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
                    <asp:Label ID="lblExito" runat="server" CssClass="alert alert-success d-block" Visible="false"></asp:Label>

                    <asp:Panel ID="pnlFormulario" runat="server">

                        <div class="mb-3">
                            <label class="form-label d-block">Tipo de cuenta</label>
                            <asp:RadioButtonList ID="rblTipo" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Text="Usuario Individual" Value="Freelance" Selected="True" />
                                <asp:ListItem Text="Empresa" Value="Empresa" />
                            </asp:RadioButtonList>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label">Nombre</label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label">Apellido</label>
                                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label">Nombre de usuario</label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label">Contraseña</label>
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label">Confirmar contraseña</label>
                                <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label">Email</label>
                            <asp:TextBox ID="txtGmail" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label">DNI</label>
                                <asp:TextBox ID="txtDni" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label">Teléfono</label>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="d-grid mt-3">
                            <asp:Button ID="btnRegistrar" runat="server" Text="Registrarse" CssClass="btn btn-primary" OnClick="btnRegistrar_Click" />
                        </div>

                    </asp:Panel>

                    <div class="text-center mt-3">
                        <a href="/Login.aspx">¿Ya tenés cuenta? Iniciar sesión</a>
                    </div>

                </div>
            </div>
        </div>
    </div>

</asp:Content>