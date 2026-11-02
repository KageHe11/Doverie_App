using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BE;

namespace DAL
{
    public class BackupDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_BackUp_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        /// CU-002-013 paso 4: genera el .bak de la base. Devuelve la ruta del
        /// archivo (en el servidor de SQL Server) y su tamaño en bytes. La
        /// carpeta se puede fijar con BackupBaseDatosRuta en AppSettings; si
        /// no, va a la carpeta de backups de la instancia.
        public string GenerarBaseDatos(string Parchivo, out long Ptamano)
        {
            DAO Dao = new DAO();
            string Carpeta = ConfigurationManager.AppSettings["BackupBaseDatosRuta"];
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Archivo", Parchivo },
                { "@Carpeta", string.IsNullOrWhiteSpace(Carpeta) ? (object)DBNull.Value : Carpeta }
            };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_BackUp_GenerarBaseDatos", Parametros, CommandType.StoredProcedure);
            DataRow Fila = Ds.Tables[0].Rows[0];
            Ptamano = Convert.ToInt64(Fila["Tamano"]);
            return Fila["Ruta"].ToString();
        }
        public void Registrar(BackupBE Pbackup)
        {
            DAO Dao = new DAO();
            Pbackup.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pbackup.Id },
                { "@UsuarioId", Pbackup.UsuarioId },
                { "@Fecha", Pbackup.Fecha },
                { "@Estado", Pbackup.Estado },
                { "@Detalle", (object)Pbackup.Detalle ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_BackUp_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public List<BackupBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_BackUp_Listar", null, CommandType.StoredProcedure);
            List<BackupBE> Lista = new List<BackupBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Lista.Add(new BackupBE
                {
                    Id = Convert.ToInt32(Fila["Id"]),
                    UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                    NombreUsuario = Fila["NombreUsuario"].ToString(),
                    Fecha = Convert.ToDateTime(Fila["Fecha"]),
                    Estado = Fila["Estado"].ToString(),
                    Detalle = Fila["Detalle"] == DBNull.Value ? null : Fila["Detalle"].ToString()
                });
            }
            return Lista;
        }
    }
}
