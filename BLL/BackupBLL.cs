using System;
using System.Collections.Generic;
using System.IO;
using BE;
using DAL;
using SERVICE;

namespace BLL
{
    /// CU-002-013 Generar Backup Manual: copia de la base de datos y de los
    /// archivos de los documentos (AWS S3 o disco, según el almacenamiento
    /// configurado). Cada intento queda en la tabla BackUp y en la Bitácora,
    /// salga bien o mal.
    public class BackupBLL
    {
        /// Un backup a la vez: dos administradores no pueden pisarse.
        private static readonly object Bloqueo = new object();

        public ResultadoBackup Generar(UsuarioBE administrador, out BackupBE backup)
        {
            lock (Bloqueo)
            {
                DateTime Fecha = DateTime.Now;
                string Etiqueta = Fecha.ToString("yyyyMMdd-HHmmss");
                backup = new BackupBE { UsuarioId = administrador.Id, NombreUsuario = administrador.NombreUsuario, Fecha = Fecha };
                // 4. Copia de la base de datos [ERROR-041]
                string RutaBak;
                long TamanoBak;
                try
                {
                    RutaBak = new BackupDAL().GenerarBaseDatos("DB_Doverie_" + Etiqueta + ".bak", out TamanoBak);
                }
                catch (Exception)
                {
                    return Finalizar(backup, ResultadoBackup.ErrorBaseDatos, "Falló la copia de la base de datos.");
                }
                // El .bak lo escribe SQL Server; si corre en otro equipo, la app
                // no lo ve y la copia queda solo en ese servidor.
                bool BakAccesible = File.Exists(RutaBak);
                string DetalleBase = "Base: " + Path.GetFileName(RutaBak) + " (" + FormatearTamano(TamanoBak) + ")" +
                    (BakAccesible ? "" : ", solo en el servidor de SQL Server");
                // 5. Copia de los archivos (AWS S3) [ERROR-042]
                RespaldoArchivosBE Archivos;
                try
                {
                    Archivos = ProveedorServicios.Almacenamiento().Respaldar(Etiqueta, BakAccesible ? RutaBak : null);
                }
                catch (Exception)
                {
                    return Finalizar(backup, ResultadoBackup.ErrorArchivos, DetalleBase + ". Falló la copia de los archivos.");
                }
                return Finalizar(backup, ResultadoBackup.Exitoso, DetalleBase + ". Archivos: " + Archivos.CantidadArchivos +
                    " (" + FormatearTamano(Archivos.TamanoBytes) + ") en " + Archivos.Ubicacion);
            }
        }
        public List<BackupBE> Listar()
        {
            return new BackupDAL().Listar();
        }
        private static ResultadoBackup Finalizar(BackupBE backup, ResultadoBackup resultado, string detalle)
        {
            backup.Estado = resultado == ResultadoBackup.Exitoso ? BackupBE.EstadoExitoso : BackupBE.EstadoFallido;
            backup.Detalle = detalle.Length > 500 ? detalle.Substring(0, 500) : detalle;
            new BackupDAL().Registrar(backup);
            new BitacoraBLL().Registrar("Generar Backup Manual", backup.Estado + ". " + backup.Detalle, backup.UsuarioId);
            return resultado;
        }
        private static string FormatearTamano(long bytes)
        {
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("0.0") + " MB";
        }
    }
}
