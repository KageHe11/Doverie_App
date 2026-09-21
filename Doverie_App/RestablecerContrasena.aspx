<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="RestablecerContrasena.aspx.cs" Inherits="RestablecerContrasena" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 70vh;">
        <div class="col-md-5 col-lg-4">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center">Elegir nueva contraseña</h2>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

                    <asp:Panel ID="pnlFormulario" runat="server">
                        <div class="mb-3">
                            <label class="form-label">Nueva contraseña</label>
                            <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Confirmar contraseña</label>
                            <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>
                        <div class="d-grid">
                            <asp:Button ID="btnRestablecer" runat="server" Text="Restablecer contraseña" CssClass="btn btn-primary" OnClick="btnRestablecer_Click" />
                        </div>
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>

</asp:Content>