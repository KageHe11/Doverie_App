using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    public class IdiomaBLL
    {
        public List<IdiomaBE> Listar()
        {
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            return IdiomaDal.Listar();
        }
        public IdiomaBE ObtenerDatos(int id)
        {
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            return IdiomaDal.ObtenerDatos(id);
        }
        public IdiomaBE ObtenerPredeterminado()
        {
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            return IdiomaDal.ObtenerPredeterminado();
        }
        /// RC-004 Traducir: arma el contexto de traducción para un Usuario.
        /// Usa su idioma preferido o, si no tiene (o no se puede cargar),
        /// el predeterminado [ERROR-057].
        public ContextoIdioma CrearContexto(int? idiomaId)
        {
            ContextoIdioma Contexto = new ContextoIdioma();
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            IdiomaBE Predeterminado = IdiomaDal.ObtenerPredeterminado();
            if (Predeterminado == null)
            {
                // Sin idiomas cargados: el contexto devuelve las claves tal cual.
                return Contexto;
            }
            try
            {
                Contexto.EstablecerResguardo(Predeterminado);
                Contexto.EstablecerIdioma(Predeterminado);
            }
            catch (FormatException)
            {
                // El JSON del predeterminado está dañado: sin resguardo
                // posible, el contexto devuelve las claves tal cual.
                return new ContextoIdioma();
            }
            if (idiomaId.HasValue && idiomaId.Value != Predeterminado.Id)
            {
                IdiomaBE Preferido = IdiomaDal.ObtenerDatos(idiomaId.Value);
                if (Preferido != null)
                {
                    try
                    {
                        Contexto.EstablecerIdioma(Preferido);
                    }
                    catch (FormatException)
                    {
                        // [ERROR-057] El JSON del idioma elegido no se pudo
                        // cargar: queda el predeterminado como resguardo.
                    }
                }
            }
            return Contexto;
        }
        /// CU-002-018 Alta Idioma.
        public ResultadoIdioma Registrar(IdiomaBE idioma, int usuarioId, out List<string> clavesFaltantes)
        {
            clavesFaltantes = new List<string>();
            // 6. Validación de datos [ERROR-004]
            if (string.IsNullOrWhiteSpace(idioma.Codigo) || string.IsNullOrWhiteSpace(idioma.Nombre) ||
                idioma.Nombre.Length > 50 || !Regex.IsMatch(idioma.Codigo, @"^[a-z]{2,3}(-[A-Z]{2})?$"))
            {
                return ResultadoIdioma.DatosInvalidos;
            }
            // 7. Validación del JSON y sus claves requeridas [ERROR-050]
            if (!ValidarJson(idioma.ArchivoJson, out clavesFaltantes))
            {
                return ResultadoIdioma.JsonInvalido;
            }
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            // 8. Verificar que el idioma no exista [ERROR-051]
            if (IdiomaDal.ExistePorCodigo(idioma.Codigo))
            {
                return ResultadoIdioma.YaExiste;
            }
            // 9. Crear el idioma [ERROR-052]
            try
            {
                IdiomaDal.Registrar(idioma);
            }
            catch (Exception)
            {
                return ResultadoIdioma.ErrorAlta;
            }
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Alta Idioma", "Idioma: " + idioma.Nombre + " (" + idioma.Codigo + ")", usuarioId);
            return ResultadoIdioma.Exitoso;
        }
        /// CU-002-019 Modificar Idioma. Si archivoJsonNuevo viene vacío se
        /// conservan las traducciones actuales.
        public ResultadoIdioma Modificar(int id, string nombre, string archivoJsonNuevo, bool marcarPredeterminado, int usuarioId, out List<string> clavesFaltantes)
        {
            clavesFaltantes = new List<string>();
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            IdiomaBE Idioma = IdiomaDal.ObtenerDatos(id);
            // 6. Validación de datos [ERROR-004]
            if (Idioma == null || string.IsNullOrWhiteSpace(nombre) || nombre.Length > 50)
            {
                return ResultadoIdioma.DatosInvalidos;
            }
            string ArchivoJson = string.IsNullOrWhiteSpace(archivoJsonNuevo) ? Idioma.ArchivoJson : archivoJsonNuevo;
            // 7. Validación del JSON [ERROR-050]. También se valida si solo
            // se lo marca como predeterminado: el nuevo predeterminado tiene
            // que tener todas las claves del actual.
            if (!string.IsNullOrWhiteSpace(archivoJsonNuevo) || (marcarPredeterminado && !Idioma.EsPredeterminado))
            {
                if (!ValidarJson(ArchivoJson, out clavesFaltantes))
                {
                    return ResultadoIdioma.JsonInvalido;
                }
            }
            // 8. Actualizar el idioma [ERROR-053]
            try
            {
                Idioma.Nombre = nombre;
                Idioma.ArchivoJson = ArchivoJson;
                IdiomaDal.ActualizarDatos(Idioma);
                if (marcarPredeterminado && !Idioma.EsPredeterminado)
                {
                    IdiomaDal.EstablecerPredeterminado(Idioma.Id);
                }
            }
            catch (Exception)
            {
                return ResultadoIdioma.ErrorModificacion;
            }
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Modificar Idioma", "Idioma: " + Idioma.Nombre + " (" + Idioma.Codigo + ")" + (marcarPredeterminado && !Idioma.EsPredeterminado ? " - nuevo predeterminado" : ""), usuarioId);
            return ResultadoIdioma.Exitoso;
        }
        /// CU-002-020 Baja Idioma.
        public ResultadoIdioma Eliminar(int id, int usuarioId)
        {
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            IdiomaBE Idioma = IdiomaDal.ObtenerDatos(id);
            if (Idioma == null)
            {
                return ResultadoIdioma.DatosInvalidos;
            }
            // 4. No puede ser el predeterminado ni el único [ERROR-054]
            if (Idioma.EsPredeterminado || IdiomaDal.Listar().Count <= 1)
            {
                return ResultadoIdioma.NoEliminable;
            }
            // 5. Eliminar el idioma [ERROR-055]
            try
            {
                IdiomaDal.Eliminar(id);
            }
            catch (Exception)
            {
                return ResultadoIdioma.ErrorBaja;
            }
            BitacoraBLL BitacoraBll = new BitacoraBLL();
            BitacoraBll.Registrar("Baja Idioma", "Idioma: " + Idioma.Nombre + " (" + Idioma.Codigo + ")", usuarioId);
            return ResultadoIdioma.Exitoso;
        }
        /// Valida el formato del JSON y que contenga todas las claves
        /// requeridas, que son las del idioma predeterminado actual. Así,
        /// las claves nuevas se agregan en el predeterminado y el resto de
        /// los idiomas tiene que acompañar; ninguna clave se puede perder.
        private bool ValidarJson(string archivoJson, out List<string> clavesFaltantes)
        {
            clavesFaltantes = new List<string>();
            Dictionary<string, string> Textos;
            try
            {
                Textos = TraduccionJson.Parsear(archivoJson);
            }
            catch (FormatException)
            {
                return false;
            }
            IdiomaDAL IdiomaDal = new IdiomaDAL();
            IdiomaBE Predeterminado = IdiomaDal.ObtenerPredeterminado();
            if (Predeterminado == null)
            {
                // Primer idioma del sistema: no hay contra qué comparar.
                return Textos.Count > 0;
            }
            Dictionary<string, string> Requeridas;
            try
            {
                Requeridas = TraduccionJson.Parsear(Predeterminado.ArchivoJson);
            }
            catch (FormatException)
            {
                // El predeterminado guardado está dañado: no puede ser la
                // referencia, se acepta cualquier JSON con formato válido.
                return Textos.Count > 0;
            }
            clavesFaltantes = Requeridas.Keys.Where(Clave => !Textos.ContainsKey(Clave)).OrderBy(Clave => Clave).ToList();
            return clavesFaltantes.Count == 0;
        }
    }
}
