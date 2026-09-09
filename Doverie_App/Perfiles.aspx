<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Perfiles.aspx.cs" Inherits="Perfiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2>Gestionar Perfiles</h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click">
            + Agregar Perfil
        </asp:LinkButton>
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>

            <asp:GridView ID="gvPerfiles" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvPerfiles_RowCommand">
                <Columns>
                    <asp:BoundField DataField="NombrePerfil" HeaderText="Nombre" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <span class='<%# (bool)Eval("Activo") ? "badge bg-success" : "badge bg-secondary" %>'>
                                <%# (bool)Eval("Activo") ? "Activo" : "Inactivo" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>'>Editar</asp:LinkButton>

                            <asp:LinkButton runat="server" CssClass='<%# (bool)Eval("Activo") ? "btn btn-sm btn-outline-danger" : "btn btn-sm btn-outline-success" %>'
                                CommandName="ToggleEstado" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick="return confirm('¿Confirmás esta acción?');">
                                <%# (bool)Eval("Activo") ? "Dar de baja" : "Reactivar" %>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="modal fade" id="modalPerfil" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <asp:UpdatePanel ID="UpdatePanelModal" runat="server">
                    <ContentTemplate>
                        <div class="modal-header">
                            <h5 class="modal-title">
                                <asp:Literal ID="litTituloModal" runat="server" Text="Agregar Perfil" />
                            </h5>
                        </div>
                        <div class="modal-body">

                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />

                            <asp:Label ID="lblErrorModal" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

                            <div class="mb-3">
                                <label class="form-label">Nombre del Perfil</label>
                                <asp:TextBox ID="txtNombrePerfil" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <label class="form-label">Familias asignadas</label>
                            <div class="row">
                                <div class="col-5">
                                    <small class="text-muted">Disponibles</small>
                                    <asp:ListBox ID="lstFamiliasDisponibles" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="8"></asp:ListBox>
                                </div>
                                <div class="col-2 d-flex flex-column justify-content-center align-items-center">
                                    <asp:Button ID="btnAgregarFamilia" runat="server" Text=">>" CssClass="btn btn-outline-primary btn-sm mb-2" OnClick="btnAgregarFamilia_Click" />
                                    <asp:Button ID="btnQuitarFamilia" runat="server" Text="<<" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnQuitarFamilia_Click" />
                                </div>
                                <div class="col-5">
                                    <small class="text-muted">Seleccionadas</small>
                                    <asp:ListBox ID="lstFamiliasSeleccionadas" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="8"></asp:ListBox>
                                </div>
                            </div>

                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <asp:Button ID="btnGuardar" runat="server" Text="Guardar" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

</asp:Content>