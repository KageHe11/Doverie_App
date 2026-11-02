<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <%-- Sin sesión: qué es Doverie y cómo entrar. --%>
    <asp:Panel ID="pnlVisitante" runat="server">
        <div class="py-5 text-center">
            <img src="/Content/img/logo.png" alt="Doverie" height="72" class="mb-4" />
            <h1 class="display-6 fw-bold"><%: Traductor.T("inicio.lema") %></h1>
            <p class="lead text-muted col-lg-8 mx-auto"><%: Traductor.T("inicio.descripcion") %></p>
            <div class="d-grid gap-2 d-sm-flex justify-content-sm-center mt-4">
                <a class="btn btn-primary btn-lg px-4" href="/Registro.aspx"><%: Traductor.T("inicio.crearCuenta") %></a>
                <a class="btn btn-outline-secondary btn-lg px-4" href="/Login.aspx"><%: Traductor.T("nav.iniciarSesion") %></a>
            </div>
        </div>
        <div class="row g-4 pb-5">
            <div class="col-md-4">
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <div class="fs-2 mb-2" aria-hidden="true">&#128279;</div>
                        <h2 class="h5"><%: Traductor.T("inicio.blockchainTitulo") %></h2>
                        <p class="text-muted mb-0"><%: Traductor.T("inicio.blockchainTexto") %></p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <div class="fs-2 mb-2" aria-hidden="true">&#128274;</div>
                        <h2 class="h5"><%: Traductor.T("inicio.seguridadTitulo") %></h2>
                        <p class="text-muted mb-0"><%: Traductor.T("inicio.seguridadTexto") %></p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <div class="fs-2 mb-2" aria-hidden="true">&#128228;</div>
                        <h2 class="h5"><%: Traductor.T("inicio.compartirTitulo") %></h2>
                        <p class="text-muted mb-0"><%: Traductor.T("inicio.compartirTexto") %></p>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <%-- Con sesión: accesos a lo que el Perfil del Usuario permite. --%>
    <asp:Panel ID="pnlUsuario" runat="server" Visible="false">
        <h1 class="h3 mb-1"><asp:Literal ID="litSaludo" runat="server" Mode="Encode" /></h1>
        <p class="text-muted mb-4"><%: Traductor.T("inicio.accesos") %></p>
        <div class="row g-3 pb-4">
            <asp:Repeater ID="rptAccesos" runat="server">
                <ItemTemplate>
                    <div class="col-sm-6 col-lg-4">
                        <div class="card h-100 shadow-sm">
                            <div class="card-body d-flex">
                                <div class="fs-3 me-3" aria-hidden="true"><%# Eval("Icono") %></div>
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
