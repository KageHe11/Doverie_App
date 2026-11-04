using System;
using BE;

namespace SERVICE
{
    /// Context (patrón Strategy): mantiene la estrategia de traducción
    /// activa y expone Traducir() al resto del sistema, sin saber qué
    /// idioma tiene cargado. Hay una instancia por sesión de usuario.
    [Serializable]
    public class ContextoIdioma
    {
        private ITraduccion _estrategia;
        // Estrategia del idioma predeterminado: se usa para las claves que
        // el idioma activo no tenga traducidas.
        private ITraduccion _resguardo;

        public int? IdiomaId { get; private set; }
        public string Codigo { get; private set; }
        /// Cuándo se leyeron las traducciones: la sesión las vuelve a leer
        /// cada tanto para ver los cambios de un idioma (ver Traductor).
        public DateTime Cargado { get; private set; }

        public ContextoIdioma()
        {
            Cargado = DateTime.UtcNow;
        }

        /// Carga el JSON del idioma y lo deja como estrategia activa.
        /// Lanza FormatException si el JSON no es válido.
        public void EstablecerIdioma(IdiomaBE idioma)
        {
            TraduccionJson Estrategia = new TraduccionJson();
            Estrategia.CargarIdioma(idioma.ArchivoJson);
            _estrategia = Estrategia;
            IdiomaId = idioma.Id;
            Codigo = idioma.Codigo;
        }
        public void EstablecerResguardo(IdiomaBE predeterminado)
        {
            TraduccionJson Estrategia = new TraduccionJson();
            Estrategia.CargarIdioma(predeterminado.ArchivoJson);
            _resguardo = Estrategia;
        }
        /// Busca la clave en el idioma activo, después en el predeterminado,
        /// y si ninguno la tiene devuelve la clave misma (así se nota en
        /// pantalla qué falta traducir, en lugar de mostrar un hueco).
        public string Traducir(string clave)
        {
            string Texto = _estrategia != null ? _estrategia.Traducir(clave) : null;
            if (Texto == null && _resguardo != null) Texto = _resguardo.Traducir(clave);
            return Texto ?? clave;
        }
        public string Traducir(string clave, params object[] valores)
        {
            return string.Format(Traducir(clave), valores);
        }
    }
}
