<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Backup.aspx.cs" Inherits="Backup" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Import Namespace="BE" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <h2 class="mb-3"><%: Traductor.T("backup.titulo") %></h2>

    <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

    <%-- CU-002-013 pasos 1 a 3: generar con confirmación --%>
    <div class="card mb-4">
        <div class="card-body">
            <p class="mb-2"><%: Traductor.T("backup.descripcion") %></p>
            <ul class="small text-muted">
                <li><%: Traductor.T("backup.itemBase") %></li>
                <li><%: Traductor.T("backup.itemArchivos") %></li>
            </ul>
            <asp:Button ID="btnGenerar" runat="server" CssClass="btn btn-primary" OnClick="btnGenerar_Click" />
            <span class="small text-muted ms-2"><%: Traductor.T("backup.demora") %></span>
        </div>
    </div>

    <%-- Restore desde un .bak propio (el del historial se elige en la grilla) --%>
    <asp:Panel ID="pnlRestaurarArchivo" runat="server" CssClass="card mb-4 border-warning">
        <div class="card-body">
            <h5 class="card-title"><%: Traductor.T("restore.titulo") %></h5>
            <p class="small mb-2"><%: Traductor.T("restore.descripcion") %></p>
            <div class="row g-2 align-items-start">
                <div class="col-md-8">
                    <asp:FileUpload ID="fuBak" runat="server" CssClass="form-control" accept=".bak" />
                </div>
                <div class="col-md-4">
                    <asp:Button ID="btnRestaurarArchivo" runat="server" CssClass="btn btn-warning w-100" OnClick="btnRestaurarArchivo_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

    <h5><%: Traductor.T("backup.historial") %></h5>
    <asp:Label ID="lblSinBackups" runat="server" CssClass="alert alert-info d-block" Visible="false"></asp:Label>
    <asp:GridView ID="gvBackups" runat="server" CssClass="table table-striped table-sm align-middle small" AutoGenerateColumns="false"
        OnRowCommand="gvBackups_RowCommand" OnRowDataBound="gvBackups_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm:ss}" />
            <asp:BoundField DataField="NombreUsuario" />
            <asp:TemplateField>
                <ItemTemplate>
                    <span class='<%# "badge " + ((string)Eval("Estado") == BackupBE.EstadoExitoso ? "bg-success" : "bg-danger") %>'><%#: Traductor.T("backup.estado." + Eval("Estado")) %></span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Detalle" ItemStyle-CssClass="text-break" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnRestaurar" runat="server" CssClass="btn btn-sm btn-outline-warning text-nowrap"
                        CommandName="Restaurar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("restore.boton") %>'
                        OnClientClick='<%# Confirmacion.Para("restore.confirmar") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
