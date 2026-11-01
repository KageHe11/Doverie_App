<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Suscripcion.aspx.cs" Inherits="Suscripcion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2 class="mb-0"><%: Traductor.T("susc.titulo") %></h2>
        <asp:HyperLink ID="lnkFacturacion" runat="server" NavigateUrl="~/Facturacion.aspx" CssClass="btn btn-outline-secondary btn-sm" />
    </div>

    <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblInfo" runat="server" CssClass="alert alert-info d-block" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

    <%-- CU-001-008 paso 2: plan actual con sus características y vencimiento --%>
    <asp:Panel ID="pnlActual" runat="server" CssClass="card mb-4" Visible="false">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-start flex-wrap gap-2">
                <div>
                    <div class="text-muted small"><%: Traductor.T("susc.planActual") %></div>
                    <h4 class="mb-1">
                        <asp:Literal ID="litActualNivel" runat="server" Mode="Encode" />
                        <small class="text-muted fs-6"><asp:Literal ID="litActualModalidad" runat="server" Mode="Encode" /></small>
                    </h4>
                    <asp:Label ID="lblActualEstado" runat="server" CssClass="badge" />
                </div>
                <div class="d-flex gap-2">
                    <asp:Button ID="btnCambiarPlan" runat="server" CssClass="btn btn-outline-primary" OnClick="btnCambiarPlan_Click" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn btn-outline-danger" OnClick="btnCancelar_Click" />
                </div>
            </div>
            <div class="row mt-3 small">
                <div class="col-md-6">
                    <table class="table table-sm mb-0">
                        <tr><th style="width: 45%"><%: Traductor.T("susc.precio") %></th><td><asp:Literal ID="litActualPrecio" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><%: Traductor.T("susc.inicio") %></th><td><asp:Literal ID="litActualInicio" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><asp:Literal ID="litActualFechaEtiqueta" runat="server" Mode="Encode" /></th><td><asp:Literal ID="litActualFecha" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><%: Traductor.T("susc.medioRegistrado") %></th><td><asp:Literal ID="litActualMedio" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><%: Traductor.T("susc.usuarios") %></th><td><asp:Literal ID="litActualUsuarios" runat="server" Mode="Encode" /></td></tr>
                    </table>
                </div>
                <div class="col-md-6">
                    <ul class="mb-0"><asp:Literal ID="litActualCaracteristicas" runat="server" /></ul>
                </div>
            </div>
        </div>
    </asp:Panel>

    <%-- CU-001-017 paso 2 / CU-001-008 paso 3: planes disponibles --%>
    <asp:Panel ID="pnlPlanes" runat="server" Visible="false">
        <p class="text-muted"><asp:Literal ID="litPlanesAyuda" runat="server" Mode="Encode" /></p>

        <div class="btn-group mb-3 flex-wrap" role="group">
            <asp:Repeater ID="rptModalidades" runat="server" OnItemCommand="rptModalidades_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="btnModalidad" runat="server" CommandName="Modalidad" CommandArgument='<%# Eval("Tipo") %>'
                        CssClass='<%# "btn " + ((string)Eval("Tipo") == ModalidadElegida ? "btn-primary" : "btn-outline-primary") %>'>
                        <%#: Traductor.T("plan.tipo." + Eval("Tipo")) %>
                        <%# (int)Eval("Descuento") > 0 ? "<span class=\"badge bg-success ms-1\">-" + Eval("Descuento") + "%</span>" : "" %>
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <div class="row g-3 mb-4">
            <asp:Repeater ID="rptPlanes" runat="server" OnItemCommand="rptPlanes_ItemCommand">
                <ItemTemplate>
                    <div class="col-md-4">
                        <div class='<%# "card h-100" + ((bool)Eval("Destacado") ? " border-primary" : "") %>'>
                            <div class="card-body d-flex flex-column">
                                <h4 class="card-title mb-0"><%#: Traductor.T("plan.nivel." + Eval("Nivel")) %></h4>
                                <div class="text-muted mb-3"><%#: Traductor.T("plan.subtitulo." + Eval("Nivel")) %></div>
                                <div class="fs-3 fw-semibold"><%#: Eval("PrecioMensual") %><small class="fs-6 text-muted"> / <%: Traductor.T("susc.mes") %></small></div>
                                <div class="small text-muted mb-3"><%#: Eval("DetallePrecio") %></div>
                                <ul class="small flex-grow-1"><%# Eval("Caracteristicas") %></ul>
                                <asp:Button ID="btnElegir" runat="server" CommandName="Elegir" CommandArgument='<%# Eval("Id") %>'
                                    CssClass="btn btn-primary w-100" Text='<%# Traductor.T("susc.elegir") %>' Visible='<%# !(bool)Eval("EsActual") %>' />
                                <asp:Label ID="lblEsActual" runat="server" CssClass="btn btn-outline-secondary w-100 disabled" Text='<%# Traductor.T("susc.esTuPlan") %>' Visible='<%# (bool)Eval("EsActual") %>' />
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <asp:Button ID="btnVolverActual" runat="server" CssClass="btn btn-link" OnClick="btnVolverActual_Click" Visible="false" />
    </asp:Panel>

    <%-- CU-001-017 pasos 4 y 5: formulario de pago --%>
    <asp:Panel ID="pnlPago" runat="server" CssClass="card mb-4 border-primary" Visible="false">
        <div class="card-body">
            <h5 class="card-title"><%: Traductor.T("susc.confirmarTitulo") %></h5>
            <asp:HiddenField ID="hdnPlanId" runat="server" />
            <table class="table table-sm small">
                <tr><th style="width: 35%"><%: Traductor.T("fact.plan") %></th><td><asp:Literal ID="litPagoPlan" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("susc.precio") %></th><td><asp:Literal ID="litPagoPrecio" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("susc.totalHoy") %></th><td class="fw-semibold"><asp:Literal ID="litPagoTotal" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("susc.renovacionAuto") %></th><td><asp:Literal ID="litPagoRenovacion" runat="server" Mode="Encode" /></td></tr>
            </table>
            <div class="mb-3" style="max-width: 360px">
                <label class="form-label" for="<%= ddlMedioPago.ClientID %>"><%: Traductor.T("fact.medio") %></label>
                <asp:DropDownList ID="ddlMedioPago" runat="server" CssClass="form-select" />
            </div>
            <p class="small text-muted"><%: Traductor.T("susc.pasarelaAyuda") %></p>
            <asp:Button ID="btnPagar" runat="server" CssClass="btn btn-primary" OnClick="btnPagar_Click" />
            <asp:Button ID="btnVolverPlanes" runat="server" CssClass="btn btn-link" OnClick="btnVolverPlanes_Click" CausesValidation="false" />
        </div>
    </asp:Panel>

</asp:Content>
