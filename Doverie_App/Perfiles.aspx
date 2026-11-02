<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Perfiles.aspx.cs" Inherits="Perfiles" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2><%: Traductor.T("perfiles.titulo") %></h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click" />
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

            <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" />
            <asp:GridView ID="gvPerfiles" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvPerfiles_RowCommand">
                <Columns>
                    <asp:BoundField DataField="NombrePerfil" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <span class='<%# (bool)Eval("Activo") ? "badge bg-success" : "badge bg-secondary" %>'>
                                <%# (bool)Eval("Activo") ? Traductor.T("comun.activo") : Traductor.T("comun.inactivo") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.editar") %>' />

                            <asp:LinkButton runat="server" CssClass='<%# (bool)Eval("Activo") ? "btn btn-sm btn-outline-danger" : "btn btn-sm btn-outline-success" %>'
                                CommandName="ToggleEstado" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick='<%# Confirmacion.Para("comun.confirmarAccion") %>'
                                Text='<%# (bool)Eval("Activo") ? Traductor.T("comun.darDeBaja") : Traductor.T("comun.reactivar") %>' />
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
                                <asp:Literal ID="litTituloModal" runat="server" />
                            </h5>
                        </div>
                        <div class="modal-body">

                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />

                            <asp:Label ID="lblErrorModal" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>

                            <div class="mb-3">
                                <label class="form-label"><%: Traductor.T("perfiles.nombre") %></label>
                                <asp:TextBox ID="txtNombrePerfil" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvNombrePerfil" runat="server" ControlToValidate="txtNombrePerfil" ValidationGroup="Perfil" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            </div>

                            <div class="mb-2 form-check">
                                <asp:CheckBox ID="chkAsignablePorEmpresa" runat="server" CssClass="form-check-input" />
                                    <label class="form-check-label"><%: Traductor.T("perfiles.asignableEmpresa") %></label>
                            </div>

                            <label class="form-label"><%: Traductor.T("perfiles.familiasAsignadas") %></label>
                            <div class="row">
                                <div class="col-5">
                                    <small class="text-muted"><%: Traductor.T("comun.disponibles") %></small>
                                    <asp:ListBox ID="lstFamiliasDisponibles" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="8"></asp:ListBox>
                                </div>
                                <div class="col-2 d-flex flex-column justify-content-center align-items-center">
                                    <asp:Button ID="btnAgregarFamilia" runat="server" Text=">>" CssClass="btn btn-outline-primary btn-sm mb-2" OnClick="btnAgregarFamilia_Click" />
                                    <asp:Button ID="btnQuitarFamilia" runat="server" Text="<<" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnQuitarFamilia_Click" />
                                </div>
                                <div class="col-5">
                                    <small class="text-muted"><%: Traductor.T("comun.seleccionadas") %></small>
                                    <asp:ListBox ID="lstFamiliasSeleccionadas" runat="server" CssClass="form-select" SelectionMode="Multiple" Rows="8"></asp:ListBox>
                                </div>
                            </div>

                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal"><%: Traductor.T("comun.cancelar") %></button>
                            <asp:Button ID="btnGuardar" runat="server" ValidationGroup="Perfil" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

</asp:Content>
