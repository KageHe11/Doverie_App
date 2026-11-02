<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Integridad.aspx.cs" Inherits="Integridad" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <h2 class="mb-3"><%: Traductor.T("integ.titulo") %></h2>

    <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>

    <%-- Estado general --%>
    <asp:Panel ID="pnlIntegra" runat="server" CssClass="alert alert-success" Visible="false">
        <strong><%: Traductor.T("integ.integra") %></strong>
        <div class="small"><%: Traductor.T("integ.integraDetalle") %></div>
    </asp:Panel>
    <asp:Panel ID="pnlComprometida" runat="server" CssClass="alert alert-danger" Visible="false">
        <strong><%: Traductor.T("integ.comprometida") %></strong>
        <div class="small mb-2"><%: Traductor.T("integ.comprometidaDetalle") %></div>
        <asp:Button ID="btnRecalcularTodo" runat="server" CssClass="btn btn-sm btn-danger" OnClick="btnRecalcularTodo_Click" />
        <asp:HyperLink ID="lnkBackup" runat="server" NavigateUrl="~/Backup.aspx" CssClass="btn btn-sm btn-outline-dark ms-1" />
    </asp:Panel>

    <%-- Qué no coincide: tabla, nivel y fila o columna --%>
    <asp:Panel ID="pnlProblemas" runat="server" Visible="false">
        <h5><%: Traductor.T("integ.problemas") %></h5>
        <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" />
        <asp:GridView ID="gvProblemas" runat="server" CssClass="table table-sm table-striped align-middle small" AutoGenerateColumns="false"
            OnRowCommand="gvProblemas_RowCommand">
            <Columns>
                <asp:BoundField DataField="Tabla" />
                <asp:TemplateField>
                    <ItemTemplate><%#: Traductor.T("integ.nivel." + Eval("Nivel")) %></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Detalle" ItemStyle-CssClass="font-monospace text-break" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton ID="btnRecalcularTabla" runat="server" CssClass="btn btn-sm btn-outline-danger text-nowrap"
                            CommandName="Recalcular" CommandArgument='<%# Eval("Tabla") %>' Text='<%# Traductor.T("integ.recalcularTabla") %>'
                            OnClientClick='<%# Confirmacion.Para("integ.confirmarTabla") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <asp:Button ID="btnVerificar" runat="server" CssClass="btn btn-outline-primary" OnClick="btnVerificar_Click" />

</asp:Content>
