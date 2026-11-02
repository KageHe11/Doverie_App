<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Documentos.aspx.cs" Inherits="Documentos" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <%-- Sin UpdatePanel a propósito: la subida y la descarga de archivos
         necesitan postbacks completos. --%>

    <h2 class="mb-3"><%: Traductor.T("doc.titulo") %></h2>

    <%-- Sin ViewState: cada mensaje se muestra una sola vez y no queda
         pegado en los postbacks siguientes (filtrar, verificar...). --%>
    <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

    <%-- CU-001-002 Subir Documento --%>
    <asp:Panel ID="pnlSubir" runat="server" CssClass="card mb-3">
        <div class="card-body">
            <h5 class="card-title"><%: Traductor.T("doc.subirTitulo") %></h5>
            <div class="row g-2 align-items-end">
                <div class="col-md-9">
                    <asp:FileUpload ID="fuArchivo" runat="server" CssClass="form-control" />
                    <div class="form-text"><asp:Literal ID="litAyudaArchivo" runat="server" Mode="Encode" /></div>
                </div>
                <div class="col-md-3 align-self-start">
                    <asp:Button ID="btnSubir" runat="server" CssClass="btn btn-primary w-100" OnClick="btnSubir_Click" />
                </div>
            </div>
        </div>
    </asp:Panel>

    <%-- CU-001-006 Verificar Integridad: resultado --%>
    <asp:Panel ID="pnlVerificacion" runat="server" CssClass="card mb-3" Visible="false">
        <div class="card-body">
            <h5 class="card-title"><%: Traductor.T("doc.verificacionTitulo") %>: <asp:Literal ID="litVerificacionNombre" runat="server" Mode="Encode" /></h5>
            <asp:Label ID="lblVerificacionResultado" runat="server" CssClass="alert d-block"></asp:Label>
            <table class="table table-sm small mb-3">
                <tr><th style="width: 30%"><%: Traductor.T("doc.hashRegistrado") %></th><td class="font-monospace text-break"><asp:Literal ID="litHashRegistrado" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.hashActual") %></th><td class="font-monospace text-break"><asp:Literal ID="litHashActual" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.fechaSellado") %></th><td><asp:Literal ID="litFechaSellado" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.red") %></th><td><asp:Literal ID="litRed" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.transaccion") %></th><td class="font-monospace text-break"><asp:Literal ID="litTransaccion" runat="server" Mode="Encode" />
                    <asp:HyperLink ID="lnkExplorador" runat="server" CssClass="d-block small mt-1" Target="_blank" rel="noopener noreferrer" Visible="false" /></td></tr>
            </table>
            <h6><%: Traductor.T("doc.historial") %></h6>
            <asp:GridView ID="gvHistorial" runat="server" CssClass="table table-sm table-striped small" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="Accion" />
                    <asp:BoundField DataField="NombreUsuario" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>

    <%-- CU-001-007 Generar NFT y CU-001-013 Consultar Estado de NFT --%>
    <asp:Panel ID="pnlNFT" runat="server" CssClass="card mb-3 border-dark" Visible="false">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-start">
                <h5 class="card-title"><%: Traductor.T("doc.nftTitulo") %>: <asp:Literal ID="litNFTNombre" runat="server" Mode="Encode" /></h5>
                <asp:LinkButton ID="btnCerrarNFT" runat="server" CssClass="btn-close" OnClick="btnCerrarNFT_Click" />
            </div>
            <asp:Label ID="lblNFTEstado" runat="server" CssClass="alert d-block"></asp:Label>
            <table class="table table-sm small mb-0">
                <tr><th style="width: 30%"><%: Traductor.T("doc.nftIdentificador") %></th><td class="font-monospace text-break"><asp:Literal ID="litNFTIdentificador" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.nftToken") %></th><td><asp:Literal ID="litNFTToken" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.nftHash") %></th><td class="font-monospace text-break"><asp:Literal ID="litNFTHash" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.nftFecha") %></th><td><asp:Literal ID="litNFTFecha" runat="server" Mode="Encode" /></td></tr>
                <tr><th><%: Traductor.T("doc.red") %></th><td><asp:Literal ID="litNFTRed" runat="server" Mode="Encode" /></td></tr>
                <asp:PlaceHolder ID="phNFTPropietario" runat="server">
                    <tr><th><%: Traductor.T("doc.nftPropietario") %></th><td class="font-monospace text-break"><asp:Literal ID="litNFTPropietario" runat="server" Mode="Encode" /></td></tr>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phNFTTransaccion" runat="server">
                    <tr><th><%: Traductor.T("doc.transaccion") %></th><td class="font-monospace text-break"><asp:Literal ID="litNFTTransaccion" runat="server" Mode="Encode" /></td></tr>
                </asp:PlaceHolder>
            </table>
            <asp:HyperLink ID="lnkNFTExplorador" runat="server" CssClass="d-inline-block small mt-2" Target="_blank" rel="noopener noreferrer" Visible="false" />
        </div>
    </asp:Panel>

    <%-- CU-001-005 Compartir Documento --%>
    <asp:Panel ID="pnlCompartir" runat="server" CssClass="card mb-3 border-primary" Visible="false">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-start">
                <h5 class="card-title"><%: Traductor.T("compartir.titulo") %>: <asp:Literal ID="litCompartirNombre" runat="server" Mode="Encode" /></h5>
                <asp:LinkButton ID="btnCerrarCompartir" runat="server" CssClass="btn-close" OnClick="btnCerrarCompartir_Click" />
            </div>
            <asp:HiddenField ID="hdnCompartirDocumentoId" runat="server" />
            <asp:Label ID="lblCompartirMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>
            <asp:Label ID="lblCompartirError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>
            <%-- Si el email no se pudo enviar, el enlace se muestra para copiarlo a mano. --%>
            <asp:Panel ID="pnlEnlace" runat="server" CssClass="alert alert-warning" Visible="false" EnableViewState="false">
                <div class="mb-2"><%: Traductor.T("compartir.copiarAyuda") %></div>
                <div class="input-group">
                    <asp:TextBox ID="txtEnlace" runat="server" CssClass="form-control font-monospace small" ReadOnly="true" EnableViewState="false" />
                    <button type="button" class="btn btn-outline-secondary" onclick="copiarEnlace(this)"><%: Traductor.T("compartir.copiar") %></button>
                </div>
            </asp:Panel>
            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-6">
                    <label class="form-label"><%: Traductor.T("compartir.emailDestinatario") %></label>
                    <asp:TextBox ID="txtEmailDestinatario" runat="server" CssClass="form-control" TextMode="Email" MaxLength="100"></asp:TextBox>
                </div>
                <div class="col-md-3">
                    <label class="form-label"><%: Traductor.T("compartir.vigencia") %></label>
                    <asp:DropDownList ID="ddlVigencia" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-3">
                    <asp:Button ID="btnCompartir" runat="server" CssClass="btn btn-primary w-100" OnClick="btnCompartir_Click" />
                </div>
            </div>
            <h6><%: Traductor.T("compartir.enlaces") %></h6>
            <asp:Label ID="lblSinEnlaces" runat="server" CssClass="text-muted small d-block" Visible="false"></asp:Label>
            <asp:GridView ID="gvCompartidos" runat="server" CssClass="table table-sm small align-middle" AutoGenerateColumns="false"
                OnRowDataBound="gvCompartidos_RowDataBound" OnRowCommand="gvCompartidos_RowCommand">
                <Columns>
                    <asp:BoundField DataField="EmailDestinatario" />
                    <asp:BoundField DataField="NombreUsuario" />
                    <asp:BoundField DataField="FechaCreacion" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="FechaVencimiento" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="CantidadAccesos" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Label ID="lblEstadoEnlace" runat="server" />
                            <asp:LinkButton ID="btnRevocar" runat="server" CssClass="btn btn-sm btn-outline-danger ms-1"
                                CommandName="Revocar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("compartir.revocar") %>'
                                OnClientClick='<%# Confirmacion.Para("compartir.confirmarRevocar") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
    <script>
        function copiarEnlace(boton) {
            var campo = boton.parentNode.querySelector('input');
            campo.select();
            if (navigator.clipboard) { navigator.clipboard.writeText(campo.value); } else { document.execCommand('copy'); }
        }
    </script>

    <%-- CU-001-012 Ver Listado de Documentos: filtros --%>
    <div class="card mb-3">
        <div class="card-body">
            <div class="row g-2 align-items-end">
                <div class="col-md-4">
                    <label class="form-label"><%: Traductor.T("comun.nombre") %></label>
                    <asp:TextBox ID="txtFiltroNombre" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%: Traductor.T("doc.tipo") %></label>
                    <asp:DropDownList ID="ddlTipo" runat="server" CssClass="form-select">
                        <asp:ListItem Value="" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%: Traductor.T("bitacora.desde") %></label>
                    <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%: Traductor.T("bitacora.hasta") %></label>
                    <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                </div>
                <div class="col-md-2">
                    <asp:Button ID="btnFiltrar" runat="server" CssClass="btn btn-outline-primary w-100" OnClick="btnFiltrar_Click" />
                </div>
            </div>
        </div>
    </div>

    <asp:Label ID="lblSinResultados" runat="server" CssClass="alert alert-info d-block" Visible="false"></asp:Label>

    <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" MostrarBuscador="false" />
    <asp:GridView ID="gvDocumentos" runat="server" CssClass="table table-striped table-hover align-middle"
        AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvDocumentos_RowCommand" OnRowDataBound="gvDocumentos_RowDataBound">
        <Columns>
            <asp:BoundField DataField="Nombre" />
            <asp:BoundField DataField="Tipo" />
            <asp:BoundField DataField="NombreUsuario" />
            <asp:BoundField DataField="FechaSubida" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:LinkButton ID="btnDescargar" runat="server" CssClass="btn btn-sm btn-outline-secondary"
                        CommandName="Descargar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("doc.descargar") %>' />
                    <asp:LinkButton ID="btnVerificar" runat="server" CssClass="btn btn-sm btn-outline-primary"
                        CommandName="Verificar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("doc.verificar") %>' />
                    <asp:LinkButton ID="btnGenerarNFT" runat="server" CssClass="btn btn-sm btn-outline-dark"
                        CommandName="GenerarNFT" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("doc.generarNFT") %>'
                        OnClientClick='<%# Confirmacion.Para("doc.nftConfirmar") %>' />
                    <asp:LinkButton ID="btnConsultarNFT" runat="server" CssClass="btn btn-sm btn-dark"
                        CommandName="ConsultarNFT" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("doc.consultarNFT") %>' />
                    <asp:LinkButton ID="btnCompartirFila" runat="server" CssClass="btn btn-sm btn-outline-success"
                        CommandName="Compartir" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("compartir.boton") %>' />
                    <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn btn-sm btn-outline-danger"
                        CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.eliminar") %>'
                        OnClientClick='<%# Confirmacion.Para("doc.confirmarEliminar") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

</asp:Content>
