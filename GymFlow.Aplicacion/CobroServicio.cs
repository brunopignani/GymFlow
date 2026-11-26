using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class CobroServicio
    {
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;
        private readonly IComprobanteRepositorio _comprobanteRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly ITipoMovimientoRepositorio _tipoMovimientoRepositorio;
        private readonly ITransaccionBD _transaccion;
        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly DebitoMensualServicio _debitoMensualServicio;

        public static readonly string[] MediosPagoValidos = { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia" };

        public CobroServicio(
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IEmpleadoRepositorio empleadoRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion,
            ICuotaRepositorio cuotaRepositorio,
            DebitoMensualServicio debitoMensualServicio)
        {
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _transaccion = transaccion;
            _cuotaRepositorio = cuotaRepositorio;
            _debitoMensualServicio = debitoMensualServicio;
        }

        public static bool EsMedioPagoValido(string metodoPago)
        {
            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                return false;
            }

            foreach (string valido in MediosPagoValidos)
            {
                if (string.Equals(valido, metodoPago.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool RegistrarCobro(int clienteId, int empleadoId, decimal importe, string metodoPago)
        {
            if (importe <= 0 || !EsMedioPagoValido(metodoPago))
            {
                return false;
            }

            DateTime hoy = DateTime.Today;

            try
            {
                _transaccion.IniciarTransaccion(System.Data.IsolationLevel.Serializable);

                Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);
                CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);
                CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);
                TipoMovimiento tipoCredito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                    .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Credito);
                TipoMovimiento tipoDebito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                    .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Debito);

                if (empleado == null || cuenta == null || cuotaCliente == null || tipoCredito == null || tipoDebito == null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                ResumenCobro resumen = ObtenerResumenCobro(cuenta, cuotaCliente, hoy);

                // Regla: el cobro debe ser exactamente igual al mínimo exigible (sin cobros parciales ni anticipados).
                if (resumen == null || !resumen.PermiteRegistrarCobro || resumen.MinimoExigible <= 0m || importe != resumen.MinimoExigible)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                if (!_debitoMensualServicio.AsegurarNotaVentaDelPeriodo(
                    cuenta,
                    cuotaCliente,
                    resumen.PrecioCuota,
                    tipoDebito,
                    hoy))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                // Si la NV ya existía pero no tenía débito (NV huérfana), registrar el débito faltante.
                if (resumen.TieneNotaVentaDelPeriodo && !resumen.NotaVentaContabilizada && resumen.NotaVentaDelPeriodo != null)
                {
                    Movimiento debitoFaltante = new Movimiento
                    {
                        Fecha = hoy,
                        Importe = resumen.NotaVentaDelPeriodo.Total,
                        TipoMovimientoId = tipoDebito.Id,
                        ComprobanteId = resumen.NotaVentaDelPeriodo.Id,
                        CuentaCorrienteId = cuenta.Id
                    };
                    if (!_movimientoRepositorio.AddMovimiento(debitoFaltante))
                    {
                        _transaccion.RevertirTransaccion();
                        return false;
                    }
                }

                // Registrar recibo y movimiento de crédito.
                string metodoNormalizado = MediosPagoValidos.FirstOrDefault(m => string.Equals(m, metodoPago.Trim(), StringComparison.OrdinalIgnoreCase)) ?? metodoPago.Trim();

                ReciboPago recibo = new ReciboPago();
                recibo.Numero = GenerarNumeroRecibo();
                recibo.FechaEmision = hoy;
                recibo.Total = importe;
                recibo.MetodoPago = metodoNormalizado;
                recibo.EmpleadoId = empleadoId;

                if (!_comprobanteRepositorio.AddComprobante(recibo))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Movimiento movimiento = new Movimiento();
                movimiento.Fecha = hoy;
                movimiento.Importe = importe;
                movimiento.TipoMovimientoId = tipoCredito.Id;
                movimiento.ComprobanteId = recibo.Id;
                movimiento.CuentaCorrienteId = cuenta.Id;

                if (!_movimientoRepositorio.AddMovimiento(movimiento))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                ResumenCobro resumenFinal = ObtenerResumenCobro(cuenta, cuotaCliente, hoy);

                if (resumenFinal == null || resumenFinal.TieneInconsistenciaContable || resumenFinal.Saldo > 0)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                cuotaCliente.FechaVencimiento = hoy.AddDays(30);
                cuotaCliente.Estado = EstadosCliente.Activo;
                if (!_cuotaClienteRepositorio.UpdateCuotaCliente(cuotaCliente))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                _transaccion.ConfirmarTransaccion();
                return true;
            }
            catch (Exception)
            {
                _transaccion.RevertirTransaccion();
                throw;
            }
        }

        public ResumenCobro ObtenerResumenCobro(int clienteId)
        {
            CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);
            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);

            if (cuenta == null || cuotaCliente == null)
            {
                return null;
            }

            return ObtenerResumenCobro(cuenta, cuotaCliente, DateTime.Today);
        }

        private ResumenCobro ObtenerResumenCobro(CuentaCorriente cuenta, CuotaCliente cuotaCliente, DateTime fecha)
        {
            Cuota cuota = _cuotaRepositorio.GetCuotaById(cuotaCliente.CuotaId);

            if (cuota == null || cuota.Precio <= 0)
            {
                return null;
            }

            List<Movimiento> movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuenta.Id);
            decimal saldo = CuentaCorrienteServicio.CalcularSaldo(movimientos);

            DateTime periodo = new DateTime(fecha.Year, fecha.Month, 1);
            NotaVenta notaVenta = _comprobanteRepositorio.ObtenerNotaVentaPorCuotaYPeriodo(cuotaCliente.Id, periodo);

            bool tieneNotaVenta = notaVenta != null;
            bool notaContabilizada = false;
            bool inconsistencia = false;
            decimal deudaPendiente = 0m;

            if (tieneNotaVenta)
            {
                List<Movimiento> movimientosNota = _movimientoRepositorio.GetMovimientosByComprobante(notaVenta.Id);
                bool tieneMovimientoEnOtraCuenta = movimientosNota.Any(m => m.CuentaCorrienteId != cuenta.Id);
                List<Movimiento> debitosEnEstaCuenta = movimientosNota
                    .Where(m => m.CuentaCorrienteId == cuenta.Id && m.TipoMovimiento != null
                        && m.TipoMovimiento.Nombre == NombresTipoMovimiento.Debito)
                    .ToList();

                if (tieneMovimientoEnOtraCuenta)
                {
                    inconsistencia = true;
                }
                else if (movimientosNota.Count == 0)
                {
                    // NV huérfana: sin movimientos, falta registrar el débito.
                    deudaPendiente = notaVenta.Total;
                }
                else if (movimientosNota.Count == 1 && debitosEnEstaCuenta.Count == 1 && debitosEnEstaCuenta[0].Importe == notaVenta.Total)
                {
                    notaContabilizada = true;
                }
                else
                {
                    inconsistencia = true;
                }
            }
            else
            {
                deudaPendiente = cuota.Precio;
            }

            decimal minimo;
            if (inconsistencia)
            {
                minimo = 0m;
            }
            else if (notaContabilizada)
            {
                minimo = Math.Max(0m, saldo);
            }
            else
            {
                minimo = Math.Max(0m, saldo + deudaPendiente);
            }

            string estadoCuota = ClienteServicio.DeterminarEstadoCuota(cuotaCliente, fecha);
            MotivoBloqueoCobro motivoBloqueo;

            if (inconsistencia)
            {
                motivoBloqueo = MotivoBloqueoCobro.InconsistenciaContable;
            }
            else if (estadoCuota == EstadosCliente.Activo)
            {
                motivoBloqueo = MotivoBloqueoCobro.CuotaVigente;
            }
            else if (estadoCuota != EstadosCliente.Pendiente && estadoCuota != EstadosCliente.Vencido)
            {
                motivoBloqueo = MotivoBloqueoCobro.EstadoCuotaNoCobrable;
            }
            else if (minimo <= 0m)
            {
                motivoBloqueo = MotivoBloqueoCobro.SaldoAFavorCubreCuota;
            }
            else
            {
                motivoBloqueo = MotivoBloqueoCobro.Ninguno;
            }

            return new ResumenCobro
            {
                EstadoCuota = estadoCuota,
                MotivoBloqueo = motivoBloqueo,
                CuotaCliente = cuotaCliente,
                PrecioCuota = cuota.Precio,
                Saldo = saldo,
                MinimoExigible = minimo,
                TieneNotaVentaDelPeriodo = tieneNotaVenta,
                NotaVentaContabilizada = notaContabilizada,
                TieneInconsistenciaContable = inconsistencia,
                NotaVentaDelPeriodo = notaVenta
            };
        }

        private string GenerarNumeroRecibo()
        {
            return "RP-" + _comprobanteRepositorio.ObtenerSiguienteNumeroRecibo().ToString("D8");
        }
    }
}
