using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BE;

namespace DAL
{
    /// Dígitos verificadores de todas las tablas, a tres niveles:
    /// - Registro (DVH): cada fila guarda en su columna DVH el dígito de sus
    ///   valores.
    /// - Columna: un dígito por columna, con los valores de todas las filas.
    /// - Tabla: un dígito con los DVH de todas las filas y su cantidad; detecta
    ///   filas agregadas o borradas a mano.
    /// Los dígitos son HMAC-SHA256 con la clave DigitoVerificadorClave de
    /// AppSettings, que no está en la base: sin ella no se pueden recalcular.
    ///
    /// El DAO llama a Proteger en cada SP que escribe una tabla protegida:
    /// verifica esas tablas, ejecuta el SP y recalcula sus dígitos. Si una ya
    /// estaba alterada no se recalcula, para no tapar la alteración, y la base
    /// queda marcada como comprometida.
    public class DigitoVerificadorDAL
    {
        private const string ColumnaDVH = "DVH";
        /// Una escritura protegida a la vez: si dos se mezclaran, una vería la
        /// fila de la otra todavía sin su DVH y la tomaría como alterada.
        private static readonly object Bloqueo = new object();
        private static readonly ConcurrentDictionary<string, string[]> TablasPorProcedimiento = new ConcurrentDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private static volatile bool comprometida;

        /// La última verificación encontró dígitos que no coinciden.
        public static bool BaseComprometida
        {
            get { return comprometida; }
        }

        // ---------- Gancho del DAO ----------

        internal static T Proteger<T>(string procedimiento, Func<T> ejecutar)
        {
            // Los SPs de los dígitos se ejecutan desde acá mismo.
            if (procedimiento.StartsWith("SP_DV_", StringComparison.OrdinalIgnoreCase)) return ejecutar();
            string[] Tablas = TablasModificadas(procedimiento);
            if (Tablas.Length == 0) return ejecutar();
            lock (Bloqueo)
            {
                List<string> Integras = Tablas.Where(Tabla => Verificar(Tabla).Count == 0).ToList();
                if (Integras.Count < Tablas.Length) comprometida = true;
                try
                {
                    return ejecutar();
                }
                finally
                {
                    foreach (string Tabla in Integras)
                    {
                        try
                        {
                            Recalcular(Tabla);
                        }
                        catch (Exception)
                        {
                            // La escritura ya se hizo; con los dígitos sin
                            // actualizar, lo ve el Administrador en Integridad.
                            comprometida = true;
                        }
                    }
                }
            }
        }
        /// Tablas protegidas que modifica el SP (se averigua una vez por SP).
        private static string[] TablasModificadas(string procedimiento)
        {
            return TablasPorProcedimiento.GetOrAdd(procedimiento, Sp =>
            {
                try
                {
                    DataSet Ds = new DAO().ExecuteDataSetStoredProcedure("SP_DV_TablasModificadas",
                        new Dictionary<string, object> { { "@Procedimiento", Sp } }, CommandType.StoredProcedure);
                    return Ds.Tables[0].Rows.Cast<DataRow>().Select(F => F["Tabla"].ToString()).ToArray();
                }
                catch (Exception)
                {
                    // Sin la estructura de dígitos (por ejemplo, después de
                    // restaurar un backup anterior) la escritura sigue igual.
                    return new string[0];
                }
            });
        }

        // ---------- Verificación ----------

        public List<string> ListarTablas()
        {
            DataSet Ds = new DAO().ExecuteDataSetStoredProcedure("SP_DV_ListarTablas", null, CommandType.StoredProcedure);
            return Ds.Tables[0].Rows.Cast<DataRow>().Select(F => F["Tabla"].ToString()).ToList();
        }
        /// Verifica todas las tablas y actualiza BaseComprometida.
        public List<ProblemaIntegridadBE> VerificarTodo()
        {
            lock (Bloqueo)
            {
                List<ProblemaIntegridadBE> Problemas = new List<ProblemaIntegridadBE>();
                foreach (string Tabla in ListarTablas()) Problemas.AddRange(Verificar(Tabla));
                comprometida = Problemas.Count > 0;
                return Problemas;
            }
        }
        private static List<ProblemaIntegridadBE> Verificar(string tabla)
        {
            List<ProblemaIntegridadBE> Problemas = new List<ProblemaIntegridadBE>();
            string[] Clave;
            DataTable Filas = LeerFilas(tabla, out Clave);
            Dictionary<string, string> Guardados = LeerDigitos(tabla);
            if (Guardados.Count == 0)
            {
                Problemas.Add(new ProblemaIntegridadBE { Tabla = tabla, Nivel = ProblemaIntegridadBE.NivelTabla, Detalle = "Sin dígitos calculados" });
                return Problemas;
            }
            // Registro
            List<string> DVHs = new List<string>();
            foreach (DataRow Fila in Filas.Rows)
            {
                string DVH = CalcularDVH(tabla, Fila);
                DVHs.Add(DVH);
                if (!string.Equals(Fila[ColumnaDVH] as string, DVH, StringComparison.Ordinal))
                {
                    Problemas.Add(new ProblemaIntegridadBE { Tabla = tabla, Nivel = ProblemaIntegridadBE.NivelRegistro, Detalle = DescribirClave(Fila, Clave) });
                }
            }
            // Columna
            foreach (KeyValuePair<string, string> Columna in CalcularColumnas(tabla, Filas))
            {
                string Guardado;
                if (!Guardados.TryGetValue(Columna.Key, out Guardado) || Guardado != Columna.Value)
                {
                    Problemas.Add(new ProblemaIntegridadBE { Tabla = tabla, Nivel = ProblemaIntegridadBE.NivelColumna, Detalle = Columna.Key });
                }
            }
            // Tabla
            string DigitoTabla;
            if (!Guardados.TryGetValue("", out DigitoTabla) || DigitoTabla != CalcularTabla(tabla, DVHs))
            {
                Problemas.Add(new ProblemaIntegridadBE { Tabla = tabla, Nivel = ProblemaIntegridadBE.NivelTabla, Detalle = Filas.Rows.Count + " filas" });
            }
            return Problemas;
        }

        // ---------- Recálculo ----------

        /// Recalcula los dígitos de una tabla aceptando sus datos actuales
        /// (lo usa el Administrador después de revisar una alteración).
        public void RecalcularTabla(string tabla)
        {
            lock (Bloqueo)
            {
                Recalcular(tabla);
            }
        }
        public void RecalcularTodo()
        {
            lock (Bloqueo)
            {
                foreach (string Tabla in ListarTablas()) Recalcular(Tabla);
                comprometida = false;
            }
        }
        private static void Recalcular(string tabla)
        {
            string[] Clave;
            DataTable Filas = LeerFilas(tabla, out Clave);
            List<string> DVHs = new List<string>();
            foreach (DataRow Fila in Filas.Rows)
            {
                string DVH = CalcularDVH(tabla, Fila);
                DVHs.Add(DVH);
                // Solo se escriben las filas que cambiaron.
                if (string.Equals(Fila[ColumnaDVH] as string, DVH, StringComparison.Ordinal)) continue;
                Dictionary<string, string> ValoresClave = Clave.ToDictionary(C => C, C => Texto(Fila[C]));
                new DAO().ExecuteNonQueryStoredProcedure("SP_DV_GuardarDVH", new Dictionary<string, object>
                {
                    { "@Tabla", tabla },
                    { "@Clave", Json(ValoresClave) },
                    { "@DVH", DVH }
                }, CommandType.StoredProcedure);
            }
            List<Dictionary<string, string>> Digitos = CalcularColumnas(tabla, Filas)
                .Select(C => new Dictionary<string, string> { { "Columna", C.Key }, { "DV", C.Value } }).ToList();
            Digitos.Add(new Dictionary<string, string> { { "Columna", "" }, { "DV", CalcularTabla(tabla, DVHs) } });
            new DAO().ExecuteNonQueryStoredProcedure("SP_DV_GuardarDigitos", new Dictionary<string, object>
            {
                { "@Tabla", tabla },
                { "@Json", "[" + string.Join(",", Digitos.Select(Json)) + "]" }
            }, CommandType.StoredProcedure);
        }

        // ---------- Cálculo ----------

        /// DVH: los valores de la fila (todas las columnas salvo DVH, en su
        /// orden), cada uno con su largo adelante para que no se confundan.
        private static string CalcularDVH(string tabla, DataRow fila)
        {
            StringBuilder Datos = new StringBuilder(tabla).Append('|');
            foreach (DataColumn Columna in fila.Table.Columns)
            {
                if (Columna.ColumnName == ColumnaDVH) continue;
                Agregar(Datos, Columna.ColumnName);
                Agregar(Datos, Texto(fila[Columna]));
            }
            return Hmac(Datos.ToString());
        }
        private static Dictionary<string, string> CalcularColumnas(string tabla, DataTable filas)
        {
            Dictionary<string, string> Digitos = new Dictionary<string, string>();
            foreach (DataColumn Columna in filas.Columns)
            {
                if (Columna.ColumnName == ColumnaDVH) continue;
                StringBuilder Datos = new StringBuilder(tabla).Append('.').Append(Columna.ColumnName).Append('|');
                foreach (DataRow Fila in filas.Rows) Agregar(Datos, Texto(Fila[Columna]));
                Digitos[Columna.ColumnName] = Hmac(Datos.ToString());
            }
            return Digitos;
        }
        private static string CalcularTabla(string tabla, List<string> dvhs)
        {
            return Hmac(tabla + "|" + dvhs.Count + "|" + string.Join("", dvhs));
        }
        private static void Agregar(StringBuilder datos, string valor)
        {
            datos.Append(valor.Length).Append(':').Append(valor).Append(';');
        }
        /// Representación fija de cada tipo, sin depender de la cultura.
        private static string Texto(object valor)
        {
            if (valor == null || valor == DBNull.Value) return "∅";
            if (valor is DateTime) return ((DateTime)valor).ToString("yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);
            if (valor is bool) return (bool)valor ? "1" : "0";
            byte[] Bytes = valor as byte[];
            if (Bytes != null) return BitConverter.ToString(Bytes);
            return Convert.ToString(valor, CultureInfo.InvariantCulture);
        }
        private static string Hmac(string datos)
        {
            string Clave = ConfigurationManager.AppSettings["DigitoVerificadorClave"];
            if (string.IsNullOrWhiteSpace(Clave)) throw new ConfigurationErrorsException("Falta configurar DigitoVerificadorClave.");
            using (HMACSHA256 Algoritmo = new HMACSHA256(Encoding.UTF8.GetBytes(Clave)))
            {
                byte[] Resultado = Algoritmo.ComputeHash(Encoding.UTF8.GetBytes(datos));
                return BitConverter.ToString(Resultado).Replace("-", "").ToLowerInvariant();
            }
        }

        // ---------- Acceso a datos ----------

        private static DataTable LeerFilas(string tabla, out string[] clave)
        {
            DataSet Ds = new DAO().ExecuteDataSetStoredProcedure("SP_DV_ObtenerFilas",
                new Dictionary<string, object> { { "@Tabla", tabla } }, CommandType.StoredProcedure);
            clave = Ds.Tables[0].Rows.Cast<DataRow>().Select(F => F["Columna"].ToString()).ToArray();
            return Ds.Tables[1];
        }
        private static Dictionary<string, string> LeerDigitos(string tabla)
        {
            DataSet Ds = new DAO().ExecuteDataSetStoredProcedure("SP_DV_ObtenerDigitos",
                new Dictionary<string, object> { { "@Tabla", tabla } }, CommandType.StoredProcedure);
            return Ds.Tables[0].Rows.Cast<DataRow>().ToDictionary(F => F["Columna"].ToString(), F => F["DV"].ToString());
        }
        private static string DescribirClave(DataRow fila, string[] clave)
        {
            return string.Join(", ", clave.Select(C => C + " = " + Texto(fila[C])));
        }
        private static string Json(Dictionary<string, string> valores)
        {
            return "{" + string.Join(",", valores.Select(V => "\"" + Escapar(V.Key) + "\":\"" + Escapar(V.Value) + "\"")) + "}";
        }
        private static string Escapar(string texto)
        {
            StringBuilder Resultado = new StringBuilder();
            foreach (char C in texto)
            {
                if (C == '"' || C == '\\') Resultado.Append('\\').Append(C);
                else if (C < ' ') Resultado.Append("\\u").Append(((int)C).ToString("x4"));
                else Resultado.Append(C);
            }
            return Resultado.ToString();
        }
    }
}
