using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
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

        public static readonly string[] MediosPagoValidos = { "Efectivo", "Tarjeta de Débito", "Tarjeta de Crédito", "Transferencia" };

        public CobroServicio(
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IEmpleadoRepositorio empleadoRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion)
        {
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _transaccion = transaccion;
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

            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);
            CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);
            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);
            TipoMovimiento tipoCredito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Credito);

            if (empleado == null || cuenta == null || cuotaCliente == null || tipoCredito == null)
            {
                return false;
            }

            DateTime hoy = DateTime.Today;

            try
            {
                _transaccion.IniciarTransaccion();

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

                // Calcular saldo después del crédito.
                var movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuenta.Id);
                decimal saldo = 0m;
                foreach (var m in movimientos)
                {
                    if (m.TipoMovimiento.Nombre == NombresTipoMovimiento.Debito)
                        saldo += m.Importe;
                    else if (m.TipoMovimiento.Nombre == NombresTipoMovimiento.Credito)
                        saldo -= m.Importe;
                    else
                        throw new InvalidOperationException($"El movimiento {m.Id} tiene un tipo desconocido.");
                }

                // Si el saldo resultante <= 0, activar cuota y fijar vencimiento.
                if (saldo <= 0)
                {
                    cuotaCliente.FechaVencimiento = hoy.AddDays(30);
                    cuotaCliente.Estado = "ACTIVO";
                    if (!_cuotaClienteRepositorio.UpdateCuotaCliente(cuotaCliente))
                    {
                        _transaccion.RevertirTransaccion();
                        return false;
                    }
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

        private string GenerarNumeroRecibo()
        {
            int cantidad = _comprobanteRepositorio.GetAllComprobantes().Count;
            return "RP-" + (cantidad + 1).ToString("D8");
        }
    }
}
