<%@ Application Language="C#" %>

<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        // Los validadores de los formularios usan su propio script
        // (WebUIValidation.js) en lugar de la validación "unobtrusive", que
        // exige jQuery: el sitio solo usa Bootstrap.
        System.Web.UI.ValidationSettings.UnobtrusiveValidationMode = System.Web.UI.UnobtrusiveValidationMode.None;
    }
</script>
