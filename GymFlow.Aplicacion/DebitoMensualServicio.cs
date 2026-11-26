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
        private readonly IContextoUsuarioActual _contextoUsuario;


        public DebitoMensualServicio(
            IClienteRepositorio clienteRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            ICuotaRepositorio cuotaRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion,
            IContextoUsuarioActual contextoUsuario)
        {
            _contextoUsuario = contextoUsuario;
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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.DebitoMensualEmitir))
            {
                return new ResultadoDebitoMensual();
            }

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

                if (!cuotaCliente.FechaVencimiento.HasValue)
                {
                    AgregarNoProcesado(resultado, cliente, "La cuota está pendiente de primera activación.");
                    continue;
                }

                Cuota cuota = _cuotaRepositorio.GetCuotaById(cuotaCliente.CuotaId);

                if (cuota == null || cuota.Precio <= 0)
                {
                    AgregarNoProcesado(resultado, cliente, "La cuota no tiene un precio válido.");
                    continue;
                }

                NotaVenta notaVentaExistente = ObtenerNotaVentaDelPeriodo(cuotaCliente.Id, hoy);
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
                    NotaVenta notaVentaEmitida = ObtenerNotaVentaDelPeriodo(cuotaCliente.Id, hoy);
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

        private bool TieneDebitoDelPeriodo(int cuotaClienteId, DateTime fecha)
        {
            return ObtenerNotaVentaDelPeriodo(cuotaClienteId, fecha) != null;
        }

        private NotaVenta ObtenerNotaVentaDelPeriodo(int cuotaClienteId, DateTime fecha)
        {
            DateTime periodo = new DateTime(fecha.Year, fecha.Month, 1);
            return _comprobanteRepositorio.ObtenerNotaVentaPorCuotaYPeriodo(cuotaClienteId, periodo);
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
            if (TieneDebitoDelPeriodo(cuotaCliente.Id, fecha))
            {
                return true;
            }

            NotaVenta notaVenta = new NotaVenta();
            notaVenta.Numero = GenerarNumeroNotaVenta();
            notaVenta.FechaEmision = fecha;
            notaVenta.Periodo = new DateTime(fecha.Year, fecha.Month, 1);
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
            return "NV-" + _comprobanteRepositorio.ObtenerSiguienteNumeroNotaVenta().ToString("D8");
        }
    }
}
