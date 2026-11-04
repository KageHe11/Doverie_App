<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" MaintainScrollPositionOnPostback="true" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        html { scroll-behavior: smooth; }
        body { overflow-x: hidden; }
        /* Las bandas ocupan todo el ancho aunque la página esté dentro del .container del master. */
        .banda { margin-left: calc(50% - 50vw); margin-right: calc(50% - 50vw); padding: 4.5rem 0; }
        .banda-oscura { background: #0b1f3a; color: #fff; }
        .banda-clara { background: #f4f7fb; }
        section[id] { scroll-margin-top: 1rem; }
        .hero { margin-top: -1.5rem; padding: 6rem 0 5rem; color: #fff;
            background: linear-gradient(90deg, rgba(11,31,58,.97) 0%, rgba(11,31,58,.85) 42%, rgba(11,31,58,.15) 100%), url('/Content/img/fondo-blockchain.svg') center / cover no-repeat, #0b1f3a; }
        .hero .lead { color: #c9d8ea; }
        .etiqueta { display: inline-block; padding: .35rem .8rem; border-radius: 2rem; font-size: .8rem; letter-spacing: .04em;
            background: rgba(127,209,199,.12); color: #8fdcd2; border: 1px solid rgba(127,209,199,.35); }
        .sello { color: #c9d8ea; font-size: .9rem; }
        .sello span { color: #f5a623; margin-right: .35rem; }
        .titulo-seccion { font-weight: 700; }
        .subtitulo-seccion { color: #5b6b7f; max-width: 46rem; margin: .5rem auto 0; }
        .paso-numero { width: 2.75rem; height: 2.75rem; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center;
            background: #1a5fa8; color: #fff; font-weight: 700; font-size: 1.1rem; }
        .icono-funcion { width: 3rem; height: 3rem; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center;
            background: #e7f0fa; font-size: 1.5rem; }
        .tarjeta { border: 1px solid #e3e9f1; border-radius: 1rem; transition: transform .15s, box-shadow .15s; }
        .tarjeta:hover { transform: translateY(-3px); box-shadow: 0 .75rem 1.5rem rgba(11,31,58,.08); }
        .plan-destacado { border: 2px solid #1a5fa8; }
        .plan-precio { font-size: 2.1rem; font-weight: 700; color: #0b1f3a; }
        .plan-lista li { padding: .3rem 0; }
        .plan-lista li::before { content: "\2713"; color: #1a5fa8; font-weight: 700; margin-right: .5rem; }
        .banda-oscura .text-muted-claro { color: #9fb3cb; }
        .dato-contacto { display: flex; gap: .9rem; align-items: flex-start; margin-bottom: 1.25rem; }
        .dato-contacto .icono-funcion { background: rgba(255,255,255,.08); flex-shrink: 0; }
        .dato-contacto a { color: #8fdcd2; text-decoration: none; }
        .campo-oculto { position: absolute; left: -10000px; width: 1px; height: 1px; overflow: hidden; }
        .pie { margin-left: calc(50% - 50vw); margin-right: calc(50% - 50vw); margin-bottom: -1rem; background: #071528; color: #8ea3bd; padding: 2rem 0; font-size: .9rem; }
        .pie a { color: #c9d8ea; text-decoration: none; }
        .banner-usuario { margin-top: -1.5rem; padding: 2.5rem 0; color: #fff;
            background: linear-gradient(90deg, rgba(11,31,58,.96) 0%, rgba(11,31,58,.6) 100%), url('/Content/img/fondo-blockchain.svg') center / cover no-repeat, #0b1f3a; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <%-- ======================= Sin sesión: página pública ======================= --%>
    <asp:Panel ID="pnlVisitante" runat="server">

        <%-- Portada --%>
        <section class="banda hero">
            <div class="container">
                <div class="row">
                    <div class="col-lg-7">
                        <span class="etiqueta"><%: Traductor.T("home.etiqueta") %></span>
                        <h1 class="display-5 fw-bold mt-3"><%: Traductor.T("inicio.lema") %></h1>
                        <p class="lead mt-3"><%: Traductor.T("inicio.descripcion") %></p>
                        <div class="d-grid gap-2 d-sm-flex mt-4">
                            <a class="btn btn-primary btn-lg px-4" href="/Registro.aspx"><%: Traductor.T("inicio.crearCuenta") %></a>
                            <a class="btn btn-outline-light btn-lg px-4" href="#planes"><%: Traductor.T("home.verPlanes") %></a>
                        </div>
                        <div class="d-flex flex-wrap gap-4 mt-4 sello">
                            <div><span>&#10004;</span><%: Traductor.T("home.sello.hash") %></div>
                            <div><span>&#10004;</span><%: Traductor.T("home.sello.blockchain") %></div>
                            <div><span>&#10004;</span><%: Traductor.T("home.sello.cifrado") %></div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <%-- Cómo funciona --%>
        <section id="como-funciona" class="banda">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 class="titulo-seccion"><%: Traductor.T("home.pasos.titulo") %></h2>
                    <p class="subtitulo-seccion"><%: Traductor.T("home.pasos.sub") %></p>
                </div>
                <div class="row g-4 text-center">
                    <div class="col-md-4">
                        <div class="paso-numero mb-3">1</div>
                        <h3 class="h5"><%: Traductor.T("home.paso1.titulo") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.paso1.texto") %></p>
                    </div>
                    <div class="col-md-4">
                        <div class="paso-numero mb-3">2</div>
                        <h3 class="h5"><%: Traductor.T("home.paso2.titulo") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.paso2.texto") %></p>
                    </div>
                    <div class="col-md-4">
                        <div class="paso-numero mb-3">3</div>
                        <h3 class="h5"><%: Traductor.T("home.paso3.titulo") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.paso3.texto") %></p>
                    </div>
                </div>
            </div>
        </section>

        <%-- Funcionalidades --%>
        <section id="funcionalidades" class="banda banda-clara">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 class="titulo-seccion"><%: Traductor.T("home.funciones.titulo") %></h2>
                    <p class="subtitulo-seccion"><%: Traductor.T("home.funciones.sub") %></p>
                </div>
                <div class="row g-4">
                    <asp:Repeater ID="rptFunciones" runat="server">
                        <ItemTemplate>
                            <div class="col-md-6 col-lg-4">
                                <div class="card tarjeta h-100 p-2">
                                    <div class="card-body">
                                        <div class="icono-funcion mb-3" aria-hidden="true"><%# Eval("Icono") %></div>
                                        <h3 class="h5"><%#: Eval("Titulo") %></h3>
                                        <p class="text-muted mb-0"><%#: Eval("Texto") %></p>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </section>

        <%-- Para quién --%>
        <section class="banda">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 class="titulo-seccion"><%: Traductor.T("home.publico.titulo") %></h2>
                </div>
                <div class="row g-4">
                    <div class="col-md-4">
                        <h3 class="h5"><span class="me-2" aria-hidden="true">&#9878;&#65039;</span><%: Traductor.T("home.publico.escribanias") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.publico.escribaniasTexto") %></p>
                    </div>
                    <div class="col-md-4">
                        <h3 class="h5"><span class="me-2" aria-hidden="true">&#128188;</span><%: Traductor.T("home.publico.estudios") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.publico.estudiosTexto") %></p>
                    </div>
                    <div class="col-md-4">
                        <h3 class="h5"><span class="me-2" aria-hidden="true">&#128187;</span><%: Traductor.T("home.publico.freelancers") %></h3>
                        <p class="text-muted"><%: Traductor.T("home.publico.freelancersTexto") %></p>
                    </div>
                </div>
            </div>
        </section>

        <%-- Planes (precios de la base) --%>
        <section id="planes" class="banda banda-clara">
            <div class="container">
                <div class="text-center mb-5">
                    <h2 class="titulo-seccion"><%: Traductor.T("home.planes.titulo") %></h2>
                    <p class="subtitulo-seccion"><%: Traductor.T("home.planes.sub") %></p>
                </div>
                <div class="row g-4 justify-content-center">
                    <asp:Repeater ID="rptPlanes" runat="server">
                        <ItemTemplate>
                            <div class="col-md-6 col-lg-4">
                                <div class='card tarjeta h-100 p-2 <%# (bool)Eval("Destacado") ? "plan-destacado" : "" %>'>
                                    <div class="card-body d-flex flex-column">
                                        <div class="d-flex justify-content-between align-items-center">
                                            <h3 class="h4 mb-0"><%#: Eval("Nombre") %></h3>
                                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Destacado") %>'>
                                                <span class="badge bg-primary"><%#: Traductor.T("home.planes.destacado") %></span>
                                            </asp:PlaceHolder>
                                        </div>
                                        <p class="text-muted small mt-1"><%#: Eval("Subtitulo") %></p>
                                        <div class="mt-2">
                                            <span class="text-muted small"><%#: Traductor.T("home.planes.desde") %></span>
                                            <div class="plan-precio"><%#: Eval("Precio") %></div>
                                            <span class="text-muted small"><%#: Traductor.T("home.planes.nota") %></span>
                                        </div>
                                        <ul class="list-unstyled plan-lista mt-3 mb-4">
                                            <asp:Repeater runat="server" DataSource='<%# Eval("Caracteristicas") %>'>
                                                <ItemTemplate><li><%#: Container.DataItem %></li></ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                        <a class='btn mt-auto <%# (bool)Eval("Destacado") ? "btn-primary" : "btn-outline-primary" %>' href="/Registro.aspx"><%#: Traductor.T("home.planes.elegir") %></a>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </section>

        <%-- Sobre nosotros --%>
        <section id="nosotros" class="banda">
            <div class="container">
                <div class="row g-5 align-items-center">
                    <div class="col-lg-6">
                        <h2 class="titulo-seccion"><%: Traductor.T("home.nosotros.titulo") %></h2>
                        <p class="mt-3"><%: Traductor.T("home.nosotros.texto") %></p>
                        <p class="text-muted"><%: Traductor.T("home.nosotros.fundador") %></p>
                    </div>
                    <div class="col-lg-6">
                        <div class="card tarjeta mb-3">
                            <div class="card-body">
                                <h3 class="h5"><span class="me-2" aria-hidden="true">&#127919;</span><%: Traductor.T("home.mision.titulo") %></h3>
                                <p class="text-muted mb-0"><%: Traductor.T("home.mision.texto") %></p>
                            </div>
                        </div>
                        <div class="card tarjeta">
                            <div class="card-body">
                                <h3 class="h5"><span class="me-2" aria-hidden="true">&#128301;</span><%: Traductor.T("home.vision.titulo") %></h3>
                                <p class="text-muted mb-0"><%: Traductor.T("home.vision.texto") %></p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <%-- Contacto --%>
        <section id="contacto" class="banda banda-oscura">
            <div class="container">
                <div class="row g-5">
                    <div class="col-lg-5">
                        <h2 class="titulo-seccion"><%: Traductor.T("home.contacto.titulo") %></h2>
                        <p class="text-muted-claro mt-3 mb-4"><%: Traductor.T("home.contacto.sub") %></p>
                        <div class="dato-contacto">
                            <div class="icono-funcion" aria-hidden="true">&#9993;&#65039;</div>
                            <div><div class="small text-muted-claro"><%: Traductor.T("comun.email") %></div><a href="mailto:<%: BLL.ContactoBLL.EmailContacto %>"><%: BLL.ContactoBLL.EmailContacto %></a></div>
                        </div>
                        <div class="dato-contacto">
                            <div class="icono-funcion" aria-hidden="true">in</div>
                            <div><div class="small text-muted-claro">LinkedIn</div><a href="<%: UrlLinkedIn %>" target="_blank" rel="noopener">Doverie</a></div>
                        </div>
                        <div class="dato-contacto">
                            <div class="icono-funcion" aria-hidden="true">&#128205;</div>
                            <div><div class="small text-muted-claro"><%: Traductor.T("home.contacto.ubicacion") %></div><%: Traductor.T("home.contacto.ubicacionTexto") %></div>
                        </div>
                    </div>
                    <div class="col-lg-7">
                        <div class="card text-dark">
                            <div class="card-body p-4">
                                <uc:Mensaje ID="Mensaje" runat="server" />
                                <div class="row g-3">
                                    <div class="col-md-6">
                                        <label class="form-label" for="<%= txtNombreContacto.ClientID %>"><%: Traductor.T("home.contacto.nombre") %></label>
                                        <asp:TextBox ID="txtNombreContacto" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvNombreContacto" runat="server" ControlToValidate="txtNombreContacto" ValidationGroup="Contacto" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                    </div>
                                    <div class="col-md-6">
                                        <label class="form-label" for="<%= txtEmailContacto.ClientID %>"><%: Traductor.T("comun.email") %></label>
                                        <asp:TextBox ID="txtEmailContacto" runat="server" CssClass="form-control" MaxLength="100" TextMode="Email"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvEmailContacto" runat="server" ControlToValidate="txtEmailContacto" ValidationGroup="Contacto" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                        <asp:RegularExpressionValidator ID="revEmailContacto" runat="server" ControlToValidate="txtEmailContacto" ValidationGroup="Contacto" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="<%$ Traduccion:valid.email %>" />
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label" for="<%= txtMensajeContacto.ClientID %>"><%: Traductor.T("home.contacto.mensaje") %></label>
                                        <asp:TextBox ID="txtMensajeContacto" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" MaxLength="2000"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvMensajeContacto" runat="server" ControlToValidate="txtMensajeContacto" ValidationGroup="Contacto" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                                        <asp:RegularExpressionValidator ID="revMensajeContacto" runat="server" ControlToValidate="txtMensajeContacto" ValidationGroup="Contacto" Display="Dynamic" CssClass="text-danger small" ValidationExpression="^[\s\S]{0,2000}$" ErrorMessage="<%$ Traduccion:home.contacto.largo %>" />
                                    </div>
                                    <%-- Trampa para bots: un campo que una persona no ve ni completa. --%>
                                    <div class="campo-oculto" aria-hidden="true">
                                        <asp:TextBox ID="txtSitioWeb" runat="server" TabIndex="-1" autocomplete="off"></asp:TextBox>
                                    </div>
                                    <div class="col-12">
                                        <asp:Button ID="btnEnviarContacto" runat="server" CssClass="btn btn-primary btn-lg px-4" ValidationGroup="Contacto" OnClick="btnEnviarContacto_Click" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <footer class="pie">
            <div class="container d-flex flex-column flex-md-row justify-content-between align-items-center gap-2">
                <div class="d-flex align-items-center gap-2">
                    <img src="/Content/img/logo.png" alt="" height="26" />
                    <span><%: Traductor.T("home.pie", DateTime.Now.Year) %></span>
                </div>
                <div class="d-flex gap-3">
                    <a href="#funcionalidades"><%: Traductor.T("home.nav.funciones") %></a>
                    <a href="#planes"><%: Traductor.T("home.nav.planes") %></a>
                    <a href="#nosotros"><%: Traductor.T("home.nav.nosotros") %></a>
                    <a href="#contacto"><%: Traductor.T("home.nav.contacto") %></a>
                </div>
            </div>
        </footer>
    </asp:Panel>

    <%-- ======================= Con sesión: accesos ======================= --%>
    <asp:Panel ID="pnlUsuario" runat="server" Visible="false">
        <div class="banda banner-usuario mb-4">
            <div class="container">
                <h1 class="h3 mb-1"><asp:Literal ID="litSaludo" runat="server" Mode="Encode" /></h1>
                <p class="mb-0" style="color:#c9d8ea"><%: Traductor.T("inicio.accesos") %></p>
            </div>
        </div>
        <div class="row g-3 pb-4">
            <asp:Repeater ID="rptAccesos" runat="server">
                <ItemTemplate>
                    <div class="col-sm-6 col-lg-4">
                        <div class="card tarjeta h-100">
                            <div class="card-body d-flex">
                                <div class="icono-funcion me-3" aria-hidden="true"><%# Eval("Icono") %></div>
                                <div>
                                    <h2 class="h6 mb-1"><a class="stretched-link text-decoration-none" href="<%#: Eval("Url") %>"><%#: Eval("Titulo") %></a></h2>
                                    <p class="small text-muted mb-0"><%#: Eval("Descripcion") %></p>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

</asp:Content>
