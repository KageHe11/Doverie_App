<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="PagoSimulado.aspx.cs" Inherits="PagoSimulado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <%-- Checkout de la pasarela de pago local (PasarelaPagoLocal). Con
         Mercado Pago, este paso es el checkout de Mercado Pago. --%>
    <div class="row justify-content-center">
        <div class="col-md-7 col-lg-6">
            <div class="alert alert-warning small"><%: Traductor.T("pagoSim.aviso") %></div>

            <asp:Panel ID="pnlCobro" runat="server" CssClass="card">
                <div class="card-body">
                    <h4 class="card-title"><%: Traductor.T("pagoSim.titulo") %></h4>
                    <table class="table table-sm">
                        <tr><th style="width: 40%"><%: Traductor.T("pagoSim.concepto") %></th><td><asp:Literal ID="litDescripcion" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><%: Traductor.T("fact.medio") %></th><td><asp:Literal ID="litMedio" runat="server" Mode="Encode" /></td></tr>
                        <tr><th><%: Traductor.T("fact.monto") %></th><td class="fs-5 fw-semibold"><asp:Literal ID="litMonto" runat="server" Mode="Encode" /></td></tr>
                    </table>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnAprobar" runat="server" CssClass="btn btn-success" OnClick="btnAprobar_Click" />
                        <asp:Button ID="btnRechazar" runat="server" CssClass="btn btn-outline-danger" OnClick="btnRechazar_Click" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlSinCobro" runat="server" CssClass="alert alert-danger" Visible="false">
                <%: Traductor.T("pagoSim.sinCobro") %>
                <a href="/Suscripcion.aspx" class="alert-link"><%: Traductor.T("susc.titulo") %></a>
            </asp:Panel>
        </div>
    </div>

</asp:Content>
