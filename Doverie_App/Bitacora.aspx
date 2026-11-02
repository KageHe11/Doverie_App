<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Bitacora.aspx.cs" Inherits="Bitacora" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />
    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <h2 class="mb-1"><%: Traductor.T("bitacora.titulo") %></h2>
    <p class="text-muted small mb-3"><%: Traductor.T("bitacora.retencion", BLL.BitacoraBLL.DiasRetencion) %></p>

    <asp:UpdatePanel ID="UpdatePanelBitacora" runat="server">
        <ContentTemplate>

            <uc:Mensaje ID="Mensaje" runat="server" />

            <div class="card mb-3">
                <div class="card-body">
                    <div class="row g-2 align-items-end">
                        <div class="col-md-3">
                            <label class="form-label"><%: Traductor.T("comun.usuario") %></label>
                            <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-select">
                                <asp:ListItem Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label"><%: Traductor.T("bitacora.tipoEvento") %></label>
                            <asp:DropDownList ID="ddlAccion" runat="server" CssClass="form-select">
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
                            <asp:CompareValidator ID="cmpHasta" runat="server" ControlToValidate="txtHasta" ValidationGroup="FiltroBitacora" Display="Dynamic" CssClass="text-danger small" ControlToCompare="txtDesde" Operator="GreaterThanEqual" Type="Date" ErrorMessage="<%$ Traduccion:valid.rangoFechas %>" />
                        </div>
                        <div class="col-md-2">
                            <asp:Button ID="btnFiltrar" runat="server" ValidationGroup="FiltroBitacora" CssClass="btn btn-primary w-100" OnClick="btnFiltrar_Click" />
                        </div>
                    </div>
                    <div class="form-check mt-2">
                        <asp:CheckBox ID="chkEliminados" runat="server" />
                        <label class="form-check-label" for="<%= chkEliminados.ClientID %>"><%: Traductor.T("bitacora.incluirEliminados") %></label>
                    </div>
                </div>
            </div>

            <asp:Label ID="lblSinResultados" runat="server" CssClass="alert alert-info" Visible="false"></asp:Label>

            <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" TamanoPagina="15" />
            <asp:GridView ID="gvBitacora" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" OnRowCommand="gvBitacora_RowCommand" OnRowDataBound="gvBitacora_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="Accion" />
                    <asp:BoundField DataField="NombreUsuario" />
                    <asp:BoundField DataField="Detalle" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Eliminado") %>'>
                                <span class="badge bg-secondary"><%#: Traductor.T("bitacora.eliminado") %></span>
                                <div class="small text-muted text-nowrap"><%#: Traductor.T("bitacora.eliminadoDetalle", Eval("FechaEliminacion", "{0:dd/MM/yyyy HH:mm}"), Eval("NombreEliminadoPor")) %></div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Archivado") %>'>
                                <span class="badge bg-light text-dark border"><%#: Traductor.T("bitacora.archivado") %></span>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn btn-sm btn-outline-danger text-nowrap"
                                CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.eliminar") %>'
                                Visible='<%# PuedeEliminar && !(bool)Eval("Archivado") && !(bool)Eval("Eliminado") %>'
                                OnClientClick='<%# Confirmacion.Para("bitacora.confirmarEliminar") %>' />
                            <asp:LinkButton ID="btnRestaurar" runat="server" CssClass="btn btn-sm btn-outline-secondary text-nowrap"
                                CommandName="Restaurar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("bitacora.restaurar") %>'
                                Visible='<%# PuedeEliminar && !(bool)Eval("Archivado") && (bool)Eval("Eliminado") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <div class="text-center mt-3">
                <asp:Button ID="btnCargarMas" runat="server" CssClass="btn btn-outline-secondary" OnClick="btnCargarMas_Click" />
                <asp:HiddenField ID="hdnHistorialCompleto" runat="server" Value="0" />
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
