using BE;
using DAL;
using SERVICE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

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
                DateTime Ahora = AhoraSinMilisegundos();
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
                urlCheckout = IniciarCobro(Pago, "Doverie - " + DescribirPlan(Plan), urlRetorno);
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
                Operacion = ProveedorServicios.PasarelaPago().ConsultarPago(pago.Id, pago.Fecha, pago.ComprobanteUrl);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.PagoPendiente;
            }
            if (Operacion.Estado == OperacionPagoBE.EstadoPendiente) return ResultadoSuscripcion.PagoPendiente;
            ValidarMonto(Operacion, pago);
            if (pago.Concepto == PagoBE.ConceptoRenovacion) return ConfirmarRenovacion(usuario, ref pago, Operacion);
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
                pdf = PdfComprobante(Pago, Titular, idioma, out nombreArchivo);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorPdf;
            }
            new BitacoraBLL().Registrar("Descargar Comprobante", "Pago Id: " + Pago.Id, usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }
        /// Reenvía por correo el comprobante de un pago aprobado de la cuenta
        /// (botón "Enviar por mail" de Facturación). destino es el email al
        /// que se mandó: el del titular.
        public ResultadoSuscripcion EnviarComprobantePorEmail(UsuarioBE usuario, int pagoId, out string destino)
        {
            destino = null;
            PagoBE Pago;
            ResultadoSuscripcion Resultado = ObtenerComprobante(usuario, pagoId, out Pago);
            if (Resultado != ResultadoSuscripcion.Exitoso) return Resultado;
            UsuarioBE Titular = new UsuarioBLL().ObtenerDatos(Pago.UsuarioId);
            try
            {
                ContextoIdioma Idioma = new IdiomaBLL().CrearContexto(Titular.IdiomaId);
                if (!EnviarFactura(Titular, Pago, Idioma, Idioma.Traducir("susc.email.asuntoComprobante", NumeroComprobante(Pago)),
                    new List<string> { Idioma.Traducir("susc.email.cuerpoComprobante") }))
                {
                    return ResultadoSuscripcion.ErrorEmail;
                }
            }
            catch (Exception)
            {
                new BitacoraBLL().Registrar("Error Envío Email", "Comprobante del Pago Id: " + Pago.Id, usuario.Id);
                return ResultadoSuscripcion.ErrorEmail;
            }
            destino = Titular.Gmail;
            new BitacoraBLL().Registrar("Enviar Comprobante por Email", "Pago Id: " + Pago.Id, usuario.Id);
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
        /// (si el cobro se rechaza, vence) o, si la pasarela no debita sola,
        /// queda la renovación pendiente de pago con plazo de gracia.
        /// Cancelada: vence. Se repite por si pasó más de un período sin que
        /// nadie entrara.
        private void ProcesarFinDePeriodo(SuscripcionBE suscripcion)
        {
            SuscripcionDAL SuscripcionDal = new SuscripcionDAL();
            int Vueltas = 0;
            while (suscripcion.FechaRenovacion <= DateTime.Now && Vueltas++ < 24)
            {
                PlanBE Plan = new PlanDAL().ObtenerDatos(suscripcion.PlanId);
                if (suscripcion.Estado == SuscripcionBE.EstadoCancelada)
                {
                    Vencer(suscripcion, Plan);
                    return;
                }
                if (suscripcion.Estado != SuscripcionBE.EstadoActiva) return;
                IPasarelaPago Pasarela = ProveedorServicios.PasarelaPago();
                if (!Pasarela.DebitoAutomatico)
                {
                    if (!ProcesarRenovacionManual(suscripcion, Plan, Pasarela)) return;
                    continue;
                }
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
                    AplicarRenovacion(suscripcion, Plan, Pago.Id, Operacion.Operacion);
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

        // ---------- Renovación sin débito automático (Mercado Pago) ----------

        /// Días que se mantiene el acceso después del fin del período para
        /// pagar la renovación.
        public const int DiasGracia = 7;
        /// Si la pasarela configurada renueva sola (débito automático) o la
        /// renovación la paga el Usuario.
        public static bool RenovacionAutomatica
        {
            get { return ProveedorServicios.PasarelaPago().DebitoAutomatico; }
        }

        /// La renovación de una suscripción Activa cuyo período terminó y que
        /// la pasarela no puede debitar sola: queda un pago de renovación
        /// pendiente y el acceso sigue durante el plazo de gracia; si se cumple
        /// sin pago, vence. Devuelve true si la suscripción quedó renovada.
        private bool ProcesarRenovacionManual(SuscripcionBE suscripcion, PlanBE plan, IPasarelaPago pasarela)
        {
            PagoDAL PagoDal = new PagoDAL();
            DateTime FinGracia = suscripcion.FechaRenovacion.AddDays(DiasGracia);
            List<PagoBE> Renovaciones = PagosDeRenovacion(suscripcion);
            PagoBE Pendiente = Renovaciones.FirstOrDefault(P => P.Estado == PagoBE.EstadoPendiente);
            if (DateTime.Now >= FinGracia)
            {
                // Antes de darla por vencida, por si pagó a último momento.
                if (Pendiente != null)
                {
                    try
                    {
                        OperacionPagoBE Operacion = pasarela.ConsultarPago(Pendiente.Id, Pendiente.Fecha, Pendiente.ComprobanteUrl);
                        ValidarMonto(Operacion, Pendiente);
                        if (Operacion.Estado == OperacionPagoBE.EstadoAprobado)
                        {
                            AplicarRenovacion(suscripcion, plan, Pendiente.Id, Operacion.Operacion);
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        // Sin respuesta de la pasarela se espera al próximo
                        // ingreso: no se da por vencida una suscripción que
                        // quizás se pagó.
                        suscripcion.FinGracia = FinGracia;
                        return false;
                    }
                    PagoDal.ActualizarEstado(Pendiente.Id, PagoBE.EstadoAnulado, null);
                }
                Vencer(suscripcion, plan);
                return false;
            }
            suscripcion.FinGracia = FinGracia;
            if (Renovaciones.Count == 0)
            {
                PagoBE Pago = NuevoPagoRenovacion(suscripcion, plan);
                new BitacoraBLL().Registrar("Renovación Pendiente de Pago", "Plan: " + DescribirPlan(plan) + " | Pago Id: " + Pago.Id + " | Plazo: " + FinGracia.ToString("dd/MM/yyyy"), suscripcion.UsuarioId);
                GestorEventos.Notificar(CatalogoEventos.RenovacionPendiente, suscripcion.UsuarioId, plan.Nombre);
            }
            return false;
        }
        /// Renovación pendiente de pago de la cuenta (con su plazo de gracia), o
        /// null si no hay ninguna.
        public PagoBE ObtenerRenovacionPendiente(UsuarioBE usuario, out SuscripcionBE suscripcion)
        {
            suscripcion = EsTitular(usuario) ? ObtenerVigente(usuario) : null;
            if (suscripcion == null || !suscripcion.FinGracia.HasValue) return null;
            SuscripcionBE Vigente = suscripcion;
            PagoBE Pendiente = PagosDeRenovacion(Vigente).FirstOrDefault(P => P.Estado == PagoBE.EstadoPendiente);
            // Si el pago anterior se rechazó, se genera otro para reintentar.
            return Pendiente ?? NuevoPagoRenovacion(Vigente, Vigente.Plan);
        }
        /// Lleva al checkout de la pasarela para pagar la renovación pendiente.
        public ResultadoSuscripcion PagarRenovacion(UsuarioBE usuario, string urlRetorno, out string urlCheckout)
        {
            urlCheckout = null;
            if (!EsTitular(usuario)) return ResultadoSuscripcion.SinAcceso;
            SuscripcionBE Suscripcion;
            PagoBE Pago = ObtenerRenovacionPendiente(usuario, out Suscripcion);
            if (Pago == null) return ResultadoSuscripcion.SinSuscripcion;
            try
            {
                urlCheckout = IniciarCobro(Pago, "Doverie - Renovación " + DescribirPlan(Suscripcion.Plan), urlRetorno);
            }
            catch (Exception)
            {
                return ResultadoSuscripcion.ErrorPago;
            }
            new BitacoraBLL().Registrar("Iniciar Pago Renovación", "Pago Id: " + Pago.Id + " | Plan: " + DescribirPlan(Suscripcion.Plan) + " | Monto: " + FormatearMonto(Pago.Monto), usuario.Id);
            return ResultadoSuscripcion.Exitoso;
        }
        /// Crea el cobro en la pasarela y, si la pasarela lo identifica con su
        /// propio número (la order de Mercado Pago), lo guarda en el Pago
        /// pendiente para poder consultarlo al volver del checkout.
        private static string IniciarCobro(PagoBE pago, string descripcion, string urlRetorno)
        {
            string Cobro;
            string Url = ProveedorServicios.PasarelaPago().IniciarPago(pago.Id, pago.Fecha, pago.Monto, descripcion, urlRetorno + "?pago=" + pago.Id, out Cobro);
            if (Cobro != null) new PagoDAL().ActualizarEstado(pago.Id, PagoBE.EstadoPendiente, Cobro);
            return Url;
        }
        /// Resultado del pago de una renovación al volver del checkout.
        private ResultadoSuscripcion ConfirmarRenovacion(UsuarioBE usuario, ref PagoBE pago, OperacionPagoBE operacion)
        {
            PagoDAL PagoDal = new PagoDAL();
            lock (BloqueoRenovacion)
            {
                // Se relee adentro del bloqueo: dos vueltas del checkout a la
                // vez no pueden extender dos veces el período.
                pago = PagoDal.ObtenerDatos(pago.Id);
                if (pago.Estado == PagoBE.EstadoAprobado) return ResultadoSuscripcion.Exitoso;
                if (pago.Estado != PagoBE.EstadoPendiente) return ResultadoSuscripcion.ErrorPago;
                SuscripcionBE Suscripcion = new SuscripcionDAL().ObtenerDatos(pago.SuscripcionId);
                PlanBE Plan = new PlanDAL().ObtenerDatos(Suscripcion.PlanId);
                if (operacion.Estado != OperacionPagoBE.EstadoAprobado)
                {
                    // Sigue en el plazo de gracia: puede reintentar con otro medio.
                    PagoDal.ActualizarEstado(pago.Id, PagoBE.EstadoRechazado, operacion.Operacion);
                    new BitacoraBLL().Registrar("Pago Rechazado", "Renovación | Pago Id: " + pago.Id + " | Plan: " + DescribirPlan(Plan), usuario.Id);
                    GestorEventos.Notificar(CatalogoEventos.PagoRechazado, Suscripcion.UsuarioId, Plan.Nombre);
                    pago = PagoDal.ObtenerDatos(pago.Id);
                    return ResultadoSuscripcion.ErrorPago;
                }
                AplicarRenovacion(Suscripcion, Plan, pago.Id, operacion.Operacion);
                pago = PagoDal.ObtenerDatos(pago.Id);
                return ResultadoSuscripcion.Exitoso;
            }
        }
        private static void AplicarRenovacion(SuscripcionBE suscripcion, PlanBE plan, int pagoId, string operacion)
        {
            new PagoDAL().ActualizarEstado(pagoId, PagoBE.EstadoAprobado, operacion);
            suscripcion.FechaRenovacion = suscripcion.FechaRenovacion.AddMonths(plan.Meses);
            suscripcion.FinGracia = null;
            new SuscripcionDAL().ActualizarRenovacion(suscripcion.Id, suscripcion.FechaRenovacion);
            new BitacoraBLL().Registrar("Renovación Suscripción", "Plan: " + DescribirPlan(plan) + " | Pago Id: " + pagoId + " | Hasta: " + suscripcion.FechaRenovacion.ToString("dd/MM/yyyy"), suscripcion.UsuarioId);
            GestorEventos.Notificar(CatalogoEventos.SuscripcionRenovacion, suscripcion.UsuarioId, plan.Nombre);
            // Factura de la renovación por correo. Si falla, la renovación
            // igual quedó hecha: se registra en la Bitácora y se sigue.
            try
            {
                UsuarioBE Titular = new UsuarioBLL().ObtenerDatos(suscripcion.UsuarioId);
                ContextoIdioma Idioma = new IdiomaBLL().CrearContexto(Titular.IdiomaId);
                EnviarFactura(Titular, new PagoDAL().ObtenerDatos(pagoId), Idioma, Idioma.Traducir("susc.email.asuntoRenovacion"),
                    new List<string> { Idioma.Traducir("susc.email.cuerpoRenovacion", NombrePlan(plan, Idioma), suscripcion.FechaRenovacion.ToString("dd/MM/yyyy")) });
            }
            catch (Exception)
            {
                new BitacoraBLL().Registrar("Error Envío Email", "Factura de la renovación, Pago Id: " + pagoId, suscripcion.UsuarioId);
            }
        }
        /// Pagos de renovación del período que terminó (creados desde su fin).
        private static List<PagoBE> PagosDeRenovacion(SuscripcionBE suscripcion)
        {
            return new PagoDAL().ListarPorUsuario(suscripcion.UsuarioId)
                .Where(P => P.SuscripcionId == suscripcion.Id && P.Concepto == PagoBE.ConceptoRenovacion && P.Fecha >= suscripcion.FechaRenovacion)
                .ToList();
        }
        private PagoBE NuevoPagoRenovacion(SuscripcionBE suscripcion, PlanBE plan)
        {
            PagoDAL PagoDal = new PagoDAL();
            PagoBE Pago = new PagoBE
            {
                SuscripcionId = suscripcion.Id,
                MedioPagoId = PagoDal.ObtenerUltimoMedioAprobado(suscripcion.UsuarioId) ?? ListarMediosPago().First().Id,
                Estado = PagoBE.EstadoPendiente,
                Fecha = AhoraSinMilisegundos(),
                Monto = plan.Total,
                Concepto = PagoBE.ConceptoRenovacion
            };
            PagoDal.Registrar(Pago);
            return Pago;
        }
        private static void Vencer(SuscripcionBE suscripcion, PlanBE plan)
        {
            new SuscripcionDAL().ActualizarEstado(suscripcion.Id, SuscripcionBE.EstadoVencida);
            suscripcion.Estado = SuscripcionBE.EstadoVencida;
            suscripcion.FinGracia = null;
            new BitacoraBLL().Registrar("Vencimiento Suscripción", "Plan: " + DescribirPlan(plan), suscripcion.UsuarioId);
            GestorEventos.Notificar(CatalogoEventos.SuscripcionVencimiento, suscripcion.UsuarioId, plan.Nombre);
        }
        /// Un cobro aprobado por menos de lo que vale el pago no lo paga.
        private static void ValidarMonto(OperacionPagoBE operacion, PagoBE pago)
        {
            if (operacion.Estado == OperacionPagoBE.EstadoAprobado && operacion.Monto > 0 && operacion.Monto < pago.Monto)
            {
                new BitacoraBLL().Registrar("Pago Rechazado", "Pago Id: " + pago.Id + " | Monto cobrado " + FormatearMonto(operacion.Monto) + " menor que " + FormatearMonto(pago.Monto), null);
                operacion.Estado = OperacionPagoBE.EstadoRechazado;
            }
        }
        /// Fecha de los pagos sin milisegundos: es parte de la referencia del
        /// cobro en la pasarela y tiene que leerse igual desde la base.
        private static DateTime AhoraSinMilisegundos()
        {
            DateTime Ahora = DateTime.Now;
            return new DateTime(Ahora.Year, Ahora.Month, Ahora.Day, Ahora.Hour, Ahora.Minute, Ahora.Second);
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
        /// Confirmación de la compra, en el idioma preferido del titular.
        /// Devuelve false si el titular no tiene email cargado.
        private static bool EnviarConfirmacion(UsuarioBE usuario, PlanBE plan, PagoBE pago, DateTime renovacion, bool esCambio)
        {
            ContextoIdioma Idioma = new IdiomaBLL().CrearContexto(usuario.IdiomaId);
            return EnviarFactura(usuario, pago, Idioma, Idioma.Traducir(esCambio ? "susc.email.asuntoCambio" : "susc.email.asunto"), new List<string>
            {
                Idioma.Traducir("susc.email.cuerpo", NombrePlan(plan, Idioma), FormatearMonto(pago.Monto)),
                Idioma.Traducir("susc.email.renovacion", renovacion.ToString("dd/MM/yyyy"))
            });
        }
        /// La factura por correo: los datos de la compra en el cuerpo y el
        /// comprobante en PDF adjunto (el mismo que se descarga desde
        /// Facturación). Devuelve false si el titular no tiene email cargado.
        private static bool EnviarFactura(UsuarioBE titular, PagoBE pago, ContextoIdioma idioma, string asunto, List<string> parrafos)
        {
            if (string.IsNullOrWhiteSpace(titular.Gmail)) return false;
            string NombreArchivo;
            byte[] Pdf = PdfComprobante(pago, titular, idioma, out NombreArchivo);
            StringBuilder Cuerpo = new StringBuilder();
            Cuerpo.Append("<p>" + WebUtility.HtmlEncode(idioma.Traducir("notif.email.saludo", titular.Nombre)) + "</p>");
            foreach (string Parrafo in parrafos)
            {
                Cuerpo.Append("<p>" + WebUtility.HtmlEncode(Parrafo) + "</p>");
            }
            Cuerpo.Append("<table style=\"border-collapse:collapse;font-size:14px\">");
            foreach (KeyValuePair<string, string> Dato in FilasComprobante(pago, titular, idioma))
            {
                Cuerpo.Append("<tr><td style=\"padding:4px 16px 4px 0;color:#666\">" + WebUtility.HtmlEncode(Dato.Key) +
                    "</td><td style=\"padding:4px 0\"><b>" + WebUtility.HtmlEncode(Dato.Value) + "</b></td></tr>");
            }
            Cuerpo.Append("</table>");
            Cuerpo.Append("<p style=\"color:#888\">" + WebUtility.HtmlEncode(idioma.Traducir("susc.email.adjunto", NombreArchivo)) + "</p>");
            new EmailService().EnviarEmail(titular.Gmail, asunto, Cuerpo.ToString(), Pdf, NombreArchivo);
            return true;
        }
        /// Datos del comprobante: los mismos en el PDF y en el correo.
        private static List<KeyValuePair<string, string>> FilasComprobante(PagoBE pago, UsuarioBE titular, ContextoIdioma idioma)
        {
            return new List<KeyValuePair<string, string>>
            {
                Fila(idioma, "fact.numero", NumeroComprobante(pago)),
                Fila(idioma, "fact.fecha", pago.Fecha.ToString("dd/MM/yyyy HH:mm")),
                Fila(idioma, "fact.titular", (titular.Nombre + " " + titular.Apellido).Trim() + " (" + titular.NombreUsuario + ")"),
                Fila(idioma, "fact.plan", idioma.Traducir("plan.nivel." + pago.PlanNombre) + " - " + idioma.Traducir("plan.tipo." + pago.PlanTipo)),
                Fila(idioma, "fact.concepto", idioma.Traducir("fact.concepto." + pago.Concepto)),
                Fila(idioma, "fact.medio", idioma.Traducir("pago.medio." + pago.MedioPagoId)),
                Fila(idioma, "fact.monto", FormatearMonto(pago.Monto)),
                Fila(idioma, "fact.estado", idioma.Traducir("fact.estado." + pago.Estado)),
                Fila(idioma, "fact.operacion", pago.ComprobanteUrl)
            };
        }
        private static byte[] PdfComprobante(PagoBE pago, UsuarioBE titular, ContextoIdioma idioma, out string nombreArchivo)
        {
            nombreArchivo = "Comprobante-" + NumeroComprobante(pago) + ".pdf";
            return new GeneradorPdf().Generar(idioma.Traducir("fact.comprobanteTitulo"), FilasComprobante(pago, titular, idioma), idioma.Traducir("fact.pie"));
        }
        private static string NombrePlan(PlanBE plan, ContextoIdioma idioma)
        {
            return idioma.Traducir("plan.nivel." + plan.Nombre) + " - " + idioma.Traducir("plan.tipo." + plan.Tipo);
        }
    }
}
