using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SERVICE
{
    /// Genera PDFs simples de una página (A4) sin librerías externas: un
    /// título, filas "etiqueta: valor" y un pie. Usa las fuentes estándar
    /// Helvetica con codificación WinAnsi, que cubre los acentos del
    /// español y el portugués.
    public class GeneradorPdf
    {
        private static readonly Encoding WinAnsi = Encoding.GetEncoding(1252);

        public byte[] Generar(string titulo, IList<KeyValuePair<string, string>> filas, string pie)
        {
            // Contenido de la página. El origen (0,0) es la esquina inferior izquierda.
            StringBuilder Contenido = new StringBuilder();
            Texto(Contenido, "F2", 18, 50, 780, titulo);
            Contenido.Append("0.6 w 50 765 m 545 765 l S\n");
            int Y = 735;
            foreach (KeyValuePair<string, string> Fila in filas)
            {
                Texto(Contenido, "F2", 11, 50, Y, Fila.Key);
                Texto(Contenido, "F1", 11, 210, Y, Fila.Value);
                Y -= 22;
            }
            if (!string.IsNullOrEmpty(pie)) Texto(Contenido, "F1", 9, 50, 60, pie);
            byte[] Flujo = WinAnsi.GetBytes(Contenido.ToString());

            string[] Objetos =
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"
            };
            using (MemoryStream Pdf = new MemoryStream())
            {
                List<long> Posiciones = new List<long>();
                Escribir(Pdf, "%PDF-1.4\n");
                for (int I = 0; I < Objetos.Length; I++)
                {
                    Posiciones.Add(Pdf.Position);
                    Escribir(Pdf, (I + 1) + " 0 obj\n" + Objetos[I] + "\nendobj\n");
                }
                Posiciones.Add(Pdf.Position);
                Escribir(Pdf, "6 0 obj\n<< /Length " + Flujo.Length + " >>\nstream\n");
                Pdf.Write(Flujo, 0, Flujo.Length);
                Escribir(Pdf, "\nendstream\nendobj\n");
                // Tabla de referencias: la posición en bytes de cada objeto.
                long InicioXref = Pdf.Position;
                StringBuilder Xref = new StringBuilder("xref\n0 7\n0000000000 65535 f \n");
                foreach (long Posicion in Posiciones)
                {
                    Xref.Append(Posicion.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
                }
                Xref.Append("trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n").Append(InicioXref).Append("\n%%EOF\n");
                Escribir(Pdf, Xref.ToString());
                return Pdf.ToArray();
            }
        }
        private static void Texto(StringBuilder contenido, string fuente, int tamano, int x, int y, string texto)
        {
            contenido.Append("BT /").Append(fuente).Append(' ').Append(tamano).Append(" Tf ")
                     .Append(x).Append(' ').Append(y).Append(" Td (").Append(Escapar(texto)).Append(") Tj ET\n");
        }
        /// Los paréntesis y la barra invertida tienen significado en los
        /// textos de PDF; los saltos de línea se reemplazan por espacios.
        private static string Escapar(string texto)
        {
            return (texto ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", " ").Replace("\n", " ");
        }
        private static void Escribir(Stream destino, string texto)
        {
            byte[] Bytes = WinAnsi.GetBytes(texto);
            destino.Write(Bytes, 0, Bytes.Length);
        }
    }
}
