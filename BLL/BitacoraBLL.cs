using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using SERVICE;

namespace BLL
{
    public class BitacoraBLL
    {
        public void Registrar(string accion, string detalle, int? usuarioId)
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            BitacoraDal.Registrar(accion, detalle, usuarioId);
            // Mantiene el máximo de 30 filas en la tabla: lo que sobra se
            // archiva en el XML y se borra de la base.
            List<BitacoraBE> Excedente = BitacoraDal.ObtenerExcedente();
            if (Excedente.Count > 0)
            {
                BitacoraArchivoService ArchivoServ = new BitacoraArchivoService();
                ArchivoServ.Archivar(Excedente);
                foreach (BitacoraBE Evento in Excedente)
                {
                    BitacoraDal.EliminarPorId(Evento.Id);
                }
            }
        }
        public List<BitacoraBE> ObtenerUltimos30()
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            return BitacoraDal.ListarUltimos30();
        }
        /// Los 30 actuales de la BD + todo lo archivado en el XML, combinado.
        public List<BitacoraBE> ObtenerHistorialCompleto()
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            List<BitacoraBE> Actuales = BitacoraDal.ListarUltimos30();
            BitacoraArchivoService ArchivoServ = new BitacoraArchivoService();
            List<BitacoraBE> Archivados = ArchivoServ.ObtenerTodos();
            List<BitacoraBE> Todo = new List<BitacoraBE>();
            Todo.AddRange(Actuales);
            Todo.AddRange(Archivados);
            Todo.Sort((a, b) => b.Fecha.CompareTo(a.Fecha));
            return Todo;
        }
        public List<BitacoraBE> Filtrar(List<BitacoraBE> origen, int? usuarioId, string accion, DateTime? desde, DateTime? hasta)
        {
            IEnumerable<BitacoraBE> Resultado = origen;
            if (usuarioId.HasValue) Resultado = Resultado.Where(x => x.UsuarioId == usuarioId.Value);
            if (!string.IsNullOrEmpty(accion)) Resultado = Resultado.Where(x => x.Accion == accion);
            if (desde.HasValue) Resultado = Resultado.Where(x => x.Fecha >= desde.Value);
            if (hasta.HasValue) Resultado = Resultado.Where(x => x.Fecha <= hasta.Value);
            return Resultado.ToList();
        }
        public List<string> ListarAcciones()
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            return BitacoraDal.ListarAcciones();
        }
    }
}