using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using SERVICE;

namespace BLL
{
    /// Bitácora del sistema. La tabla guarda los eventos de los últimos
    /// DiasRetencion días; los más viejos se pasan al archivo histórico XML.
    /// Un registro nunca se borra a pedido de un usuario: se elimina en
    /// forma lógica y se puede restaurar.
    public class BitacoraBLL
    {
        public const int DiasRetencion = 30;
        public const string PermisoEliminar = "EliminarRegistroBitacora";

        public void Registrar(string accion, string detalle, int? usuarioId)
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            BitacoraDal.Registrar(accion, detalle, usuarioId);
            // Mantenimiento de los últimos 30 días: lo vencido se archiva en
            // el XML y recién después sale de la tabla.
            List<BitacoraBE> Vencidos = BitacoraDal.ObtenerVencidos(DiasRetencion);
            if (Vencidos.Count > 0)
            {
                BitacoraArchivoService ArchivoServ = new BitacoraArchivoService();
                ArchivoServ.Archivar(Vencidos);
                foreach (BitacoraBE Evento in Vencidos)
                {
                    BitacoraDal.QuitarArchivado(Evento.Id, DiasRetencion);
                }
            }
        }
        /// Los eventos de los últimos 30 días (los de la tabla).
        public List<BitacoraBE> ObtenerRecientes()
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            return BitacoraDal.ListarRecientes();
        }
        /// Los de la tabla + todo lo archivado en el XML, combinado.
        public List<BitacoraBE> ObtenerHistorialCompleto()
        {
            DAL.BitacoraDAL BitacoraDal = new DAL.BitacoraDAL();
            List<BitacoraBE> Actuales = BitacoraDal.ListarRecientes();
            BitacoraArchivoService ArchivoServ = new BitacoraArchivoService();
            List<BitacoraBE> Archivados = ArchivoServ.ObtenerTodos();
            foreach (BitacoraBE Evento in Archivados) Evento.Archivado = true;
            List<BitacoraBE> Todo = new List<BitacoraBE>();
            Todo.AddRange(Actuales);
            Todo.AddRange(Archivados);
            Todo.Sort((a, b) => b.Fecha.CompareTo(a.Fecha));
            return Todo;
        }
        /// Eliminación lógica de un evento de la tabla. Queda registrada en
        /// la propia Bitácora.
        public bool EliminarLogico(int id, UsuarioBE usuario)
        {
            BitacoraBE Evento = BuscarReciente(id);
            if (Evento == null || Evento.Eliminado) return false;
            new DAL.BitacoraDAL().EliminarLogico(id, usuario.Id);
            Registrar("Eliminar Registro Bitácora", Describir(Evento), usuario.Id);
            return true;
        }
        public bool Restaurar(int id, UsuarioBE usuario)
        {
            BitacoraBE Evento = BuscarReciente(id);
            if (Evento == null || !Evento.Eliminado) return false;
            new DAL.BitacoraDAL().Restaurar(id);
            Registrar("Restaurar Registro Bitácora", Describir(Evento), usuario.Id);
            return true;
        }
        private static BitacoraBE BuscarReciente(int id)
        {
            return new DAL.BitacoraDAL().ListarRecientes().FirstOrDefault(B => B.Id == id);
        }
        private static string Describir(BitacoraBE evento)
        {
            return "Registro #" + evento.Id + ": " + evento.Accion + " del " + evento.Fecha.ToString("dd/MM/yyyy HH:mm");
        }
        public List<BitacoraBE> Filtrar(List<BitacoraBE> origen, int? usuarioId, string accion, DateTime? desde, DateTime? hasta, bool incluirEliminados)
        {
            IEnumerable<BitacoraBE> Resultado = origen;
            if (!incluirEliminados) Resultado = Resultado.Where(x => !x.Eliminado);
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