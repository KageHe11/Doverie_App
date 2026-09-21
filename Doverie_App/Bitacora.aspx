<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Bitacora.aspx.cs" Inherits="Bitacora" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <h2 class="mb-3">Bitácora del Sistema</h2>

    <asp:UpdatePanel ID="UpdatePanelBitacora" runat="server">
        <ContentTemplate>

            <div class="card mb-3">
                <div class="card-body">
                    <div class="row g-2 align-items-end">
                        <div class="col-md-3">
                            <label class="form-label">Usuario</label>
                            <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Todos --" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label">Tipo de evento</label>
                            <asp:DropDownList ID="ddlAccion" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Todos --" Value="" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Desde</label>
                            <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Hasta</label>
                            <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="col-md-2">
                            <asp:Button ID="btnFiltrar" runat="server" Text="Filtrar" CssClass="btn btn-primary w-100" OnClick="btnFiltrar_Click" />
                        </div>
                    </div>
                </div>
            </div>

            <asp:Label ID="lblSinResultados" runat="server" CssClass="alert alert-info" Visible="false" Text="No se encontraron eventos con ese filtro."></asp:Label>

            <asp:GridView ID="gvBitacora" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                    <asp:BoundField DataField="Accion" HeaderText="Evento" />
                    <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
                    <asp:BoundField DataField="Detalle" HeaderText="Detalle" />
                </Columns>
            </asp:GridView>

            <div class="text-center mt-3">
                <asp:Button ID="btnCargarMas" runat="server" Text="Cargar más" CssClass="btn btn-outline-secondary" OnClick="btnCargarMas_Click" />
                <asp:HiddenField ID="hdnHistorialCompleto" runat="server" Value="0" />
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>