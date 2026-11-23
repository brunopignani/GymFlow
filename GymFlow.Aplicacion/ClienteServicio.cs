using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class ClienteServicio
    {
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly IInscripcionRepositorio _inscripcionRepositorio;
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly IComprobanteRepositorio _comprobanteRepositorio;
        private readonly IPagoRepositorio _pagoRepositorio;
        private readonly ITransaccionBD _transaccionBD;

        public ClienteServicio(
            IClienteRepositorio clienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            ICuotaRepositorio cuotaRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            IPagoRepositorio pagoRepositorio,
            ITransaccionBD transaccionBD)
        {
            _clienteRepositorio = clienteRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _cuotaRepositorio = cuotaRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _pagoRepositorio = pagoRepositorio;
            _transaccionBD = transaccionBD;
        }

        public ResultadoAutoingreso ValidarAutoingreso(int dni)
        {
            Cliente cliente = _clienteRepositorio.GetClienteByDNI(dni);

            if (cliente == null)
            {
                return new ResultadoAutoingreso
                {
                    SocioEncontrado = false,
                    AccesoPermitido = false,
                    Dni = dni,
                    Actividades = new List<string>()
                };
            }

            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);
            string estado = DeterminarEstadoCuota(cuotaCliente, DateTime.Today);
            bool accesoPermitido = estado == EstadosCliente.Activo;

            List<Inscripcion> inscripciones = _inscripcionRepositorio.GetInscripcionesByCliente(cliente.Id) ?? new List<Inscripcion>();
            List<string> actividades = inscripciones
                .Where(i => i.Actividad != null && !string.IsNullOrWhiteSpace(i.Actividad.Nombre))
                .Select(i => i.Actividad.Nombre)
                .Distinct()
                .ToList();

            return new ResultadoAutoingreso
            {
                SocioEncontrado = true,
                AccesoPermitido = accesoPermitido,
                Dni = cliente.Dni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                EstadoCuota = estado,
                FechaVencimiento = cuotaCliente?.FechaVencimiento,
                Actividades = actividades
            };
        }

        public List<Cliente> GetAllClientes()
        {
            return _clienteRepositorio.GetAllClientes();
        }

        public Cliente GetClienteByDNI(int dni)
        {
            return _clienteRepositorio.GetClienteByDNI(dni);
        }

        public bool AddCliente(Cliente cliente)
        {
            if (!TieneDatosValidos(cliente))
            {
                return false;
            }

            if (_clienteRepositorio.GetClienteByDNI(cliente.Dni) != null)
            {
                return false;
            }

            List<Cuota> cuotas = _cuotaRepositorio.GetAllCuotas();

            if (cuotas.Count != 1 || !string.Equals(cuotas[0].Nombre, "Mensual", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            cliente.Cuenta = new CuentaCorriente();
            cliente.CuotasCliente = new List<CuotaCliente>
            {
                new CuotaCliente
                {
                    CuotaId = cuotas[0].Id,
                    FechaVencimiento = null,
                    Estado = null
                }
            };

            return _clienteRepositorio.AddCliente(cliente);
        }

        public bool UpdateCliente(Cliente cliente)
        {
            if (!TieneDatosValidos(cliente))
            {
                return false;
            }

            Cliente conMismoDni = _clienteRepositorio.GetClienteByDNI(cliente.Dni);

            if (conMismoDni != null && conMismoDni.Id != cliente.Id)
            {
                return false;
            }

            return _clienteRepositorio.UpdateCliente(cliente);
        }

        public bool DeleteCliente(int id)
        {
            return DeleteClienteConResultado(id) == ResultadoBajaCliente.Eliminado;
        }

        public ResultadoBajaCliente DeleteClienteConResultado(int id)
        {
            if (!_clienteRepositorio.GetAllClientes().Any(c => c.Id == id))
            {
                return ResultadoBajaCliente.ClienteInexistente;
            }

            _transaccionBD.IniciarTransaccion(IsolationLevel.Serializable);

            try
            {
                Cliente cliente = _clienteRepositorio.GetAllClientes().FirstOrDefault(c => c.Id == id);

                if (cliente == null)
                {
                    return RechazarBaja(ResultadoBajaCliente.ClienteInexistente);
                }

                if (cliente.UsuarioId.HasValue)
                {
                    return RechazarBaja(ResultadoBajaCliente.UsuarioAsociado);
                }

                List<CuotaCliente> cuotasCliente = _cuotaClienteRepositorio.GetCuotasByCliente(id);

                if (cuotasCliente.Any(cc => cc.FechaVencimiento.HasValue || cc.Estado != null))
                {
                    return RechazarBaja(ResultadoBajaCliente.CuotasNoIniciales);
                }

                if (_inscripcionRepositorio.GetInscripcionesByCliente(id).Count > 0)
                {
                    return RechazarBaja(ResultadoBajaCliente.Inscripciones);
                }

                CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(id);

                if (cuenta == null)
                {
                    return RechazarBaja(ResultadoBajaCliente.CuentaCorrienteInexistente);
                }

                if (_movimientoRepositorio.GetMovimientosByCuenta(cuenta.Id).Count > 0)
                {
                    return RechazarBaja(ResultadoBajaCliente.Movimientos);
                }

                List<int> idsCuotas = cuotasCliente.Select(cc => cc.Id).ToList();
                List<Comprobante> comprobantes = _comprobanteRepositorio.GetAllComprobantes();
                bool tieneNotaVenta = comprobantes
                    .OfType<NotaVenta>()
                    .Any(nv => idsCuotas.Contains(nv.CuotaClienteId));
                HashSet<int> idsNotasVenta = comprobantes
                    .OfType<NotaVenta>()
                    .Where(nv => idsCuotas.Contains(nv.CuotaClienteId))
                    .Select(nv => nv.Id)
                    .ToHashSet();
                bool tieneNotaCredito = comprobantes
                    .OfType<NotaCredito>()
                    .Any(nc => idsNotasVenta.Contains(nc.NotaVentaId));

                if (tieneNotaVenta || tieneNotaCredito)
                {
                    return RechazarBaja(ResultadoBajaCliente.Comprobantes);
                }

                if (_pagoRepositorio.GetPagosByCliente(id).Count > 0)
                {
                    return RechazarBaja(ResultadoBajaCliente.Pagos);
                }

                foreach (CuotaCliente cuotaCliente in cuotasCliente)
                {
                    if (!_cuotaClienteRepositorio.DeleteCuotaCliente(cuotaCliente.Id))
                    {
                        throw new InvalidOperationException("No se pudo eliminar una cuota inicial del cliente.");
                    }
                }

                if (!_cuentaCorrienteRepositorio.DeleteCuentaCorriente(cuenta.Id))
                {
                    throw new InvalidOperationException("No se pudo eliminar la cuenta corriente vacía del cliente.");
                }

                if (!_clienteRepositorio.DeleteCliente(id))
                {
                    throw new InvalidOperationException("No se pudo eliminar el cliente.");
                }

                _transaccionBD.ConfirmarTransaccion();
                return ResultadoBajaCliente.Eliminado;
            }
            catch
            {
                _transaccionBD.RevertirTransaccion();
                throw;
            }
        }

        private ResultadoBajaCliente RechazarBaja(ResultadoBajaCliente resultado)
        {
            _transaccionBD.RevertirTransaccion();
            return resultado;
        }

        public CuotaCliente GetCuotaActivaByCliente(int clienteId)
        {
            return _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);
        }

        public static string DeterminarEstadoCuota(CuotaCliente cuotaCliente, DateTime? fechaReferencia = null)
        {
            if (cuotaCliente == null)
            {
                return EstadosCliente.SinCuota;
            }

            if (!cuotaCliente.FechaVencimiento.HasValue)
            {
                return EstadosCliente.Pendiente;
            }

            DateTime hoy = fechaReferencia ?? DateTime.Today;
            return cuotaCliente.FechaVencimiento.Value >= hoy ? EstadosCliente.Activo : EstadosCliente.Vencido;
        }

        public List<EstadoCliente> GetEstadoClientes()
        {
            List<EstadoCliente> estados = new List<EstadoCliente>();

            foreach (Cliente cliente in _clienteRepositorio.GetAllClientes())
            {
                EstadoCliente estado = new EstadoCliente();
                estado.Cliente = cliente;
                estado.CuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);
                estado.TieneCuota = estado.CuotaCliente != null;

                estados.Add(estado);
            }

            return estados;
        }

        private bool TieneDatosValidos(Cliente cliente)
        {
            return cliente != null
                && ValidacionesDatos.EsDniValido(cliente.Dni)
                && ValidacionesDatos.EsNombreValido(cliente.Nombre)
                && ValidacionesDatos.EsNombreValido(cliente.Apellido)
                && ValidacionesDatos.EsTelefonoValido(cliente.Telefono);
        }
    }
}
