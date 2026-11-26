using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GymFlow.Aplicacion
{
    public class ClienteServicio
    {
        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly IPersonaRepositorio _personaRepositorio;
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly IInscripcionRepositorio _inscripcionRepositorio;
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly IComprobanteRepositorio _comprobanteRepositorio;
        private readonly IAuditoriaRepositorio _auditoriaRepositorio;
        private readonly ITransaccionBD _transaccionBD;
        private readonly IContextoUsuarioActual _contextoUsuario;


        public ClienteServicio(
            IClienteRepositorio clienteRepositorio,
            IPersonaRepositorio personaRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            ICuotaRepositorio cuotaRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            IAuditoriaRepositorio auditoriaRepositorio,
            ITransaccionBD transaccionBD,
            IContextoUsuarioActual contextoUsuario)
        {
            _contextoUsuario = contextoUsuario;
            _clienteRepositorio = clienteRepositorio;
            _personaRepositorio = personaRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _cuotaRepositorio = cuotaRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _auditoriaRepositorio = auditoriaRepositorio;
            _transaccionBD = transaccionBD;
        }

        public ResultadoAutoingreso ValidarAutoingreso(int dni)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.AutoingresoUsar))
            {
                return new ResultadoAutoingreso();
            }

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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return new List<Cliente>();
            }

            return _clienteRepositorio.GetAllClientes();
        }

        public Cliente GetClienteByDNI(int dni)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return null;
            }

            return _clienteRepositorio.GetClienteByDNI(dni);
        }

        public bool AddCliente(Cliente cliente)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return false;
            }

            if (!TieneDatosValidos(cliente))
            {
                return false;
            }

            if (_personaRepositorio.ExisteDni(cliente.Dni))
            {
                return false;
            }

            List<Cuota> cuotas = _cuotaRepositorio.GetAllCuotas();

            if (cuotas.Count != 1 || !string.Equals(cuotas[0].Nombre, "Mensual", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _transaccionBD.IniciarTransaccion();

            try
            {
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

                if (!_clienteRepositorio.AddCliente(cliente))
                {
                    _transaccionBD.RevertirTransaccion();
                    return false;
                }

                string valoresNuevos = SerializarSnapshot(cliente.Dni, cliente.Nombre, cliente.Apellido, cliente.Telefono);
                AuditoriaCliente auditoria = CrearAuditoriaCliente(
                    OperacionesAuditoria.Alta,
                    cliente.Id,
                    cliente.Dni,
                    null,
                    valoresNuevos);

                if (!_auditoriaRepositorio.RegistrarCliente(auditoria))
                {
                    throw new InvalidOperationException("No se pudo registrar la auditoría de alta del cliente.");
                }

                _transaccionBD.ConfirmarTransaccion();
                return true;
            }
            catch
            {
                _transaccionBD.RevertirTransaccion();
                throw;
            }
        }

        public bool UpdateCliente(Cliente cliente)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return false;
            }

            if (!TieneDatosValidos(cliente))
            {
                return false;
            }

            if (_personaRepositorio.ExisteDni(cliente.Dni, cliente.Id))
            {
                return false;
            }

            _transaccionBD.IniciarTransaccion();

            try
            {
                Cliente actual = _clienteRepositorio.GetClienteById(cliente.Id);

                if (actual == null)
                {
                    _transaccionBD.RevertirTransaccion();
                    return false;
                }

                int dniAnterior = actual.Dni;
                string nombreAnterior = actual.Nombre;
                string apellidoAnterior = actual.Apellido;
                string telefonoAnterior = actual.Telefono;

                bool huboCambios = dniAnterior != cliente.Dni
                    || !string.Equals(nombreAnterior, cliente.Nombre, StringComparison.Ordinal)
                    || !string.Equals(apellidoAnterior, cliente.Apellido, StringComparison.Ordinal)
                    || !string.Equals(telefonoAnterior, cliente.Telefono, StringComparison.Ordinal);

                if (!_clienteRepositorio.UpdateCliente(cliente))
                {
                    _transaccionBD.RevertirTransaccion();
                    return false;
                }

                if (huboCambios)
                {
                    string valoresAnteriores = SerializarSnapshot(dniAnterior, nombreAnterior, apellidoAnterior, telefonoAnterior);
                    string valoresNuevos = SerializarSnapshot(cliente.Dni, cliente.Nombre, cliente.Apellido, cliente.Telefono);
                    AuditoriaCliente auditoria = CrearAuditoriaCliente(
                        OperacionesAuditoria.Modificacion,
                        cliente.Id,
                        cliente.Dni,
                        valoresAnteriores,
                        valoresNuevos);

                    if (!_auditoriaRepositorio.RegistrarCliente(auditoria))
                    {
                        throw new InvalidOperationException("No se pudo registrar la auditoría de modificación del cliente.");
                    }
                }

                _transaccionBD.ConfirmarTransaccion();
                return true;
            }
            catch
            {
                _transaccionBD.RevertirTransaccion();
                throw;
            }
        }

        public bool DeleteCliente(int id)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return false;
            }

            return DeleteClienteConResultado(id) == ResultadoBajaCliente.Eliminado;
        }

        public ResultadoBajaCliente DeleteClienteConResultado(int id)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return ResultadoBajaCliente.SinPermiso;
            }

            if (_clienteRepositorio.GetClienteById(id) == null)
            {
                return ResultadoBajaCliente.ClienteInexistente;
            }

            _transaccionBD.IniciarTransaccion(IsolationLevel.Serializable);

            try
            {
                Cliente cliente = _clienteRepositorio.GetClienteById(id);

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

                if (_comprobanteRepositorio.ExisteComprobanteParaCuotas(idsCuotas))
                {
                    return RechazarBaja(ResultadoBajaCliente.Comprobantes);
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

                int dniAnterior = cliente.Dni;
                string nombreAnterior = cliente.Nombre;
                string apellidoAnterior = cliente.Apellido;
                string telefonoAnterior = cliente.Telefono;

                if (!_clienteRepositorio.DeleteCliente(id))
                {
                    throw new InvalidOperationException("No se pudo eliminar el cliente.");
                }

                string valoresAnteriores = SerializarSnapshot(dniAnterior, nombreAnterior, apellidoAnterior, telefonoAnterior);
                AuditoriaCliente auditoria = CrearAuditoriaCliente(
                    OperacionesAuditoria.Baja,
                    id,
                    dniAnterior,
                    valoresAnteriores,
                    null);

                if (!_auditoriaRepositorio.RegistrarCliente(auditoria))
                {
                    throw new InvalidOperationException("No se pudo registrar la auditoría de baja del cliente.");
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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return null;
            }

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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ClientesGestionar))
            {
                return new List<EstadoCliente>();
            }

            List<EstadoCliente> estados = new List<EstadoCliente>();
            Dictionary<int, CuotaCliente> cuotasActivas = _cuotaClienteRepositorio.GetCuotasActivasPorCliente();

            foreach (Cliente cliente in _clienteRepositorio.GetAllClientes())
            {
                cuotasActivas.TryGetValue(cliente.Id, out CuotaCliente cuotaCliente);

                EstadoCliente estado = new EstadoCliente();
                estado.Cliente = cliente;
                estado.CuotaCliente = cuotaCliente;
                estado.TieneCuota = cuotaCliente != null;

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

        private static string SerializarSnapshot(int dni, string nombre, string apellido, string telefono)
        {
            return JsonSerializer.Serialize(new
            {
                Dni = dni,
                Nombre = nombre,
                Apellido = apellido,
                Telefono = telefono
            }, OpcionesJson);
        }

        private AuditoriaCliente CrearAuditoriaCliente(
            string operacion,
            int clienteId,
            int dniCliente,
            string valoresAnteriores,
            string valoresNuevos)
        {
            return new AuditoriaCliente
            {
                FechaHora = DateTime.Now,
                Operacion = operacion,
                ClienteId = clienteId,
                DniCliente = dniCliente,
                UsuarioId = _contextoUsuario.UsuarioId,
                EmpleadoId = _contextoUsuario.EmpleadoId > 0 ? _contextoUsuario.EmpleadoId : (int?)null,
                NombreUsuario = string.IsNullOrWhiteSpace(_contextoUsuario.NombreUsuario) ? "Desconocido" : _contextoUsuario.NombreUsuario,
                ValoresAnteriores = valoresAnteriores,
                ValoresNuevos = valoresNuevos
            };
        }
    }
}
