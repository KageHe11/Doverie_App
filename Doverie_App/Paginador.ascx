<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Paginador.ascx.cs" Inherits="Paginador" %>

<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
    <asp:Panel ID="pnlBuscador" runat="server" CssClass="d-flex gap-2" DefaultButton="btnBuscar">
        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-sm" style="min-width: 220px" />
        <asp:Button ID="btnBuscar" runat="server" CssClass="btn btn-sm btn-outline-primary" OnClick="btnBuscar_Click" />
        <asp:LinkButton ID="btnLimpiar" runat="server" CssClass="btn btn-sm btn-link" OnClick="btnLimpiar_Click" Visible="false" />
    </asp:Panel>
    <div class="d-flex align-items-center gap-2 ms-auto">
        <span class="small text-muted"><asp:Literal ID="litResumen" runat="server" Mode="Encode" /></span>
        <asp:PlaceHolder ID="phPaginas" runat="server">
            <nav aria-label="<%: Traductor.T("pag.paginas") %>">
                <ul class="pagination pagination-sm mb-0">
                    <li class="page-item <%= PaginaActual == 0 ? "disabled" : "" %>">
                        <asp:LinkButton ID="btnAnterior" runat="server" CssClass="page-link" OnClick="btnAnterior_Click" Text="&lsaquo;" />
                    </li>
                    <asp:Repeater ID="rptPaginas" runat="server" OnItemCommand="rptPaginas_ItemCommand">
                        <ItemTemplate>
                            <li class='page-item <%# (int)Container.DataItem == PaginaActual ? "active" : "" %>'>
                                <asp:LinkButton ID="btnPagina" runat="server" CssClass="page-link" CommandName="Pagina"
                                    CommandArgument='<%# Container.DataItem %>' Text='<%# (int)Container.DataItem + 1 %>' />
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                    <li class="page-item <%= PaginaActual >= TotalPaginas - 1 ? "disabled" : "" %>">
                        <asp:LinkButton ID="btnSiguiente" runat="server" CssClass="page-link" OnClick="btnSiguiente_Click" Text="&rsaquo;" />
                    </li>
                </ul>
            </nav>
        </asp:PlaceHolder>
    </div>
</div>
