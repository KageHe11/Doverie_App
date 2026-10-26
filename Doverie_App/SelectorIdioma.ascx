<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SelectorIdioma.ascx.cs" Inherits="SelectorIdioma" %>

<div class="d-flex align-items-center me-3">
    <label for="<%= ddlIdioma.ClientID %>" class="visually-hidden"><%: Traductor.T("nav.idioma") %></label>
    <asp:DropDownList ID="ddlIdioma" runat="server" CssClass="form-select form-select-sm"
        DataTextField="Nombre" DataValueField="Id" AutoPostBack="true"
        OnSelectedIndexChanged="ddlIdioma_SelectedIndexChanged" />
</div>
