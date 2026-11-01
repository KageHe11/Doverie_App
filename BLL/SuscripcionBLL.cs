using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace BLL
{
    /// CU-001-017 Suscribirse, CU-001-008 Gestionar Suscripción y
    /// CU-001-014 Ver Detalle de Facturación. La suscripción es de la cuenta:
    /// la contrata y la gestiona el titular (la Empresa o el Usuario
    /// independiente) y sus empleados usan el plan de la Empresa.
    /// El cobro pasa por IPasarelaPago (ProveedorServicios), así que esta
    /// clase no cambia al pasar del checkout simulado a Mercado Pago.
    public class SuscripcionBLL
    {
        private static readonly object BloqueoRenovacion = new object();
        private static readonly CultureInfo Moneda = CultureInfo.GetCultureInfo("es-AR");

        public List<PlanBE> ListarPlanes()
        {
            return new PlanDAL().ListarActivos();
        }
        public List<MedioPagoBE> ListarMediosPago()
        {
            return new MedioPagoDAL().ListarActivos();
        }
        /// Descuento de la modalidad respecto del precio Mensual del mismo
        /// nivel, en porcentaje (ej: Anual = 20).
        public static int Descuento(PlanBE plan, IEnumerable<PlanBE> planes)
        {
            PlanBE Mensual = planes.FirstOrDefault(P => P.Nombre == plan.Nombre && P.Meses == 1);
            if (Mensual == null || Mensual.PrecioMensual == 0) return 0;
            return (int)Math.Round((1 - plan.PrecioMensual / Mensual.PrecioMensual) * 100);
        }
        /// Solo el titular de la cuenta contrata y gestiona la suscripción.
        public static bool EsTitular(UsuarioBE usuario)
        {
            return usuario.EmpresaId == null && (usuario.Empresa || usuario.Independiente);
        }
        /// Suscripción vigente de la cuenta del Usuario (con su Plan), o null.
        /// Antes de devolverla procesa lo que corresponda por fecha: si el
        /// período abonado terminó, la renueva (débito automático) o la da
        /// por vencida si estaba cancelada.
        public SuscripcionBE ObtenerVigente(UsuarioBE usuario)
        {
            int CuentaId = DocumentoBLL.CuentaDe(usuario);
            SuscripcionBE Suscripcion;
            lock (BloqueoRenovacion)
            {
                Suscripcion = new SuscripcionDAL().ObtenerEnCurso(CuentaId);
                if (Suscripcion == null) return null;
                ProcesarFinDePeriodo(Suscripcion);
            }
            if (!Suscripcion.Vigente) return null;
            Suscripcion.Plan = new PlanDAL().ObtenerDatos(Suscripcion.PlanId);
            return Suscripcion;
        }
        /// Medio de pago registrado de la cuenta (el del último pago
        /// aprobado), o null.
        public MedioPagoBE ObtenerMedioRegistrado(UsuarioBE usuario)
        {
            int? MedioId = new PagoDAL().ObtenerUltimoMedioAprobado(DocumentoBLL.CuentaDe(usuario));
            if (!MedioId.HasValue) return null;
            return ListarMediosPago().FirstOrDefault(M => M.Id == MedioId.Value);
        }
        /// Cupo de usuarios del plan: false si la cuenta no tiene suscripción
        /// vigente o si ya tiene todos los usuarios que permite su plan
        /// (contando al titular).
        public bool PuedeAgregarUsuario(UsuarioBE usuario)
        {
            SuscripcionBE Suscripcion = ObtenerVigente(usuario);
            if (Suscripcion == null) return false;
            if (!Suscripcion.Plan.MaxUsuarios.HasValue) return true;
            return ContarUsuarios(DocumentoBLL.CuentaDe(usuario)) < Suscripcion.Plan.MaxUsuarios.Value;
        }

        /// CU-001-017 pasos 3 a 6 y CU-001-008 pasos 3 a 5: registra la
        /// suscripción y el pago pendientes y devuelve la URL del checkout de
        /// la pasarela. Si la cuenta ya tiene una suscripción vigente, es un
        /// cambio de plan.
        public ResultadoSuscripcion IniciarPago(UsuarioBE usuario, int planId, int medioPagoId, string urlRetorno, out string urlCheckout)
        {
            urlCheckout = null;
            if (!EsTitular(usuario)) return ResultadoSuscripcion.SinAcceso;
            PlanBE Plan = new PlanDAL().ObtenerDatos(planId);
            if (Plan == null || !Plan.Activo || !ListarMediosPago().Any(M => M.Id == medioPagoId))
            {
                return ResultadoSuscripcion.DatosInvalidos;
            }
            int CuentaId = DocumentoBLL.CuentaDe(usuario);
            SuscripcionBE Actual = ObtenerVigente(usuario);
            if (Actual != null && Actual.PlanId == planId) return ResultadoSuscripcion.MismoPlan;
            // Bajar a un plan con menos usuarios de los que ya tiene la cuenta.
            if (Plan.MaxUsuarios.HasValue && ContarUsuarios(CuentaId) > Plan.MaxUsuarios.Value)
            {
                return ResultadoSuscripcion.LimiteUsuarios;
            }
            PagoDAL PagoDal = new PagoDAL();
            PagoBE Pago;
            try
            {
                // Un pago anterior que quedó abandonado en el checkout ya no
                // se puede confirmar.
                PagoDal.AnularPendientes(CuentaId);
                DateTime Ahora = DateTime.Now;
                SuscripcionBE Nueva = new SuscripcionBE
                {
                    UsuarioId = CuentaId,
                    PlanId = Plan.Id,
                    Estado = SuscripcionBE.EstadoPendiente,
                    FechaInicio = Ahora,
                    FechaRenovacion = Ahora.AddMonths(Plan.Meses)
                };
                new SuscripcionDAL().Registrar(Nueva);
                Pago = new PagoBE
                {
                    SuscripcionId = Nueva.Id,
                    MedioPagoId = medioPagoId,
                    Estado = PagoBE.EstadoPendiente,
                    Fecha = Ahora,
                    Monto = Plan.Total,
                    Concepto = Actual == null ? PagoBE.ConceptoAlta : PagoBE.ConceptoCambioPlan
                };
                PagoDal.Registrar(Pago);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorPago;
            }
            // 6. Redirigir al proceso de pago [ERROR-014]
            try
            {
                urlCheckout = ProveedorServicios.PasarelaPago().IniciarPago(Pago.Id, Pago.Monto, "Doverie - " + DescribirPlan(Plan), urlRetorno + "?pago=" + Pago.Id);
            }
            catch (Exception)
            {
                PagoDal.Rechazar(Pago.Id, null);
                return ResultadoSuscripcion.ErrorPago;
            }
            new BitacoraBLL().Registrar("Iniciar Pago Suscripción", "Pago Id: " + Pago.Id + " | Plan: " + DescribirPlan(Plan) + " | Monto: " + FormatearMonto(Pago.Monto), usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }
        /// CU-001-017 pasos 7 a 10 y CU-001-008 pasos 7 a 9: al volver del
        /// checkout, consulta el resultado a la pasarela (nunca a la URL) y
        /// activa la suscripción. Volver a llamarlo con el mismo pago no
        /// repite nada. emailEnviado indica si salió el correo de
        /// confirmación en esta llamada.
        public ResultadoSuscripcion ConfirmarPago(UsuarioBE usuario, int pagoId, out PagoBE pago, out bool emailEnviado)
        {
            emailEnviado = false;
            PagoDAL PagoDal = new PagoDAL();
            pago = PagoDal.ObtenerDatos(pagoId);
            if (pago == null || pago.UsuarioId != DocumentoBLL.CuentaDe(usuario) || !EsTitular(usuario))
            {
                pago = null;
                return ResultadoSuscripcion.SinAcceso;
            }
            if (pago.Estado == PagoBE.EstadoAprobado) return ResultadoSuscripcion.Exitoso;
            if (pago.Estado != PagoBE.EstadoPendiente) return ResultadoSuscripcion.ErrorPago;
            OperacionPagoBE Operacion;
            try
            {
                Operacion = ProveedorServicios.PasarelaPago().ConsultarPago(pago.Id);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.PagoPendiente;
            }
            if (Operacion.Estado == OperacionPagoBE.EstadoPendiente) return ResultadoSuscripcion.PagoPendiente;
            PlanBE Plan = new PlanDAL().ObtenerDatos(pago.PlanId);
            if (Operacion.Estado != OperacionPagoBE.EstadoAprobado)
            {
                // 7.1 El sistema no puede confirmar el pago [ERROR-014]
                PagoDal.Rechazar(pago.Id, Operacion.Operacion);
                new BitacoraBLL().Registrar("Pago Rechazado", "Pago Id: " + pago.Id + " | Plan: " + DescribirPlan(Plan), usuario.Id);
                GestorEventos.Notificar(CatalogoEventos.PagoRechazado, pago.UsuarioId, Plan.Nombre);
                pago.Estado = PagoBE.EstadoRechazado;
                return ResultadoSuscripcion.ErrorPago;
            }
            // 8. Activar la suscripción
            DateTime Inicio = DateTime.Now;
            DateTime Renovacion = Inicio.AddMonths(Plan.Meses);
            PagoDal.Aprobar(pago.Id, Operacion.Operacion, Inicio, Renovacion);
            pago = PagoDal.ObtenerDatos(pago.Id);
            bool EsCambio = pago.Concepto == PagoBE.ConceptoCambioPlan;
            new BitacoraBLL().Registrar(EsCambio ? "Modificar Suscripción" : "Suscribirse",
                "Plan: " + DescribirPlan(Plan) + " | Pago Id: " + pago.Id + " | Operación: " + Operacion.Operacion, usuario.Id);
            // 9. Correo de confirmación. Si falla, la suscripción igual quedó
            // activa: se registra en la Bitácora y se sigue.
            try
            {
                emailEnviado = EnviarConfirmacion(usuario, Plan, pago, Renovacion, EsCambio);
            }
            catch (Exception)
            {
                new BitacoraBLL().Registrar("Error Envío Email", "Confirmación de suscripción de " + usuario.NombreUsuario, usuario.Id);
            }
            return ResultadoSuscripcion.Exitoso;
        }
        /// Pago pendiente de la cuenta del Usuario, o null. Lo usa el
        /// checkout simulado para no mostrar ni resolver pagos ajenos.
        public PagoBE ObtenerPagoPendiente(UsuarioBE usuario, int pagoId)
        {
            PagoBE Pago = new PagoDAL().ObtenerDatos(pagoId);
            if (Pago == null || !EsTitular(usuario) || Pago.UsuarioId != DocumentoBLL.CuentaDe(usuario) || Pago.Estado != PagoBE.EstadoPendiente)
            {
                return null;
            }
            return Pago;
        }
        /// CU-001-008 4.1 a 7.1: cancela la renovación automática. El acceso
        /// sigue hasta el fin del período abonado (accesoHasta).
        public ResultadoSuscripcion Cancelar(UsuarioBE usuario, out DateTime? accesoHasta)
        {
            accesoHasta = null;
            if (!EsTitular(usuario)) return ResultadoSuscripcion.SinAcceso;
            SuscripcionBE Suscripcion = ObtenerVigente(usuario);
            if (Suscripcion == null) return ResultadoSuscripcion.SinSuscripcion;
            accesoHasta = Suscripcion.FechaRenovacion;
            if (Suscripcion.Estado == SuscripcionBE.EstadoCancelada) return ResultadoSuscripcion.Exitoso;
            // 5.1 / 5.2 [ERROR-015]
            try
            {
                new SuscripcionDAL().ActualizarEstado(Suscripcion.Id, SuscripcionBE.EstadoCancelada);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorCancelar;
            }
            new BitacoraBLL().Registrar("Cancelar Suscripción", "Plan: " + DescribirPlan(Suscripcion.Plan) + " | Acceso hasta: " + Suscripcion.FechaRenovacion.ToString("dd/MM/yyyy"), usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }

        /// CU-001-014 pasos 2 y 4: historial de pagos de la cuenta [ERROR-034].
        public ResultadoSuscripcion ListarPagos(UsuarioBE usuario, out List<PagoBE> pagos)
        {
            pagos = new List<PagoBE>();
            if (!EsTitular(usuario)) return ResultadoSuscripcion.SinAcceso;
            // Procesa una renovación pendiente antes de listar, para que el
            // historial la incluya.
            ObtenerVigente(usuario);
            pagos = new PagoDAL().ListarPorUsuario(DocumentoBLL.CuentaDe(usuario));
            new BitacoraBLL().Registrar("Ver Facturación", null, usuario.Id);
            return pagos.Count == 0 ? ResultadoSuscripcion.SinPagos : ResultadoSuscripcion.Exitoso;
        }
        /// CU-001-014 pasos 6 y 7: el comprobante de un pago aprobado de la
        /// cuenta [ERROR-035].
        public ResultadoSuscripcion ObtenerComprobante(UsuarioBE usuario, int pagoId, out PagoBE pago)
        {
            pago = null;
            try
            {
                PagoBE Encontrado = new PagoDAL().ObtenerDatos(pagoId);
                if (Encontrado == null || !EsTitular(usuario) || Encontrado.UsuarioId != DocumentoBLL.CuentaDe(usuario) ||
                    Encontrado.Estado != PagoBE.EstadoAprobado || string.IsNullOrEmpty(Encontrado.ComprobanteUrl))
                {
                    return ResultadoSuscripcion.ErrorComprobante;
                }
                pago = Encontrado;
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorComprobante;
            }
            new BitacoraBLL().Registrar("Ver Comprobante", "Pago Id: " + pago.Id, usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }
        /// CU-001-014 paso 9: el comprobante en PDF [ERROR-035 / ERROR-036],
        /// con los textos en el idioma de la sesión.
        public ResultadoSuscripcion GenerarComprobantePdf(UsuarioBE usuario, int pagoId, ContextoIdioma idioma, out byte[] pdf, out string nombreArchivo)
        {
            pdf = null;
            nombreArchivo = null;
            PagoBE Pago;
            ResultadoSuscripcion Resultado = ObtenerComprobante(usuario, pagoId, out Pago);
            if (Resultado != ResultadoSuscripcion.Exitoso) return Resultado;
            try
            {
                UsuarioBE Titular = new UsuarioBLL().ObtenerDatos(Pago.UsuarioId);
                List<KeyValuePair<string, string>> Filas = new List<KeyValuePair<string, string>>
                {
                    Fila(idioma, "fact.numero", NumeroComprobante(Pago)),
                    Fila(idioma, "fact.fecha", Pago.Fecha.ToString("dd/MM/yyyy HH:mm")),
                    Fila(idioma, "fact.titular", (Titular.Nombre + " " + Titular.Apellido).Trim() + " (" + Titular.NombreUsuario + ")"),
                    Fila(idioma, "fact.plan", idioma.Traducir("plan.nivel." + Pago.PlanNombre) + " - " + idioma.Traducir("plan.tipo." + Pago.PlanTipo)),
                    Fila(idioma, "fact.concepto", idioma.Traducir("fact.concepto." + Pago.Concepto)),
                    Fila(idioma, "fact.medio", idioma.Traducir("pago.medio." + Pago.MedioPagoId)),
                    Fila(idioma, "fact.monto", FormatearMonto(Pago.Monto)),
                    Fila(idioma, "fact.estado", idioma.Traducir("fact.estado." + Pago.Estado)),
                    Fila(idioma, "fact.operacion", Pago.ComprobanteUrl)
                };
                pdf = new GeneradorPdf().Generar(idioma.Traducir("fact.comprobanteTitulo"), Filas, idioma.Traducir("fact.pie"));
                nombreArchivo = "Comprobante-" + NumeroComprobante(Pago) + ".pdf";
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorPdf;
            }
            new BitacoraBLL().Registrar("Descargar Comprobante", "Pago Id: " + Pago.Id, usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }
        /// Número de comprobante a mostrar: el Id del pago con ceros.
        public static string NumeroComprobante(PagoBE pago)
        {
            return pago.Id.ToString("D8");
        }
        public static string FormatearMonto(decimal monto)
        {
            return "ARS " + monto.ToString("N2", Moneda);
        }

        /// Fin del período abonado. Activa: se renueva con débito automático
        /// (si el cobro se rechaza, vence). Cancelada: vence. Se repite por
        /// si pasó más de un período sin que nadie entrara.
        private void ProcesarFinDePeriodo(SuscripcionBE suscripcion)
        {
            SuscripcionDAL SuscripcionDal = new SuscripcionDAL();
            int Vueltas = 0;
            while (suscripcion.FechaRenovacion <= DateTime.Now && Vueltas++ < 24)
            {
                PlanBE Plan = new PlanDAL().ObtenerDatos(suscripcion.PlanId);
                if (suscripcion.Estado == SuscripcionBE.EstadoCancelada)
                {
                    SuscripcionDal.ActualizarEstado(suscripcion.Id, SuscripcionBE.EstadoVencida);
                    suscripcion.Estado = SuscripcionBE.EstadoVencida;
                    new BitacoraBLL().Registrar("Vencimiento Suscripción", "Plan: " + DescribirPlan(Plan), suscripcion.UsuarioId);
                    GestorEventos.Notificar(CatalogoEventos.SuscripcionVencimiento, suscripcion.UsuarioId, Plan.Nombre);
                    return;
                }
                if (suscripcion.Estado != SuscripcionBE.EstadoActiva) return;
                PagoDAL PagoDal = new PagoDAL();
                PagoBE Pago = new PagoBE
                {
                    SuscripcionId = suscripcion.Id,
                    MedioPagoId = PagoDal.ObtenerUltimoMedioAprobado(suscripcion.UsuarioId) ?? ListarMediosPago().First().Id,
                    Estado = PagoBE.EstadoPendiente,
                    Fecha = DateTime.Now,
                    Monto = Plan.Total,
                    Concepto = PagoBE.ConceptoRenovacion
                };
                PagoDal.Registrar(Pago);
                OperacionPagoBE Operacion;
                try
                {
                    Operacion = ProveedorServicios.PasarelaPago().CobrarRecurrente(Pago.Id, Pago.Monto, "Doverie - " + DescribirPlan(Plan));
                }
                catch (Exception)
                {
                    Operacion = new OperacionPagoBE { Referencia = Pago.Id, Estado = OperacionPagoBE.EstadoRechazado };
                }
                if (Operacion.Estado == OperacionPagoBE.EstadoAprobado)
                {
                    PagoDal.ActualizarEstado(Pago.Id, PagoBE.EstadoAprobado, Operacion.Operacion);
                    suscripcion.FechaRenovacion = suscripcion.FechaRenovacion.AddMonths(Plan.Meses);
                    SuscripcionDal.ActualizarRenovacion(suscripcion.Id, suscripcion.FechaRenovacion);
                    new BitacoraBLL().Registrar("Renovación Suscripción", "Plan: " + DescribirPlan(Plan) + " | Pago Id: " + Pago.Id + " | Hasta: " + suscripcion.FechaRenovacion.ToString("dd/MM/yyyy"), suscripcion.UsuarioId);
                    GestorEventos.Notificar(CatalogoEventos.SuscripcionRenovacion, suscripcion.UsuarioId, Plan.Nombre);
                }
                else
                {
                    PagoDal.ActualizarEstado(Pago.Id, PagoBE.EstadoRechazado, Operacion.Operacion);
                    SuscripcionDal.ActualizarEstado(suscripcion.Id, SuscripcionBE.EstadoVencida);
                    suscripcion.Estado = SuscripcionBE.EstadoVencida;
                    new BitacoraBLL().Registrar("Pago Rechazado", "Renovación | Pago Id: " + Pago.Id + " | Plan: " + DescribirPlan(Plan), suscripcion.UsuarioId);
                    GestorEventos.Notificar(CatalogoEventos.PagoRechazado, suscripcion.UsuarioId, Plan.Nombre);
                    return;
                }
            }
        }
        /// Usuarios de la cuenta: el titular y sus empleados (sin los
        /// revocados).
        private static int ContarUsuarios(int cuentaId)
        {
            return 1 + new UsuarioBLL().ListarPorEmpresa(cuentaId).Count;
        }
        /// Para la Bitácora y las notificaciones (sin traducir).
        private static string DescribirPlan(PlanBE plan)
        {
            return plan == null ? "" : plan.Nombre + " " + plan.Tipo;
        }
        private static KeyValuePair<string, string> Fila(ContextoIdioma idioma, string clave, string valor)
        {
            return new KeyValuePair<string, string>(idioma.Traducir(clave), valor ?? "");
        }
        /// El correo sale en el idioma preferido del titular.
        /// Devuelve false si el titular no tiene email cargado.
        private static bool EnviarConfirmacion(UsuarioBE usuario, PlanBE plan, PagoBE pago, DateTime renovacion, bool esCambio)
        {
            if (string.IsNullOrWhiteSpace(usuario.Gmail)) return false;
            ContextoIdioma Idioma = new IdiomaBLL().CrearContexto(usuario.IdiomaId);
            string NombrePlan = Idioma.Traducir("plan.nivel." + plan.Nombre) + " - " + Idioma.Traducir("plan.tipo." + plan.Tipo);
            string Asunto = Idioma.Traducir(esCambio ? "susc.email.asuntoCambio" : "susc.email.asunto");
            string Cuerpo =
                "<p>" + WebUtility.HtmlEncode(Idioma.Traducir("notif.email.saludo", usuario.Nombre)) + "</p>" +
                "<p>" + WebUtility.HtmlEncode(Idioma.Traducir("susc.email.cuerpo", NombrePlan, FormatearMonto(pago.Monto))) + "</p>" +
                "<p>" + WebUtility.HtmlEncode(Idioma.Traducir("susc.email.renovacion", renovacion.ToString("dd/MM/yyyy"))) + "</p>" +
                "<p style=\"color:#888\">" + WebUtility.HtmlEncode(Idioma.Traducir("susc.email.comprobante", NumeroComprobante(pago))) + "</p>";
            new EmailService().EnviarEmail(usuario.Gmail, Asunto, Cuerpo);
            return true;
        }
    }
}
