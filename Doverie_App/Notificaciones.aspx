<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Notificaciones.aspx.cs" Inherits="Notificaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <h2 class="mb-3"><%: Traductor.T("notif.titulo") %></h2>

    <div class="row g-4">

        <%-- Bandeja: notificaciones recibidas dentro de la plataforma --%>
        <div class="col-lg-5">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title"><%: Traductor.T("notif.bandeja") %></h5>
                    <asp:Panel ID="pnlBandejaVacia" runat="server" CssClass="text-muted" Visible="false">
                        <%: Traductor.T("notif.bandejaVacia") %>
                    </asp:Panel>
                    <asp:Repeater ID="rptRecibidas" runat="server">
                        <HeaderTemplate><ul class="list-group list-group-flush"></HeaderTemplate>
                        <ItemTemplate>
                            <li class='list-group-item px-0 <%# (bool)Eval("Leida") ? "" : "fw-semibold" %>'>
                                <div class="d-flex justify-content-between small text-muted">
                                    <span><%#: Traductor.T("notif.evento." + Eval("Evento")) %></span>
                                    <span><%# Eval("Fecha", "{0:dd/MM/yyyy HH:mm}") %></span>
                                </div>
                                <div>
                                    <%#: Traductor.T("notif.msg." + Eval("Evento"), Eval("Detalle") ?? "") %>
                                    <%# (bool)Eval("Leida") ? "" : "<span class=\"badge bg-primary ms-1\">" + HttpUtility.HtmlEncode(Traductor.T("notif.nueva")) + "</span>" %>
                                </div>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <%-- CU-001-015 Configurar Notificaciones --%>
        <div class="col-lg-7">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title"><%: Traductor.T("notif.preferencias") %></h5>
                    <p class="text-muted small"><%: Traductor.T("notif.preferenciasAyuda") %></p>

                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false" EnableViewState="false"></asp:Label>
                    <asp:Panel ID="pnlExito" runat="server" CssClass="alert alert-success" Visible="false" EnableViewState="false">
                        <div><%: Traductor.T("notif.guardadoExitoso") %></div>
                        <asp:Literal ID="litResumen" runat="server" EnableViewState="false" />
                    </asp:Panel>

                    <table class="table table-sm align-middle" id="tablaPreferencias">
                        <thead>
                            <tr>
                                <th><%: Traductor.T("notif.evento") %></th>
                                <th class="text-center"><%: Traductor.T("notif.habilitado") %></th>
                                <th class="text-center"><%: Traductor.T("notif.canal.Email") %></th>
                                <th class="text-center"><%: Traductor.T("notif.canal.Push") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptPreferencias" runat="server" OnItemCreated="rptPreferencias_ItemCreated" OnItemDataBound="rptPreferencias_ItemDataBound">
                                <ItemTemplate>
                                    <asp:PlaceHolder ID="phCategoria" runat="server" Visible="false">
                                        <tr class="table-light"><td colspan="4" class="fw-semibold"><asp:Literal ID="litCategoria" runat="server" Mode="Encode" /></td></tr>
                                    </asp:PlaceHolder>
                                    <tr data-evento="1">
                                        <td>
                                            <asp:HiddenField ID="hdnEvento" runat="server" Value='<%# Eval("Evento") %>' />
                                            <asp:Literal ID="litEvento" runat="server" Mode="Encode" />
                                            <asp:Label ID="lblProximamente" runat="server" CssClass="badge bg-secondary ms-1" Visible="false" />
                                        </td>
                                        <td class="text-center"><asp:CheckBox ID="chkHabilitado" runat="server" Checked='<%# Eval("Habilitado") %>' /></td>
                                        <td class="text-center"><asp:CheckBox ID="chkEmail" runat="server" Checked='<%# Eval("Email") %>' /></td>
                                        <td class="text-center"><asp:CheckBox ID="chkPush" runat="server" Checked='<%# Eval("Push") %>' /></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>

                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn btn-primary" OnClick="btnGuardar_Click" />
                </div>
            </div>
        </div>
    </div>

    <%-- Paso 5 del CU: al habilitar o deshabilitar un evento, sus canales se
         activan o desactivan en pantalla. Al habilitar un evento sin canal,
         se propone "dentro de la plataforma". --%>
    <script>
        (function () {
            function sincronizar(fila, alCambiar) {
                var habilitado = fila.querySelector('input[data-rol="habilitado"]');
                var canales = fila.querySelectorAll('input[data-rol="canal"]');
                var alguno = false;
                canales.forEach(function (c) { c.disabled = !habilitado.checked; if (c.checked) alguno = true; });
                if (alCambiar && habilitado.checked && !alguno) {
                    fila.querySelector('input[data-canal="Push"]').checked = true;
                }
            }
            document.querySelectorAll('#tablaPreferencias tr[data-evento]').forEach(function (fila) {
                sincronizar(fila, false);
                fila.querySelector('input[data-rol="habilitado"]').addEventListener('change', function () { sincronizar(fila, true); });
            });
        })();
    </script>

</asp:Content>
