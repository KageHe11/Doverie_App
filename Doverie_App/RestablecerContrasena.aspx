<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="RestablecerContrasena.aspx.cs" Inherits="RestablecerContrasena" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="row justify-content-center align-items-center" style="min-height: 70vh;">
        <div class="col-md-5 col-lg-4">
            <div class="card shadow-sm">
                <div class="card-body p-4">

                    <h2 class="mb-3 text-center"><%: Traductor.T("restablecer.titulo") %></h2>

                    <uc:Mensaje ID="Mensaje" runat="server" />

                    <asp:Panel ID="pnlFormulario" runat="server">
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.nuevaContrasena") %></label>
                            <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Restablecer" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            <asp:RegularExpressionValidator ID="revContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Restablecer" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[\s\S]{6,}$" ErrorMessage="<%$ Traduccion:valid.contrasena %>" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label"><%: Traductor.T("comun.confirmarContrasena") %></label>
                            <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvConfirmar" runat="server" ControlToValidate="txtConfirmar" ValidationGroup="Restablecer" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            <asp:CompareValidator ID="cmpConfirmar" runat="server" ControlToValidate="txtConfirmar" ValidationGroup="Restablecer" Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtContrasena" ErrorMessage="<%$ Traduccion:comun.contrasenasNoCoinciden %>" />
                        </div>
                        <div class="d-grid">
                            <asp:Button ID="btnRestablecer" runat="server" ValidationGroup="Restablecer" CssClass="btn btn-primary" OnClick="btnRestablecer_Click" />
                        </div>
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>

</asp:Content>
