<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Estadisticas.aspx.cs" Inherits="Estadisticas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <h2 class="mb-3"><%: Traductor.T("est.titulo") %></h2>

    <%-- CU-002-012: panel general (pasos 2, 3 y 4) --%>
    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block" Visible="false"></asp:Label>
    <asp:Panel ID="pnlResumen" runat="server" CssClass="row g-3 mb-4">
        <div class="col-md-4">
            <div class="card text-center h-100"><div class="card-body">
                <div class="display-6"><asp:Literal ID="litUsuariosActivos" runat="server" /></div>
                <div class="text-muted"><%: Traductor.T("est.usuariosActivos") %></div>
            </div></div>
        </div>
        <div class="col-md-4">
            <div class="card text-center h-100"><div class="card-body">
                <div class="display-6"><asp:Literal ID="litDocumentosAlmacenados" runat="server" /></div>
                <div class="text-muted"><%: Traductor.T("est.documentosAlmacenados") %></div>
            </div></div>
        </div>
        <div class="col-md-4">
            <div class="card text-center h-100"><div class="card-body">
                <div class="display-6"><asp:Literal ID="litOperacionesBlockchain" runat="server" /></div>
                <div class="text-muted"><%: Traductor.T("est.operacionesBlockchain") %></div>
            </div></div>
        </div>
    </asp:Panel>

    <%-- Métricas por período: se consultan a los webservices WS_Documentos,
         WS_Usuarios y WS_Blockchain (anexo 10.12.1), que responden en XML. --%>
    <div class="card">
        <div class="card-body">
            <h5 class="card-title"><%: Traductor.T("est.porPeriodo") %></h5>
            <p class="text-muted small"><%: Traductor.T("est.porPeriodoAyuda") %></p>
            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-3">
                    <label class="form-label"><%: Traductor.T("bitacora.desde") %></label>
                    <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control" TextMode="Date" ClientIDMode="Static" />
                </div>
                <div class="col-md-3">
                    <label class="form-label"><%: Traductor.T("bitacora.hasta") %></label>
                    <asp:TextBox ID="txtHasta" runat="server" CssClass="form-control" TextMode="Date" ClientIDMode="Static" />
                </div>
                <div class="col-md-4">
                    <label class="form-label"><%: Traductor.T("est.usuarioTransacciones") %></label>
                    <asp:DropDownList ID="ddlUsuario" runat="server" CssClass="form-select" ClientIDMode="Static" />
                </div>
                <div class="col-md-2">
                    <button type="button" class="btn btn-primary w-100" id="btnConsultar"><%: Traductor.T("est.consultar") %></button>
                </div>
            </div>
            <div id="errorPeriodo" class="alert alert-danger d-none"></div>

            <div class="row g-3">
                <asp:Repeater ID="rptMetricas" runat="server">
                    <ItemTemplate>
                        <div class="col-md-4">
                            <div class="card h-100 metrica" data-url='<%# Eval("Url") %>' data-archivo='<%# Eval("Archivo") %>' data-tipo='<%# Eval("Tipo") %>'>
                                <div class="card-body">
                                    <div class="small text-muted"><%# Eval("Servicio") %></div>
                                    <div class="fw-semibold"><%#: Eval("Titulo") %></div>
                                    <div class="fs-3 valor">&ndash;</div>
                                    <a class="small enlace-xml d-none" href="#"><%: Traductor.T("est.descargarXml") %></a>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <script>
        (function () {
            var textos = <%= TextosJs() %>;
            function consultar() {
                var desde = document.getElementById('txtDesde').value;
                var hasta = document.getElementById('txtHasta').value;
                var usuario = document.getElementById('ddlUsuario').value;
                var error = document.getElementById('errorPeriodo');
                error.classList.add('d-none');
                if (!desde || !hasta || desde > hasta) {
                    error.textContent = textos.periodoInvalido;
                    error.classList.remove('d-none');
                    return;
                }
                var huboError = false;
                var pendientes = [];
                document.querySelectorAll('.metrica').forEach(function (tarjeta) {
                    var tipo = tarjeta.getAttribute('data-tipo');
                    var qs = 'desde=' + encodeURIComponent(desde) + '&hasta=' + encodeURIComponent(hasta);
                    if (tipo === 'usuario') qs += '&usuarioId=' + encodeURIComponent(usuario);
                    var url = tarjeta.getAttribute('data-url') + '?' + qs;
                    var valor = tarjeta.querySelector('.valor');
                    var enlace = tarjeta.querySelector('.enlace-xml');
                    valor.textContent = '…';
                    enlace.classList.add('d-none');
                    pendientes.push(fetch(url, { credentials: 'same-origin' })
                        .then(function (r) { if (!r.ok) throw new Error(r.status); return r.text(); })
                        .then(function (xml) {
                            var raiz = new DOMParser().parseFromString(xml, 'application/xml').documentElement;
                            if (tipo === 'plan') {
                                // Plan nulo: xsi:nil="true" o sin elementos hijos.
                                var nombre = raiz.getElementsByTagName('Nombre')[0];
                                var modalidad = raiz.getElementsByTagName('Tipo')[0];
                                valor.textContent = nombre ? nombre.textContent + (modalidad ? ' - ' + modalidad.textContent : '') : textos.sinPlan;
                                if (!nombre) valor.classList.add('fs-6', 'text-muted');
                            } else {
                                valor.textContent = raiz.textContent;
                            }
                            // Salida XML del anexo 10.12.2, con su nombre de archivo.
                            enlace.href = url;
                            enlace.setAttribute('download', tarjeta.getAttribute('data-archivo'));
                            enlace.classList.remove('d-none');
                        })
                        .catch(function () { valor.textContent = '!'; huboError = true; }));
                });
                Promise.all(pendientes).then(function () {
                    if (huboError) {
                        // 2.1 No se pudo obtener la información [ERROR-040]
                        error.textContent = textos.error040;
                        error.classList.remove('d-none');
                    }
                });
            }
            document.getElementById('btnConsultar').addEventListener('click', consultar);
            consultar();
        })();
    </script>

</asp:Content>
