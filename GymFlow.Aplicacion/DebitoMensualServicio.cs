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
            List<Cliente> clientes = _clienteRepositorio.GetAllClientes();

            TipoMovimiento tipoDebito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Debito);

            if (tipoDebito == null)
            {
                foreach (Cliente clienteSinTipo in clientes)
                {
                    AgregarNoProcesado(resultado, clienteSinTipo, "No existe el tipo de movimiento " + NombresTipoMovimiento.Debito + ".");
                }

                return resultado;
            }

            DateTime hoy = DateTime.Today;

            foreach (Cliente cliente in clientes)
            {
                CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(cliente.Id);

                if (cuenta == null)
                {
                    AgregarNoProcesado(resultado, cliente, "No tiene cuenta corriente.");
                    continue;
                }

                CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);

                if (cuotaCliente == null)
                {
                    AgregarNoProcesado(resultado, cliente, "No tiene cuota asignada.");
                    continue;
                }

                Cuota cuota = _cuotaRepositorio.GetCuotaById(cuotaCliente.CuotaId);

                if (cuota == null || cuota.Precio <= 0)
                {
                    AgregarNoProcesado(resultado, cliente, "La cuota no tiene un precio válido.");
                    continue;
                }

                NotaVenta notaVentaExistente = ObtenerNotaVentaDelPeriodo(cuenta.Id, cuotaCliente.Id, hoy);
                if (notaVentaExistente != null)
                {
                    resultado.YaEmitidos = resultado.YaEmitidos + 1;
                    resultado.Detalles.Add(new DetalleDebitoMensual
                    {
                        Nombre = cliente.Apellido + " " + cliente.Nombre,
                        Dni = cliente.Dni,
                        Resultado = "Ya emitido",
                        NumeroComprobante = notaVentaExistente.Numero
                    });
                    continue;
                }

                if (EmitirDebito(cuenta, cuotaCliente, cuota, tipoDebito, hoy))
                {
                    resultado.Emitidos = resultado.Emitidos + 1;
                    NotaVenta notaVentaEmitida = ObtenerNotaVentaDelPeriodo(cuenta.Id, cuotaCliente.Id, hoy);
                    resultado.Detalles.Add(new DetalleDebitoMensual
                    {
                        Nombre = cliente.Apellido + " " + cliente.Nombre,
                        Dni = cliente.Dni,
                        Resultado = "Emitido",
                        NumeroComprobante = notaVentaEmitida == null ? null : notaVentaEmitida.Numero
                    });
                }
                else
                {
                    AgregarNoProcesado(resultado, cliente, "No se pudo guardar el débito.");
                }
            }

            return resultado;
        }

        private bool TieneDebitoDelPeriodo(int cuentaId, int? cuotaClienteId, DateTime fecha)
        {
            return ObtenerNotaVentaDelPeriodo(cuentaId, cuotaClienteId, fecha) != null;
        }

        private NotaVenta ObtenerNotaVentaDelPeriodo(int cuentaId, int? cuotaClienteId, DateTime fecha)
        {
            List<Movimiento> movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuentaId);

            foreach (Movimiento movimiento in movimientos)
            {
                if (movimiento.Comprobante is NotaVenta
                    && (!cuotaClienteId.HasValue || ((NotaVenta)movimiento.Comprobante).CuotaClienteId == cuotaClienteId.Value)
                    && movimiento.Comprobante.FechaEmision.Year == fecha.Year
                    && movimiento.Comprobante.FechaEmision.Month == fecha.Month)
                {
                    return (NotaVenta)movimiento.Comprobante;
                }
            }

            return null;
        }

        private void AgregarNoProcesado(ResultadoDebitoMensual resultado, Cliente cliente, string motivo)
        {
            string nombre = cliente.Apellido + " " + cliente.Nombre;
            resultado.NoProcesados.Add(nombre + " (DNI " + cliente.Dni + "): " + motivo);
            resultado.Detalles.Add(new DetalleDebitoMensual
            {
                Nombre = nombre,
                Dni = cliente.Dni,
                Resultado = "No procesado",
                Motivo = motivo
            });
        }

        private bool EmitirDebito(CuentaCorriente cuenta, CuotaCliente cuotaCliente, Cuota cuota, TipoMovimiento tipoDebito, DateTime fecha)
        {
            try
            {
                _transaccion.IniciarTransaccion();

                if (!AsegurarNotaVentaDelPeriodo(cuenta, cuotaCliente, cuota.Precio, tipoDebito, fecha))
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

        public bool AsegurarNotaVentaDelPeriodo(
            CuentaCorriente cuenta,
            CuotaCliente cuotaCliente,
            decimal precio,
            TipoMovimiento tipoDebito,
            DateTime fecha)
        {
            if (TieneDebitoDelPeriodo(cuenta.Id, cuotaCliente.Id, fecha))
            {
                return true;
            }

            NotaVenta notaVenta = new NotaVenta();
            notaVenta.Numero = GenerarNumeroNotaVenta();
            notaVenta.FechaEmision = fecha;
            notaVenta.Total = precio;
            notaVenta.CuotaClienteId = cuotaCliente.Id;

            if (!_comprobanteRepositorio.AddComprobante(notaVenta))
            {
                return false;
            }

            Movimiento movimiento = new Movimiento();
            movimiento.Fecha = fecha;
            movimiento.Importe = precio;
            movimiento.TipoMovimientoId = tipoDebito.Id;
            movimiento.ComprobanteId = notaVenta.Id;
            movimiento.CuentaCorrienteId = cuenta.Id;

            return _movimientoRepositorio.AddMovimiento(movimiento);
        }

        private string GenerarNumeroNotaVenta()
        {
            int cantidad = _comprobanteRepositorio.GetAllComprobantes().Count;
            return "NV-" + (cantidad + 1).ToString("D8");
        }
    }
}
