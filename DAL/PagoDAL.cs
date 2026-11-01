using BE;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class PagoDAL
    {
        static int UltimoId;
        private static int ProximoId()
        {
            DAO Dao = new DAO();
            if (UltimoId == 0) UltimoId = int.Parse(Dao.ExecuteScalarStoredProcedure("SP_Pago_ObtenerUltimoId", null, CommandType.StoredProcedure).ToString());
            UltimoId += 1;
            return UltimoId;
        }
        public void Registrar(PagoBE Ppago)
        {
            DAO Dao = new DAO();
            Ppago.Id = ProximoId();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Ppago.Id },
                { "@SuscripcionId", Ppago.SuscripcionId },
                { "@MedioPagoId", Ppago.MedioPagoId },
                { "@ComprobanteUrl", (object)Ppago.ComprobanteUrl ?? DBNull.Value },
                { "@Estado", Ppago.Estado },
                { "@Fecha", Ppago.Fecha },
                { "@Monto", Ppago.Monto },
                { "@Concepto", Ppago.Concepto }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Pago_Registrar", Parametros, CommandType.StoredProcedure);
        }
        public PagoBE ObtenerDatos(int Pid)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@Id", Pid } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Pago_ObtenerDatos", Parametros, CommandType.StoredProcedure);
            if (Ds.Tables[0].Rows.Count == 0) return null;
            return MapearFila(Ds.Tables[0].Rows[0]);
        }
        public List<PagoBE> ListarPorUsuario(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            DataSet Ds = Dao.ExecuteDataSetStoredProcedure("SP_Pago_ListarPorUsuario", Parametros, CommandType.StoredProcedure);
            List<PagoBE> Pagos = new List<PagoBE>();
            foreach (DataRow Fila in Ds.Tables[0].Rows)
            {
                Pagos.Add(MapearFila(Fila));
            }
            return Pagos;
        }
        /// Medio del último pago aprobado de la cuenta, o null si no hay.
        public int? ObtenerUltimoMedioAprobado(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            object Valor = Dao.ExecuteScalarStoredProcedure("SP_Pago_ObtenerUltimoMedioAprobado", Parametros, CommandType.StoredProcedure);
            return Valor == null || Valor == DBNull.Value ? (int?)null : Convert.ToInt32(Valor);
        }
        public void AnularPendientes(int PusuarioId)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object> { { "@UsuarioId", PusuarioId } };
            Dao.ExecuteNonQueryStoredProcedure("SP_Pago_AnularPendientes", Parametros, CommandType.StoredProcedure);
        }
        /// Aprueba el pago, activa su suscripción y reemplaza la anterior
        /// (una sola transacción en el SP).
        public void Aprobar(int Pid, string PcomprobanteUrl, DateTime PfechaInicio, DateTime PfechaRenovacion)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@ComprobanteUrl", PcomprobanteUrl },
                { "@FechaInicio", PfechaInicio },
                { "@FechaRenovacion", PfechaRenovacion }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Pago_Aprobar", Parametros, CommandType.StoredProcedure);
        }
        public void Rechazar(int Pid, string PcomprobanteUrl)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@ComprobanteUrl", (object)PcomprobanteUrl ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Pago_Rechazar", Parametros, CommandType.StoredProcedure);
        }
        public void ActualizarEstado(int Pid, string Pestado, string PcomprobanteUrl)
        {
            DAO Dao = new DAO();
            Dictionary<string, object> Parametros = new Dictionary<string, object>
            {
                { "@Id", Pid },
                { "@Estado", Pestado },
                { "@ComprobanteUrl", (object)PcomprobanteUrl ?? DBNull.Value }
            };
            Dao.ExecuteNonQueryStoredProcedure("SP_Pago_ActualizarEstado", Parametros, CommandType.StoredProcedure);
        }
        private PagoBE MapearFila(DataRow Fila)
        {
            return new PagoBE
            {
                Id = Convert.ToInt32(Fila["Id"]),
                SuscripcionId = Convert.ToInt32(Fila["SuscripcionId"]),
                MedioPagoId = Convert.ToInt32(Fila["MedioPagoId"]),
                ComprobanteUrl = Fila["ComprobanteUrl"] == DBNull.Value ? null : Fila["ComprobanteUrl"].ToString(),
                Estado = Fila["Estado"].ToString(),
                Fecha = Convert.ToDateTime(Fila["Fecha"]),
                Monto = Convert.ToDecimal(Fila["Monto"]),
                Concepto = Fila["Concepto"].ToString(),
                MedioPago = Fila["MedioPago"].ToString(),
                UsuarioId = Convert.ToInt32(Fila["UsuarioId"]),
                PlanId = Convert.ToInt32(Fila["PlanId"]),
                PlanNombre = Fila["PlanNombre"].ToString(),
                PlanTipo = Fila["PlanTipo"].ToString()
            };
        }
    }
}
