<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="EmpleadosEmpresa.aspx.cs" Inherits="EmpleadosEmpresa" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2><%: Traductor.T("empleados.titulo") %></h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click" />
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>
            <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false" EnableViewState="false"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>

            <asp:GridView ID="gvEmpleados" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvEmpleados_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Nombre" />
                    <asp:BoundField DataField="Apellido" />
                    <asp:BoundField DataField="NombreUsuario" />
                    <asp:BoundField DataField="Gmail" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <span class='<%# (bool)Eval("Suspendido") ? "badge bg-danger" : "badge bg-success" %>'>
                                <%# (bool)Eval("Suspendido") ? Traductor.T("comun.suspendido") : Traductor.T("comun.activo") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>' Text='<%# Traductor.T("comun.editar") %>' />

                            <%-- CU-001-016: la baja es permanente; el empleado deja de listarse. --%>
                            <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick='<%# Confirmacion.Para("empleados.confirmarEliminar") %>'
                                Text='<%# Traductor.T("comun.eliminar") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="modal fade" id="modalEmpleado" tabindex="-1">
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
                                <label class="form-label"><%: Traductor.T("comun.apellido") %></label>
                                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.nombreUsuario") %></label>
                                <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.contrasena") %></label>
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <small class="text-muted"><%: Traductor.T("comun.ayudaContrasenaEditar") %></small>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.email") %></label>
                                <asp:TextBox ID="txtGmail" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.dni") %></label>
                                <asp:TextBox ID="txtDni" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("comun.telefono") %></label>
                                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("empleados.perfil") %></label>
                                <asp:DropDownList ID="ddlPerfil" runat="server" CssClass="form-select" DataTextField="NombrePerfil" DataValueField="Id"></asp:DropDownList>
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
