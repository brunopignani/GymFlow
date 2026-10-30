using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class DebitoMensualServicio
    {
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly IComprobanteRepositorio _comprobanteRepositorio;
        private readonly ITipoMovimientoRepositorio _tipoMovimientoRepositorio;
        private readonly ITransaccionBD _transaccion;

        public DebitoMensualServicio(
            IClienteRepositorio clienteRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            ICuotaRepositorio cuotaRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion)
        {
            _clienteRepositorio = clienteRepositorio;
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _cuotaRepositorio = cuotaRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _transaccion = transaccion;
        }

        public ResultadoDebitoMensual EmitirDebitosMensuales()
        {
            ResultadoDebitoMensual resultado = new ResultadoDebitoMensual();

            TipoMovimiento tipoDebito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Debito);

            if (tipoDebito == null)
            {
                resultado.NoProcesados.Add("No existe el tipo de movimiento " + NombresTipoMovimiento.Debito + ".");
                return resultado;
            }

            DateTime hoy = DateTime.Today;
            List<Cliente> clientes = _clienteRepositorio.GetAllClientes();

            foreach (Cliente cliente in clientes)
            {
                string nombreCliente = cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + ")";

                CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(cliente.Id);

                if (cuenta == null)
                {
                    resultado.NoProcesados.Add(nombreCliente + ": no tiene cuenta corriente.");
                    continue;
                }

                CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);

                if (cuotaCliente == null)
                {
                    resultado.NoProcesados.Add(nombreCliente + ": no tiene cuota asignada.");
                    continue;
                }

                Cuota cuota = _cuotaRepositorio.GetCuotaById(cuotaCliente.CuotaId);

                if (cuota == null || cuota.Precio <= 0)
                {
                    resultado.NoProcesados.Add(nombreCliente + ": la cuota no tiene un precio válido.");
                    continue;
                }

                if (TieneDebitoDelPeriodo(cuenta.Id, hoy))
                {
                    resultado.YaEmitidos = resultado.YaEmitidos + 1;
                    continue;
                }

                if (EmitirDebito(cuenta, cuotaCliente, cuota, tipoDebito, hoy))
                {
                    resultado.Emitidos = resultado.Emitidos + 1;
                }
                else
                {
                    resultado.NoProcesados.Add(nombreCliente + ": no se pudo guardar el débito.");
                }
            }

            return resultado;
        }

        private bool TieneDebitoDelPeriodo(int cuentaId, DateTime fecha)
        {
            List<Movimiento> movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuentaId);

            foreach (Movimiento movimiento in movimientos)
            {
                if (movimiento.Comprobante is NotaVenta
                    && movimiento.Comprobante.FechaEmision.Year == fecha.Year
                    && movimiento.Comprobante.FechaEmision.Month == fecha.Month)
                {
                    return true;
                }
            }

            return false;
        }

        private bool EmitirDebito(CuentaCorriente cuenta, CuotaCliente cuotaCliente, Cuota cuota, TipoMovimiento tipoDebito, DateTime fecha)
        {
            try
            {
                _transaccion.IniciarTransaccion();

                NotaVenta notaVenta = new NotaVenta();
                notaVenta.Numero = GenerarNumeroNotaVenta();
                notaVenta.FechaEmision = fecha;
                notaVenta.Total = cuota.Precio;
                notaVenta.CuotaClienteId = cuotaCliente.Id;

                if (!_comprobanteRepositorio.AddComprobante(notaVenta))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Movimiento movimiento = new Movimiento();
                movimiento.Fecha = fecha;
                movimiento.Importe = cuota.Precio;
                movimiento.TipoMovimientoId = tipoDebito.Id;
                movimiento.ComprobanteId = notaVenta.Id;
                movimiento.CuentaCorrienteId = cuenta.Id;

                if (!_movimientoRepositorio.AddMovimiento(movimiento))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                _transaccion.ConfirmarTransaccion();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                _transaccion.RevertirTransaccion();
                return false;
            }
        }

        private string GenerarNumeroNotaVenta()
        {
            int cantidad = _comprobanteRepositorio.GetAllComprobantes().Count;
            return "NV-" + (cantidad + 1).ToString("D8");
        }
    }
}
