<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ArbolPermisos.ascx.cs" Inherits="ArbolPermisos" %>

<%-- Árbol de solo lectura de Familias (carpeta) y Permisos (llave). La
     página lo llena con Mostrar(perfil) o Mostrar(familias). --%>
<asp:TreeView ID="tvArbol" runat="server" ShowLines="true"></asp:TreeView>
<asp:Label ID="lblVacio" runat="server" CssClass="text-muted small" Visible="false" />
