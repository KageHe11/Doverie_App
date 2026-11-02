using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using BE;

namespace DAL
{
    public class BackupDAL
    {
        /// Backup y restore pueden tardar minutos con una base grande.
        private const int TiempoEsperaRespaldo = 600;

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
            DAO Dao = new DAO { TiempoEspera = TiempoEsperaRespaldo };
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
        /// Carpeta donde quedan los .bak: la configurada o la de la instancia.
        public string ObtenerCarpeta()
        {
            string Carpeta = ConfigurationManager.AppSettings["BackupBaseDatosRuta"];
            if (!string.IsNullOrWhiteSpace(Carpeta)) return Carpeta;
            DAO Dao = new DAO();
            return Dao.ExecuteScalarStoredProcedure("SP_BackUp_ObtenerCarpeta", null, CommandType.StoredProcedure).ToString();
        }
        /// Nombre de la base de la aplicación: un .bak de otra base no se restaura.
        public string NombreBase()
        {
            return new DAO().NombreBase;
        }
        /// Lee el encabezado del .bak: de qué base es y con qué nombre se generó.
        public void LeerEncabezado(string Pruta, out string PbaseDatos, out string Pnombre)
        {
            DAO Dao = DAO.Servidor();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Ruta", Pruta } };
            DataRow Fila = Dao.ExecuteDataSetStoredProcedure("dbo.SP_Doverie_LeerBackup", Parametros, CommandType.StoredProcedure).Tables[0].Rows[0];
            PbaseDatos = Fila["DatabaseName"].ToString();
            Pnombre = Fila["BackupName"] == DBNull.Value ? "" : Fila["BackupName"].ToString();
        }
        /// Lanza una excepción si el .bak está incompleto o dañado.
        public void Verificar(string Pruta)
        {
            DAO Dao = DAO.Servidor();
            Dao.TiempoEspera = TiempoEsperaRespaldo;
            Dao.ExecuteNonQueryStoredProcedure("dbo.SP_Doverie_VerificarBackup", new Dictionary<string, object> { { "@Ruta", Pruta } }, CommandType.StoredProcedure);
        }
        /// Reemplaza la base de la aplicación con el .bak. Las conexiones que
        /// estaban abiertas se cortan, así que después se descarta el pool.
        public void RestaurarBaseDatos(string Pruta)
        {
            DAO Dao = DAO.Servidor();
            Dao.TiempoEspera = TiempoEsperaRespaldo;
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Base", NombreBase() },
                { "@Ruta", Pruta }
            };
            try
            {
                Dao.ExecuteNonQueryStoredProcedure("dbo.SP_Doverie_RestaurarBaseDatos", Parametros, CommandType.StoredProcedure);
            }
            finally
            {
                DAO.ReiniciarConexiones();
            }
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
        public BackupBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_BackUp_ObtenerDatos", new Dictionary<string, object> { { "@Id", Pid } }, CommandType.StoredProcedure);
            return Ds.Tables[0].Rows.Count == 0 ? null : MapearFila(Ds.Tables[0].Rows[0]);
        }
        public List<BackupBE> Listar()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_BackUp_Listar", null, CommandType.StoredProcedure);
            List<BackupBE> Lista = new List<BackupBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Lista.Add(MapearFila(Fila));
            }
            return Lista;
        }
        private static BackupBE MapearFila(DataRow Fila)
        {
            return new BackupBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                NombreUsuario = Fila["NombreUsuario"].ToString(),
                Fecha = Convert.ToDateTime(Fila["Fecha"]),
                Estado = Fila["Estado"].ToString(),
                Detalle = Fila["Detalle"] == DBNull.Value ? null : Fila["Detalle"].ToString()
            };
        }
    }
}
