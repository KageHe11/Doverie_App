<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Registro.aspx.cs" Inherits="Registro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center">
        <div class="col-md-6 col-lg-5">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("registro.titulo") %></h2>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
                    <asp:Label ID="lblExito" runat="server" CssClass="alert alert-success d-block" Visible="false"></asp:Label>

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
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.apellido") %></label>
                                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.contrasena") %></label>
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.confirmarContrasena") %></label>
                                <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            </div>
                        </div>

                        <div class="mb-2">
                            <label class="form-label"><%: Traductor.T("comun.email") %></label>
                            <asp:TextBox ID="txtGmail" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>

                        <div class="row">
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.dni") %></label>
                                <asp:TextBox ID="txtDni" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-6 mb-2">
                                <label class="form-label"><%: Traductor.T("comun.telefono") %></label>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <div class="d-grid mt-3">
                            <asp:Button ID="btnRegistrar" runat="server" CssClass="btn btn-primary" OnClick="btnRegistrar_Click" />
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
