<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <h1><%: Traductor.T("inicio.titulo") %></h1>
    <button class="btn btn-primary"><%: Traductor.T("inicio.probar") %></button>
</asp:Content>

