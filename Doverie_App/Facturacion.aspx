<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Facturacion.aspx.cs" Inherits="Facturacion" %>
<%@ Import Namespace="BE" %>
<%@ Import Namespace="BLL" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <%-- Sin UpdatePanel: la descarga del PDF necesita un postback completo. --%>
    <h2 class="mb-3"><%: Traductor.T("fact.titulo") %></h2>

    <asp:Label ID="lblInfo" runat="server" CssClass="alert alert-info d-block" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

    <%-- CU-001-014 paso 3: plan vigente y medio de pago registrado --%>
    <div class="card mb-3">
        <div class="card-body small">
            <asp:Panel ID="pnlPlanVigente" runat="server">
                <span class="text-muted"><%: Traductor.T("susc.planActual") %>:</span>
                <strong><asp:Literal ID="litPlan" runat="server" Mode="Encode" /></strong>
                <span class="text-muted ms-3"><asp:Literal ID="litFechaEtiqueta" runat="server" Mode="Encode" />:</span>
                <asp:Literal ID="litFecha" runat="server" Mode="Encode" />
                <span class="text-muted ms-3"><%: Traductor.T("susc.medioRegistrado") %>:</span>
                <asp:Literal ID="litMedio" runat="server" Mode="Encode" />
            </asp:Panel>
            <asp:Panel ID="pnlSinPlan" runat="server" Visible="false">
                <%: Traductor.T("susc.sinSuscripcion") %>
                <a href="/Suscripcion.aspx"><%: Traductor.T("susc.verPlanes") %></a>
            </asp:Panel>
        </div>
    </div>

    <%-- CU-001-014 pasos 6 y 7: comprobante del pago elegido --%>
    <asp:Panel ID="pnlComprobante" runat="server" CssClass="card mb-3 border-primary" Visible="false">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-start">
                <h5 class="card-title"><%: Traductor.T("fact.comprobanteTitulo") %> <asp:Literal ID="litNumero" runat="server" Mode="Encode" /></h5>
                <asp:LinkButton ID="btnCerrarComprobante" runat="server" CssClass="btn-close" OnClick="btnCerrarComprobante_Click" />
            </div>
            <asp:HiddenField ID="hdnPagoId" runat="server" />
            <table class="table table-sm small mb-3">
                <tr><th style="width: 30%"><%: Traductor.T("fact.fecha") %></th><td><asp:Literal ID="litCompFecha" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.titular") %></th><td><asp:Literal ID="litCompTitular" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.plan") %></th><td><asp:Literal ID="litCompPlan" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.concepto") %></th><td><asp:Literal ID="litCompConcepto" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.medio") %></th><td><asp:Literal ID="litCompMedio" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.monto") %></th><td class="fw-semibold"><asp:Literal ID="litCompMonto" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.estado") %></th><td><asp:Literal ID="litCompEstado" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("fact.operacion") %></th><td class="font-monospace"><asp:Literal ID="litCompOperacion" runat="server" Mode="Encode" /></td></tr>
            </table>
            <asp:Button ID="btnDescargarPdf" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnDescargarPdf_Click" />
        </div>
    </asp:Panel>

    <%-- CU-001-014 paso 4: historial de pagos --%>
    <asp:GridView ID="gvPagos" runat="server" CssClass="table table-striped table-hover align-middle"
        AutoGenerateColumns="false" OnRowCommand="gvPagos_RowCommand" OnRowDataBound="gvPagos_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
            <asp:TemplateField>
                <ItemTemplate><%#: Traductor.T("fact.concepto." + Eval("Concepto")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate><%#: Traductor.T("plan.nivel." + Eval("PlanNombre")) + " - " + Traductor.T("plan.tipo." + Eval("PlanTipo")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate><%#: Traductor.T("pago.medio." + Eval("MedioPagoId")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField ItemStyle-CssClass="text-end" HeaderStyle-CssClass="text-end">
                <ItemTemplate><%#: SuscripcionBLL.FormatearMonto((decimal)Eval("Monto")) %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <span class='<%# "badge " + ClaseEstado((string)Eval("Estado")) %>'><%#: Traductor.T("fact.estado." + Eval("Estado")) %></span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnVer" runat="server" CssClass="btn btn-sm btn-outline-primary" CommandName="Ver" CommandArgument='<%# Eval("Id") %>'
                        Text='<%# Traductor.T("fact.verComprobante") %>' Visible='<%# (string)Eval("Estado") == PagoBE.EstadoAprobado %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
