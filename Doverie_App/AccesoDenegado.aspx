<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="AccesoDenegado.aspx.cs" Inherits="AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

    <div class="text-center mt-5">
        <h2>Acceso denegado</h2>
        <p class="text-muted">No tenés los permisos necesarios para ver esta página.</p>
        <a href="/Default.aspx" class="btn btn-primary">Volver al inicio</a>
    </div>

</asp:Content>