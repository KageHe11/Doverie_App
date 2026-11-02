using System.Collections.Generic;

namespace BE
{
    /// Eventos de la plataforma a los que un Usuario se puede suscribir
    /// (CU-001-015), agrupados por categoría, y los canales disponibles.
    public static class CatalogoEventos
    {
        // Categorías
        public const string CategoriaDocumentos = "Documentos";
        public const string CategoriaSuscripcion = "Suscripcion";
        public const string CategoriaAccesos = "Accesos";

        // Documentos
        public const string DocumentoSubido = "DocumentoSubido";
        public const string DocumentoAccedido = "DocumentoAccedido";
        public const string ModificacionDetectada = "ModificacionDetectada";
        // Suscripción
        public const string SuscripcionVencimiento = "SuscripcionVencimiento";
        public const string SuscripcionRenovacion = "SuscripcionRenovacion";
        public const string RenovacionPendiente = "RenovacionPendiente";
        public const string PagoRechazado = "PagoRechazado";
        // Accesos
        public const string InicioSesionNuevoDispositivo = "InicioSesionNuevoDispositivo";
        public const string PermisosModificados = "PermisosModificados";
        public const string CuentaReactivada = "CuentaReactivada";

        // Canales: Email = correo electrónico, Push = dentro de la plataforma.
        public const string CanalEmail = "Email";
        public const string CanalPush = "Push";

        /// Eventos en el orden en que se muestran, con su categoría.
        public static readonly IList<KeyValuePair<string, string>> Eventos = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(DocumentoSubido, CategoriaDocumentos),
            new KeyValuePair<string, string>(DocumentoAccedido, CategoriaDocumentos),
            new KeyValuePair<string, string>(ModificacionDetectada, CategoriaDocumentos),
            new KeyValuePair<string, string>(SuscripcionVencimiento, CategoriaSuscripcion),
            new KeyValuePair<string, string>(SuscripcionRenovacion, CategoriaSuscripcion),
            new KeyValuePair<string, string>(RenovacionPendiente, CategoriaSuscripcion),
            new KeyValuePair<string, string>(PagoRechazado, CategoriaSuscripcion),
            new KeyValuePair<string, string>(InicioSesionNuevoDispositivo, CategoriaAccesos),
            new KeyValuePair<string, string>(PermisosModificados, CategoriaAccesos),
            new KeyValuePair<string, string>(CuentaReactivada, CategoriaAccesos)
        };
    }
}
