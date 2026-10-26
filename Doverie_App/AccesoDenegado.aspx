<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AccesoDenegado.aspx.cs" Inherits="AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="text-center mt-5">
        <h2><%: Traductor.T("acceso.titulo") %></h2>
        <p class="text-muted"><%: Traductor.T("acceso.mensaje") %></p>
        <a href="/Default.aspx" class="btn btn-primary"><%: Traductor.T("acceso.volver") %></a>
    </div>

</asp:Content>