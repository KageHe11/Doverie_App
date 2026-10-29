using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    /// Consultas de estadísticas (CU-002-012 y webservices del anexo).
    public class EstadisticaDAL
    {
        public EstadisticasBE ObtenerResumen()
        {
            DAO Dao = new DAO();
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Estadistica_ObtenerResumen", null, CommandType.StoredProcedure);
            DataRow Fila = Ds.Tables[0].Rows[0];
            return new EstadisticasBE
            {
                UsuariosActivos = Convert.ToInt32(Fila["UsuariosActivos"]),
                DocumentosAlmacenados = Convert.ToInt32(Fila["DocumentosAlmacenados"]),
                OperacionesBlockchain = Convert.ToInt32(Fila["OperacionesBlockchain"])
            };
        }
        public int ObtenerDocumentosPorPeriodo(DateTime Pdesde, DateTime Phasta)
        {
            return ContarPorPeriodo("SP_Estadistica_ObtenerDocumentosPorPeriodo", Pdesde, Phasta, null);
        }
        public int ObtenerVerificacionesPorPeriodo(DateTime Pdesde, DateTime Phasta)
        {
            return ContarPorPeriodo("SP_Estadistica_ObtenerVerificacionesPorPeriodo", Pdesde, Phasta, null);
        }
        public int ObtenerUsuariosPorPeriodo(DateTime Pdesde, DateTime Phasta)
        {
            return ContarPorPeriodo("SP_Estadistica_ObtenerUsuariosPorPeriodo", Pdesde, Phasta, null);
        }
        public int ObtenerOperacionesPorPeriodo(DateTime Pdesde, DateTime Phasta)
        {
            return ContarPorPeriodo("SP_Estadistica_ObtenerOperacionesPorPeriodo", Pdesde, Phasta, null);
        }
        public int ObtenerTransaccionesPorUsuario(int PusuarioId, DateTime Pdesde, DateTime Phasta)
        {
            return ContarPorPeriodo("SP_Estadistica_ObtenerTransaccionesPorUsuario", Pdesde, Phasta, PusuarioId);
        }
        private int ContarPorPeriodo(string Psp, DateTime Pdesde, DateTime Phasta, int? PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Desde", Pdesde },
                { "@Hasta", Phasta }
            };
            if (PusuarioId.HasValue) Parametros.Add("@UsuarioId", PusuarioId.Value);
            return Convert.ToInt32(Dao.ExecuteScalarStoredProcedure(Psp, Parametros, CommandType.StoredProcedure));
        }
    }
}
