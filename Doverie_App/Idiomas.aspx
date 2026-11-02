<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Idiomas.aspx.cs" Inherits="Idiomas" %>
<%@ Register Src="~/ConfirmacionAccion.ascx" TagPrefix="uc" TagName="ConfirmacionAccion" %>
<%@ Register Src="~/Paginador.ascx" TagPrefix="uc" TagName="Paginador" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <uc:ConfirmacionAccion ID="Confirmacion" runat="server" />

    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h2><%: Traductor.T("idiomas.titulo") %></h2>
        <asp:LinkButton ID="btnAgregar" runat="server" CssClass="btn btn-primary" OnClick="btnAgregar_Click" />
    </div>

    <asp:UpdatePanel ID="UpdatePanelGrilla" runat="server">
        <ContentTemplate>
            <asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-success d-block" Visible="false"></asp:Label>
            <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
            <uc:Paginador ID="Paginador" runat="server" OnCambio="Paginador_Cambio" />
            <asp:GridView ID="gvIdiomas" runat="server" CssClass="table table-striped table-hover"
                AutoGenerateColumns="false" DataKeyNames="Id" OnRowCommand="gvIdiomas_RowCommand" OnRowDataBound="gvIdiomas_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="Codigo" />
                    <asp:BoundField DataField="Nombre" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:Label ID="lblPredeterminado" runat="server" CssClass="badge bg-primary" Visible='<%# Eval("EsPredeterminado") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEditar" runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                CommandName="Editar" CommandArgument='<%# Eval("Id") %>' />
                            <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="modal fade" id="modalIdioma" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <asp:UpdatePanel ID="UpdatePanelModal" runat="server">
                    <%-- La subida de archivos no funciona con postbacks asíncronos:
                         Guardar hace un postback completo. --%>
                    <Triggers>
                        <asp:PostBackTrigger ControlID="btnGuardar" />
                    </Triggers>
                    <ContentTemplate>
                        <div class="modal-header">
                            <h5 class="modal-title"><asp:Literal ID="litTituloModal" runat="server" /></h5>
                        </div>
                        <div class="modal-body">
                            <asp:HiddenField ID="hdnId" runat="server" Value="0" />
                            <asp:Label ID="lblErrorModal" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
                            <div class="row">
                                <div class="col-md-4 mb-2">
                                    <label class="form-label"><%: Traductor.T("idiomas.codigo") %></label>
                                    <asp:TextBox ID="txtCodigo" runat="server" CssClass="form-control" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvCodigo" runat="server" ControlToValidate="txtCodigo" ValidationGroup="Idioma" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                    <asp:RegularExpressionValidator ID="revCodigo" runat="server" ControlToValidate="txtCodigo" ValidationGroup="Idioma" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[a-z]{2,3}(-[A-Z]{2})?$" ErrorMessage="<%$ Traduccion:valid.codigoIdioma %>" />
                                    <div class="form-text"><%: Traductor.T("idiomas.codigoAyuda") %></div>
                                </div>
                                <div class="col-md-8 mb-2">
                                    <label class="form-label"><%: Traductor.T("idiomas.nombre") %></label>
                                    <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvNombre" runat="server" ControlToValidate="txtNombre" ValidationGroup="Idioma" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                </div>
                            </div>
                            <div class="mb-2">
                                <label class="form-label"><%: Traductor.T("idiomas.archivo") %></label>
                                <asp:FileUpload ID="fuArchivo" runat="server" CssClass="form-control" accept=".json,application/json" />
                                <div class="form-text"><asp:Literal ID="litAyudaArchivo" runat="server" /></div>
                            </div>
                            <asp:Panel ID="pnlPredeterminado" runat="server" CssClass="form-check mb-2">
                                <asp:CheckBox ID="chkPredeterminado" runat="server" />
                                <label class="form-check-label" for="<%= chkPredeterminado.ClientID %>"><%: Traductor.T("idiomas.marcarPredeterminado") %></label>
                            </asp:Panel>
                            <asp:Panel ID="pnlJsonActual" runat="server" Visible="false" CssClass="mt-3">
                                <label class="form-label"><%: Traductor.T("idiomas.jsonActual") %></label>
                                <pre class="border rounded bg-light p-2 small" style="max-height: 240px; overflow: auto;"><asp:Literal ID="litJsonActual" runat="server" Mode="Encode" /></pre>
                            </asp:Panel>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal"><%: Traductor.T("comun.cancelar") %></button>
                            <asp:Button ID="btnGuardar" runat="server" ValidationGroup="Idioma" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

</asp:Content>
