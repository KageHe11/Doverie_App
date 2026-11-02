<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Permisos.aspx.cs" Inherits="Permisos" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2><%: Traductor.T("permisos.titulo") %></h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click" />
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>
            <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" />
            <asp:GridView ID="gvPermisos" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvPermisos_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Nombre" />
                    <asp:BoundField DataField="Descripcion" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.editar") %>' />
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick='<%# Confirmacion.Para("permisos.confirmarEliminar") %>'
                                Text='<%# Traductor.T("comun.eliminar") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="modal fade" id="modalPermiso" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <asp:UpdatePanel ID="UpdatePanelModal" runat="server">
                    <ContentTemplate>
                        <div class="modal-header">
                            <h5 class="modal-title"><asp:Literal ID="litTituloModal" runat="server" /></h5>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />
                            <asp:Label ID="lblErrorModal" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.nombre") %></label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.descripcion") %></label>
                                <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"></asp:TextBox>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal"><%: Traductor.T("comun.cancelar") %></button>
                            <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

</asp:Content>
