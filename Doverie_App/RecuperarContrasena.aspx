<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="RecuperarContrasena.aspx.cs" Inherits="RecuperarContrasena" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 70vh;">
        <div class="col-md-5 col-lg-4">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("recuperar.titulo") %></h2>
                    <p class="text-muted text-center"><%: Traductor.T("recuperar.descripcion") %></p>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
                    <asp:Label ID="lblExito" runat="server" CssClass="alert alert-success d-block" Visible="false"></asp:Label>

                    <asp:Panel ID="pnlFormulario" runat="server">
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.email") %></label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="d-grid">
                            <asp:Button ID="btnEnviar" runat="server" CssClass="btn btn-primary" OnClick="btnEnviar_Click" />
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
