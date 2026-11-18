<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<%@ Register Src="~/Mensaje.ascx" TagPrefix="uc" TagName="Mensaje" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        body { overflow-x: hidden; }
        /* Ocupa todo el ancho y el alto disponible aunque la página esté dentro del .container del master. */
        .acceso { margin: -1.5rem calc(50% - 50vw) 0; min-height: calc(100vh - 56px); display: flex; align-items: center; padding: 3rem 0;
            background: linear-gradient(90deg, rgba(11,31,58,.97) 0%, rgba(11,31,58,.88) 45%, rgba(11,31,58,.55) 100%), url('/Content/img/fondo-blockchain.svg') center / cover no-repeat, #0b1f3a; }
        .acceso-texto { color: #fff; }
        .acceso-texto .lead { color: #c9d8ea; }
        .etiqueta { display: inline-block; padding: .35rem .8rem; border-radius: 2rem; font-size: .8rem; letter-spacing: .04em;
            background: rgba(127,209,199,.12); color: #8fdcd2; border: 1px solid rgba(127,209,199,.35); }
        .sello { color: #c9d8ea; font-size: .95rem; margin-top: .6rem; }
        .sello span { color: #f5a623; margin-right: .5rem; }
        .tarjeta-acceso { border: 0; border-radius: 1.25rem; box-shadow: 0 1.5rem 3rem rgba(0,0,0,.35); }
        .tarjeta-acceso .logo { background: #0b1f3a; border-radius: .75rem; padding: .5rem .9rem; display: inline-block; }
        .tarjeta-acceso .form-control { border-radius: .65rem; }
        .tarjeta-acceso .form-control:focus { border-color: #1a5fa8; box-shadow: 0 0 0 .2rem rgba(26,95,168,.2); }
        .boton-ingresar { border-radius: .65rem; padding: .7rem; font-weight: 600; border: 0;
            background: linear-gradient(90deg, #1a5fa8 0%, #2a7fd4 100%); }
        .boton-ingresar:hover { background: linear-gradient(90deg, #154d89 0%, #1f6dbb 100%); }
        .enlaces-acceso a { color: #1a5fa8; text-decoration: none; }
        .enlaces-acceso a:hover { text-decoration: underline; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <section class="acceso">
        <div class="container">
            <div class="row align-items-center justify-content-center g-5">

                <%-- Presentación (en pantallas chicas se oculta y queda solo el formulario) --%>
                <div class="col-lg-6 d-none d-lg-block acceso-texto">
                    <span class="etiqueta"><%: Traductor.T("home.etiqueta") %></span>
                    <h1 class="display-6 fw-bold mt-3"><%: Traductor.T("inicio.lema") %></h1>
                    <p class="lead mt-3"><%: Traductor.T("inicio.descripcion") %></p>
                    <div class="mt-4">
                        <div class="sello"><span>&#10004;</span><%: Traductor.T("home.sello.hash") %></div>
                        <div class="sello"><span>&#10004;</span><%: Traductor.T("home.sello.blockchain") %></div>
                        <div class="sello"><span>&#10004;</span><%: Traductor.T("home.sello.cifrado") %></div>
                    </div>
                </div>

                <%-- Formulario --%>
                <div class="col-sm-9 col-md-7 col-lg-5 col-xl-4">
                    <div class="card tarjeta-acceso">
                        <div class="card-body p-4 p-md-5">

                            <div class="text-center mb-4">
                                <span class="logo"><img src="/Content/img/logo.png" alt="Doverie" height="34" /></span>
                                <h2 class="h3 fw-bold mt-3 mb-0"><%: Traductor.T("login.titulo") %></h2>
                            </div>

                            <uc:Mensaje ID="Mensaje" runat="server" />

                            <div class="form-floating mb-3">
                                <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" placeholder=" " autocomplete="username"></asp:TextBox>
                                <label for="<%= txtUsuario.ClientID %>"><%: Traductor.T("login.usuario") %></label>
                                <asp:RequiredFieldValidator ID="rfvUsuario" runat="server" ControlToValidate="txtUsuario" ValidationGroup="Login" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            </div>

                            <div class="form-floating mb-4">
                                <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password" placeholder=" " autocomplete="current-password"></asp:TextBox>
                                <label for="<%= txtContrasena.ClientID %>"><%: Traductor.T("login.contrasena") %></label>
                                <asp:RequiredFieldValidator ID="rfvContrasena" runat="server" ControlToValidate="txtContrasena" ValidationGroup="Login" Display="Dynamic" CssClass="text-danger small" ErrorMessage="<%$ Traduccion:valid.obligatorio %>" />
                            </div>

                            <div class="d-grid">
                                <asp:Button ID="btnIngresar" runat="server" ValidationGroup="Login" CssClass="btn btn-primary btn-lg boton-ingresar" OnClick="btnIngresar_Click" />
                            </div>

                            <div class="enlaces-acceso text-center mt-4 small">
                                <div><a href="/RecuperarContrasena.aspx"><%: Traductor.T("login.olvideContrasena") %></a></div>
                                <div class="mt-2"><a href="/Registro.aspx"><%: Traductor.T("login.registrarse") %></a></div>
                            </div>

                        </div>
                    </div>
                </div>

            </div>
        </div>
    </section>

</asp:Content>
