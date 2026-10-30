using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    public class DatosEnMemoria
    {
        public List<Cliente> Clientes = new List<Cliente>();
        public List<CuentaCorriente> Cuentas = new List<CuentaCorriente>();
        public List<CuotaCliente> CuotasCliente = new List<CuotaCliente>();
        public List<Cuota> Cuotas = new List<Cuota>();
        public List<Empleado> Empleados = new List<Empleado>();
        public List<Comprobante> Comprobantes = new List<Comprobante>();
        public List<Movimiento> Movimientos = new List<Movimiento>();
        public List<TipoMovimiento> Tipos = new List<TipoMovimiento>();
        public List<Actividad> Actividades = new List<Actividad>();
        public List<Inscripcion> Inscripciones = new List<Inscripcion>();
        public List<Usuario> Usuarios = new List<Usuario>();
        public List<Pago> Pagos = new List<Pago>();

        public int TransaccionesIniciadas;
        public int Confirmadas;
        public int Revertidas;

        public bool FallarAlGuardarMovimiento;
        public bool RechazarGuardarComprobante;
        public bool FallarAlGuardarInscripcion;
        public bool FallarAlGuardarPago;
        public bool FallarAlEliminarInscripcion;
        public int CuentaQueFallaAlGuardarMovimiento;

        public static DatosEnMemoria CrearBase()
        {
            DatosEnMemoria datos = new DatosEnMemoria();

            datos.Tipos.Add(new TipoMovimiento { Id = 1, Nombre = "DEBITO" });
            datos.Tipos.Add(new TipoMovimiento { Id = 2, Nombre = "CREDITO" });
            datos.Empleados.Add(new Empleado { Id = 10 });
            datos.Actividades.Add(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = 30 });
            datos.Cuotas.Add(new Cuota { Id = 1, Nombre = "Mensual", Precio = 1000m });
            datos.Clientes.Add(new Cliente { Id = 1, Dni = 111, Nombre = "Ana", Apellido = "Paz" });
            datos.Cuentas.Add(new CuentaCorriente { Id = 100, ClienteId = 1 });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 5, ClienteId = 1, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(-3), Estado = "VENCIDO" });

            return datos;
        }

        public void AgregarMovimiento(int cuentaId, string tipo, decimal importe, Comprobante comprobante)
        {
            comprobante.Id = Comprobantes.Count + 1;
            Comprobantes.Add(comprobante);

            Movimiento movimiento = new Movimiento();
            movimiento.Id = Movimientos.Count + 1;
            movimiento.Fecha = comprobante.FechaEmision;
            movimiento.Importe = importe;
            movimiento.TipoMovimientoId = Tipos.First(t => t.Nombre == tipo).Id;
            movimiento.ComprobanteId = comprobante.Id;
            movimiento.CuentaCorrienteId = cuentaId;
            Movimientos.Add(movimiento);
        }
    }

    public class CuentaCorrienteRepositorioEnMemoria : ICuentaCorrienteRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public CuentaCorrienteRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public CuentaCorriente GetCuentaCorrienteByCliente(int cliId)
        {
            return _datos.Cuentas.FirstOrDefault(cc => cc.ClienteId == cliId);
        }

        public bool AddCuentaCorriente(CuentaCorriente cuentaCorriente)
        {
            _datos.Cuentas.Add(cuentaCorriente);
            return true;
        }
    }

    public class MovimientoRepositorioEnMemoria : IMovimientoRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public MovimientoRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public bool AddMovimiento(Movimiento movimiento)
        {
            if (_datos.FallarAlGuardarMovimiento || _datos.CuentaQueFallaAlGuardarMovimiento == movimiento.CuentaCorrienteId)
            {
                throw new InvalidOperationException("Falla simulada al guardar el movimiento.");
            }

            movimiento.Id = _datos.Movimientos.Count + 1;
            _datos.Movimientos.Add(movimiento);
            return true;
        }

        public List<Movimiento> GetMovimientosByCuenta(int cuentaId)
        {
            List<Movimiento> movimientos = _datos.Movimientos
                .Where(m => m.CuentaCorrienteId == cuentaId)
                .OrderBy(m => m.Fecha)
                .ToList();

            foreach (Movimiento movimiento in movimientos)
            {
                movimiento.Comprobante = _datos.Comprobantes.FirstOrDefault(c => c.Id == movimiento.ComprobanteId);
                movimiento.TipoMovimiento = _datos.Tipos.FirstOrDefault(t => t.Id == movimiento.TipoMovimientoId);
            }

            return movimientos;
        }
    }

    public class TipoMovimientoRepositorioEnMemoria : ITipoMovimientoRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public TipoMovimientoRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public List<TipoMovimiento> GetAllTiposMovimiento()
        {
            return _datos.Tipos.ToList();
        }
    }

    public class ComprobanteRepositorioEnMemoria : IComprobanteRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public ComprobanteRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public bool AddComprobante(Comprobante comprobante)
        {
            if (_datos.RechazarGuardarComprobante)
            {
                return false;
            }

            comprobante.Id = _datos.Comprobantes.Count + 1;
            _datos.Comprobantes.Add(comprobante);
            return true;
        }

        public List<Comprobante> GetAllComprobantes()
        {
            return _datos.Comprobantes.ToList();
        }

        public Comprobante GetComprobanteById(int id)
        {
            return _datos.Comprobantes.FirstOrDefault(c => c.Id == id);
        }
    }

    public class CuotaClienteRepositorioEnMemoria : ICuotaClienteRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public CuotaClienteRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public bool AddCuotaCliente(CuotaCliente cuotaCliente)
        {
            _datos.CuotasCliente.Add(cuotaCliente);
            return true;
        }

        public List<CuotaCliente> GetCuotasByCliente(int cliId)
        {
            return _datos.CuotasCliente.Where(cc => cc.ClienteId == cliId).ToList();
        }

        public bool UpdateEstadoCuota(int cclId, string nuevoEstado)
        {
            return true;
        }

        public CuotaCliente GetCuotaActivaByCliente(int cliId)
        {
            return _datos.CuotasCliente
                .Where(cc => cc.ClienteId == cliId)
                .OrderByDescending(cc => cc.FechaVencimiento)
                .FirstOrDefault();
        }

        public CuotaCliente GetCuotaClienteById(int id)
        {
            return _datos.CuotasCliente.FirstOrDefault(cc => cc.Id == id);
        }

        public bool UpdateCuotaCliente(CuotaCliente cuotaCliente)
        {
            return true;
        }
    }

    public class EmpleadoRepositorioEnMemoria : IEmpleadoRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public EmpleadoRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        private Empleado CargarUsuario(Empleado empleado)
        {
            if (empleado != null && empleado.UsuarioId.HasValue)
            {
                empleado.Usuario = _datos.Usuarios.FirstOrDefault(u => u.Id == empleado.UsuarioId.Value);
            }

            return empleado;
        }

        public Empleado GetEmpleadoById(int id)
        {
            return CargarUsuario(_datos.Empleados.FirstOrDefault(e => e.Id == id));
        }

        public List<Empleado> GetAllEmpleados()
        {
            return _datos.Empleados.Select(e => CargarUsuario(e)).ToList();
        }

        public Empleado GetEmpleadoByDni(int dni)
        {
            return CargarUsuario(_datos.Empleados.FirstOrDefault(e => e.Dni == dni));
        }

        public Empleado GetEmpleadoByUsuarioId(int usuarioId)
        {
            return CargarUsuario(_datos.Empleados.FirstOrDefault(e => e.UsuarioId == usuarioId));
        }

        public bool AddEmpleado(Empleado empleado)
        {
            empleado.Id = _datos.Empleados.Count == 0 ? 1 : _datos.Empleados.Max(e => e.Id) + 1;

            if (empleado.Usuario != null)
            {
                empleado.Usuario.Id = _datos.Usuarios.Count == 0 ? 1 : _datos.Usuarios.Max(u => u.Id) + 1;
                empleado.UsuarioId = empleado.Usuario.Id;
                _datos.Usuarios.Add(empleado.Usuario);
            }

            _datos.Empleados.Add(empleado);
            return true;
        }

        public bool UpdateEmpleado(Empleado empleado)
        {
            return _datos.Empleados.Any(e => e.Id == empleado.Id);
        }
    }

    public class ClienteRepositorioEnMemoria : IClienteRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public ClienteRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public List<Cliente> GetAllClientes()
        {
            return _datos.Clientes.ToList();
        }

        public Cliente GetClienteByDNI(int dni)
        {
            return _datos.Clientes.FirstOrDefault(c => c.Dni == dni);
        }

        public bool AddCliente(Cliente cliente)
        {
            cliente.Id = _datos.Clientes.Count == 0 ? 1 : _datos.Clientes.Max(c => c.Id) + 1;

            if (cliente.Cuenta != null)
            {
                cliente.Cuenta.Id = _datos.Cuentas.Count == 0 ? 1 : _datos.Cuentas.Max(cc => cc.Id) + 1;
                cliente.Cuenta.ClienteId = cliente.Id;
                _datos.Cuentas.Add(cliente.Cuenta);
            }

            _datos.Clientes.Add(cliente);
            return true;
        }

        public bool UpdateCliente(Cliente cliente)
        {
            Cliente existente = _datos.Clientes.FirstOrDefault(c => c.Id == cliente.Id);

            if (existente == null)
            {
                return false;
            }

            existente.Dni = cliente.Dni;
            existente.Nombre = cliente.Nombre;
            existente.Apellido = cliente.Apellido;
            existente.Telefono = cliente.Telefono;
            return true;
        }

        public bool DeleteCliente(int id)
        {
            Cliente existente = _datos.Clientes.FirstOrDefault(c => c.Id == id);

            if (existente == null)
            {
                return false;
            }

            _datos.Clientes.Remove(existente);
            _datos.Cuentas.RemoveAll(cc => cc.ClienteId == id);
            return true;
        }
    }

    public class CuotaRepositorioEnMemoria : ICuotaRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public CuotaRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public List<Cuota> GetAllCuotas()
        {
            return _datos.Cuotas.ToList();
        }

        public Cuota GetCuotaById(int id)
        {
            return _datos.Cuotas.FirstOrDefault(c => c.Id == id);
        }
    }

    public class TransaccionEnMemoria : ITransaccionBD
    {
        private readonly DatosEnMemoria _datos;
        private List<Comprobante> _comprobantes;
        private List<Movimiento> _movimientos;
        private List<Inscripcion> _inscripciones;
        private List<Pago> _pagos;
        private Dictionary<int, DateTime?> _vencimientos;
        private Dictionary<int, int> _cupos;

        public TransaccionEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public void IniciarTransaccion()
        {
            _datos.TransaccionesIniciadas++;
            _comprobantes = _datos.Comprobantes.ToList();
            _movimientos = _datos.Movimientos.ToList();
            _inscripciones = _datos.Inscripciones.ToList();
            _pagos = _datos.Pagos.ToList();
            _vencimientos = _datos.CuotasCliente.ToDictionary(cc => cc.Id, cc => cc.FechaVencimiento);
            _cupos = _datos.Actividades.ToDictionary(a => a.Id, a => a.CupoMaximo);
        }

        public void ConfirmarTransaccion()
        {
            _datos.Confirmadas++;
            Descartar();
        }

        public void RevertirTransaccion()
        {
            _datos.Revertidas++;

            if (_comprobantes == null)
            {
                return;
            }

            _datos.Comprobantes.Clear();
            _datos.Comprobantes.AddRange(_comprobantes);
            _datos.Movimientos.Clear();
            _datos.Movimientos.AddRange(_movimientos);
            _datos.Inscripciones.Clear();
            _datos.Inscripciones.AddRange(_inscripciones);
            _datos.Pagos.Clear();
            _datos.Pagos.AddRange(_pagos);

            foreach (CuotaCliente cuotaCliente in _datos.CuotasCliente)
            {
                if (_vencimientos.ContainsKey(cuotaCliente.Id))
                {
                    cuotaCliente.FechaVencimiento = _vencimientos[cuotaCliente.Id];
                }
            }

            foreach (Actividad actividad in _datos.Actividades)
            {
                if (_cupos.ContainsKey(actividad.Id))
                {
                    actividad.CupoMaximo = _cupos[actividad.Id];
                }
            }

            Descartar();
        }

        private void Descartar()
        {
            _comprobantes = null;
            _movimientos = null;
            _inscripciones = null;
            _pagos = null;
            _vencimientos = null;
            _cupos = null;
        }
    }

    public class ActividadRepositorioEnMemoria : IActividadRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public ActividadRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public List<Actividad> GetAllActividades()
        {
            return _datos.Actividades.ToList();
        }

        public bool UpdateCupo(int id, int cupoRestante)
        {
            Actividad actividad = _datos.Actividades.FirstOrDefault(a => a.Id == id);

            if (actividad == null)
            {
                return false;
            }

            actividad.CupoMaximo = cupoRestante;
            return true;
        }

        public bool AddActividad(Actividad actividad)
        {
            actividad.Id = _datos.Actividades.Count == 0 ? 1 : _datos.Actividades.Max(a => a.Id) + 1;
            _datos.Actividades.Add(actividad);
            return true;
        }

        public bool UpdateActividad(Actividad actividad)
        {
            Actividad existente = _datos.Actividades.FirstOrDefault(a => a.Id == actividad.Id);

            if (existente == null)
            {
                return false;
            }

            existente.Nombre = actividad.Nombre;
            existente.CupoMaximo = actividad.CupoMaximo;
            return true;
        }

        public bool DeleteActividad(int id)
        {
            return _datos.Actividades.RemoveAll(a => a.Id == id) > 0;
        }
    }

    public class InscripcionRepositorioEnMemoria : IInscripcionRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public InscripcionRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public bool SaveInscripcion(Inscripcion inscripcion)
        {
            if (_datos.FallarAlGuardarInscripcion)
            {
                throw new InvalidOperationException("Falla simulada al guardar la inscripción.");
            }

            inscripcion.Id = _datos.Inscripciones.Count == 0 ? 1 : _datos.Inscripciones.Max(i => i.Id) + 1;
            _datos.Inscripciones.Add(inscripcion);
            return true;
        }

        public List<Inscripcion> GetInscripcionesByActividad(int actId)
        {
            List<Inscripcion> inscripciones = _datos.Inscripciones.Where(i => i.ActividadId == actId).ToList();

            foreach (Inscripcion inscripcion in inscripciones)
            {
                inscripcion.Cliente = _datos.Clientes.FirstOrDefault(c => c.Id == inscripcion.ClienteId);
            }

            return inscripciones;
        }

        public List<Inscripcion> GetInscripcionesByCliente(int cliId)
        {
            List<Inscripcion> inscripciones = _datos.Inscripciones.Where(i => i.ClienteId == cliId).ToList();

            foreach (Inscripcion inscripcion in inscripciones)
            {
                inscripcion.Actividad = _datos.Actividades.FirstOrDefault(a => a.Id == inscripcion.ActividadId);
            }

            return inscripciones;
        }

        public bool DeleteInscripcion(int id)
        {
            if (_datos.FallarAlEliminarInscripcion)
            {
                throw new InvalidOperationException("Falla simulada al eliminar la inscripción.");
            }

            return _datos.Inscripciones.RemoveAll(i => i.Id == id) > 0;
        }
    }

    public class UsuarioRepositorioEnMemoria : IUsuarioRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public UsuarioRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public List<Usuario> GetAllUsuarios()
        {
            return _datos.Usuarios.ToList();
        }

        public Usuario GetUsuarioById(int id)
        {
            return _datos.Usuarios.FirstOrDefault(u => u.Id == id);
        }

        public bool AddUsuario(Usuario usuario)
        {
            usuario.Id = _datos.Usuarios.Count == 0 ? 1 : _datos.Usuarios.Max(u => u.Id) + 1;
            _datos.Usuarios.Add(usuario);
            return true;
        }

        public Usuario GetUsuarioByNombre(string nombre)
        {
            return _datos.Usuarios.FirstOrDefault(u => u.Nombre == nombre);
        }

        public bool UpdateUsuario(Usuario usuario)
        {
            return _datos.Usuarios.Any(u => u.Id == usuario.Id);
        }
    }

    public class PagoRepositorioEnMemoria : IPagoRepositorio
    {
        private readonly DatosEnMemoria _datos;

        public PagoRepositorioEnMemoria(DatosEnMemoria datos)
        {
            _datos = datos;
        }

        public bool AddPago(Pago pago)
        {
            if (_datos.FallarAlGuardarPago)
            {
                throw new InvalidOperationException("Falla simulada al guardar el pago.");
            }

            pago.Id = _datos.Pagos.Count == 0 ? 1 : _datos.Pagos.Max(p => p.Id) + 1;
            _datos.Pagos.Add(pago);
            return true;
        }

        public List<Pago> GetAllPagos()
        {
            return _datos.Pagos.ToList();
        }

        public List<Pago> GetPagosByCliente(int cliId)
        {
            return _datos.Pagos.Where(p => _datos.CuotasCliente.Any(cc => cc.Id == p.CuotaClienteId && cc.ClienteId == cliId)).ToList();
        }
    }
}
