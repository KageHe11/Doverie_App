<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Familias.aspx.cs" Inherits="Familias" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2><%: Traductor.T("familias.titulo") %></h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click" />
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>
            <asp:GridView ID="gvFamilias" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvFamilias_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Nombre" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.editar") %>' />
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick='<%# Traductor.Confirmar("familias.confirmarEliminar") %>'
                                Text='<%# Traductor.T("comun.eliminar") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>
        <hr class="my-4" />

    <h4><%: Traductor.T("familias.arbol") %></h4>
    <asp:UpdatePanel ID="UpdatePanelArbol" runat="server">
        <ContentTemplate>
            <div class="card">
                <div class="card-body">
                    <asp:TreeView ID="tvArbol" runat="server" ShowLines="true"></asp:TreeView>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="modal fade" id="modalFamilia" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <asp:UpdatePanel ID="UpdatePanelModal" runat="server">
                    <ContentTemplate>
                        <div class="modal-header">
                            <h5 class="modal-title"><asp:Literal ID="litTituloModal" runat="server" /></h5>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />
                            <asp:Label ID="lblErrorModal" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

                            <div class="mb-3">
                                <label class="form-label"><%: Traductor.T("familias.nombre") %></label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <label class="form-label"><%: Traductor.T("familias.componentes") %></label>
                            <div class="row">
                                <div class="col-5">
                                    <small class="text-muted"><%: Traductor.T("comun.disponibles") %></small>
                                    <asp:ListBox ID="lstDisponibles" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="10"></asp:ListBox>
                                </div>
                                <div class="col-2 d-flex flex-column justify-content-center align-items-center">
                                    <asp:Button ID="btnAgregarComponente" runat="server" Text=">>" CssClass="btn btn-outline-primary btn-sm mb-2" OnClick="btnAgregarComponente_Click" />
                                    <asp:Button ID="btnQuitarComponente" runat="server" Text="<<" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnQuitarComponente_Click" />
                                </div>
                                <div class="col-5">
                                    <small class="text-muted"><%: Traductor.T("comun.seleccionados") %></small>
                                    <asp:ListBox ID="lstSeleccionados" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="10"></asp:ListBox>
                                </div>
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
