<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="DocumentoCompartido.aspx.cs" Inherits="DocumentoCompartido" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <%-- El token viaja en la URL: que no se filtre a otros sitios por el
         encabezado Referer, ni que la página quede indexada. --%>
    <meta name="referrer" content="no-referrer" />
    <meta name="robots" content="noindex, nofollow" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mt-4" Visible="false"></asp:Label>

    <asp:Panel ID="pnlDocumento" runat="server" Visible="false">
        <div class="d-flex justify-content-between align-items-center mb-2">
            <h2 class="mb-0"><asp:Literal ID="litNombre" runat="server" Mode="Encode" /></h2>
            <asp:HyperLink ID="lnkDescargar" runat="server" CssClass="btn btn-primary" />
        </div>
        <div class="alert alert-info py-2 small">
            <span class="badge bg-secondary me-2"><%: Traductor.T("compartido.soloLectura") %></span>
            <asp:Literal ID="litInfo" runat="server" Mode="Encode" />
        </div>

        <asp:Panel ID="pnlVistaPdf" runat="server" Visible="false">
            <iframe id="ifrPdf" runat="server" class="w-100 border rounded" style="height: 75vh;"></iframe>
        </asp:Panel>
        <asp:Panel ID="pnlVistaImagen" runat="server" Visible="false" CssClass="text-center">
            <asp:Image ID="imgVista" runat="server" CssClass="img-fluid border rounded" />
        </asp:Panel>
        <asp:Panel ID="pnlSinVista" runat="server" Visible="false" CssClass="border rounded p-5 text-center text-muted">
            <%: Traductor.T("compartido.sinVistaPrevia") %>
        </asp:Panel>
    </asp:Panel>

</asp:Content>
