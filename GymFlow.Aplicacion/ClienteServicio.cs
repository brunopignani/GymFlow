using GymFlow.Abstracciones;
using GymFlow.Dominio;
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

        public ClienteServicio(
            IClienteRepositorio clienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IMovimientoRepositorio movimientoRepositorio)
            : this(clienteRepositorio, cuotaClienteRepositorio, inscripcionRepositorio, cuentaCorrienteRepositorio, movimientoRepositorio, null)
        {
        }

        public ClienteServicio(
            IClienteRepositorio clienteRepositorio,
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            ICuotaRepositorio cuotaRepositorio)
        {
            _clienteRepositorio = clienteRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _cuotaRepositorio = cuotaRepositorio;
        }

        public bool ValidarAutoingreso(int dni)
        {
            Cliente cliente = _clienteRepositorio.GetClienteByDNI(dni);

            if (cliente == null)
            {
                return false;
            }

            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);

            if (cuotaCliente == null)
            {
                return false;
            }

            return cuotaCliente.FechaVencimiento.HasValue && cuotaCliente.FechaVencimiento.Value >= DateTime.Today;
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

            cliente.Cuenta = new CuentaCorriente();

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
            Cliente cliente = _clienteRepositorio.GetAllClientes().FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return false;
            }

            if (_cuotaClienteRepositorio.GetCuotasByCliente(id).Count > 0)
            {
                return false;
            }

            if (_inscripcionRepositorio.GetInscripcionesByCliente(id).Count > 0)
            {
                return false;
            }

            CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(id);

            if (cuenta != null && _movimientoRepositorio.GetMovimientosByCuenta(cuenta.Id).Count > 0)
            {
                return false;
            }

            return _clienteRepositorio.DeleteCliente(id);
        }

        public CuotaCliente GetCuotaActivaByCliente(int clienteId)
        {
            return _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);
        }

        public List<Cuota> GetAllCuotas()
        {
            return _cuotaRepositorio != null ? _cuotaRepositorio.GetAllCuotas() : new List<Cuota>();
        }

        public bool AsignarCuota(int clienteId, int cuotaId)
        {
            Cliente cliente = _clienteRepositorio.GetAllClientes().FirstOrDefault(c => c.Id == clienteId);

            if (cliente == null)
            {
                return false;
            }

            if (_cuotaRepositorio != null && _cuotaRepositorio.GetCuotaById(cuotaId) == null)
            {
                return false;
            }

            // Si ya existe una cuota activa (con vencimiento futuro), no sobrescribir.
            CuotaCliente activa = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);

            if (activa != null && activa.FechaVencimiento.HasValue && activa.FechaVencimiento.Value >= DateTime.Today)
            {
                return false;
            }

            if (activa != null)
            {
                // Reutilizar el registro existente, actualizar cuota pero limpiar vigencia.
                activa.CuotaId = cuotaId;
                activa.FechaVencimiento = null;
                activa.Estado = null;
                return _cuotaClienteRepositorio.UpdateCuotaCliente(activa);
            }

            CuotaCliente nueva = new CuotaCliente();
            nueva.ClienteId = clienteId;
            nueva.CuotaId = cuotaId;
            nueva.FechaVencimiento = null;
            nueva.Estado = null;

            return _cuotaClienteRepositorio.AddCuotaCliente(nueva);
        }

        public List<EstadoCliente> GetEstadoClientes()
        {
            List<EstadoCliente> estados = new List<EstadoCliente>();
            DateTime hoy = DateTime.Today;

            foreach (Cliente cliente in _clienteRepositorio.GetAllClientes())
            {
                EstadoCliente estado = new EstadoCliente();
                estado.Cliente = cliente;
                estado.CuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);
                estado.TieneCuota = estado.CuotaCliente != null;
                estado.Vigente = estado.TieneCuota && estado.CuotaCliente.FechaVencimiento.HasValue && estado.CuotaCliente.FechaVencimiento.Value >= hoy;

                estados.Add(estado);
            }

            return estados;
        }

        private bool TieneDatosValidos(Cliente cliente)
        {
            return cliente != null
                && cliente.Dni > 0
                && !string.IsNullOrWhiteSpace(cliente.Nombre)
                && !string.IsNullOrWhiteSpace(cliente.Apellido);
        }
    }
}
