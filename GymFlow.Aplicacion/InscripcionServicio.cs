using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class InscripcionServicio
    {
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly IActividadRepositorio _actividadRepositorio;
        private readonly IInscripcionRepositorio _inscripcionRepositorio;
        private readonly IClienteRepositorio _clienteRepositorio;
        private readonly ITransaccionBD _transaccion;
        private readonly IContextoUsuarioActual _contextoUsuario;


        public InscripcionServicio(
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IActividadRepositorio actividadRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            IClienteRepositorio clienteRepositorio,
            ITransaccionBD transaccion,
            IContextoUsuarioActual contextoUsuario)
        {
            _contextoUsuario = contextoUsuario;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _actividadRepositorio = actividadRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _transaccion = transaccion;
        }

        public bool InscribirCliente(int clienteId, int actividadId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.InscripcionesGestionar))
            {
                return false;
            }

            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);

            if (cuotaCliente == null || !cuotaCliente.FechaVencimiento.HasValue || cuotaCliente.FechaVencimiento.Value < DateTime.Today)
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

                Actividad actividad = BuscarActividad(actividadId);

                if (actividad == null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                if (BuscarInscripcion(clienteId, actividadId) != null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                int inscriptos = _inscripcionRepositorio.GetInscripcionesByActividad(actividadId).Count;
                int disponibles = actividad.CapacidadMaxima - inscriptos;

                if (disponibles <= 0)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Inscripcion inscripcion = new Inscripcion();
                inscripcion.ClienteId = clienteId;
                inscripcion.ActividadId = actividadId;
                inscripcion.Fecha = DateTime.Today;

                if (!_inscripcionRepositorio.SaveInscripcion(inscripcion))
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

        public bool DesinscribirCliente(int clienteId, int actividadId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.InscripcionesGestionar))
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

                Inscripcion inscripcion = BuscarInscripcion(clienteId, actividadId);
                Actividad actividad = BuscarActividad(actividadId);

                if (inscripcion == null || actividad == null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                if (!_inscripcionRepositorio.DeleteInscripcion(inscripcion.Id))
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

        public List<Inscripcion> GetInscripcionesByActividad(int actId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.InscripcionesGestionar))
            {
                return new List<Inscripcion>();
            }

            return _inscripcionRepositorio.GetInscripcionesByActividad(actId);
        }

        public List<Inscripcion> GetInscripcionesByCliente(int cliId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.InscripcionesGestionar))
            {
                return new List<Inscripcion>();
            }

            return _inscripcionRepositorio.GetInscripcionesByCliente(cliId);
        }

        public ResultadoBajaMorosos DarDeBajaInscripcionesDeMorosos()
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.MorososBaja))
            {
                return new ResultadoBajaMorosos();
            }

            ResultadoBajaMorosos resultado = new ResultadoBajaMorosos();
            DateTime hoy = DateTime.Today;
            Dictionary<int, CuotaCliente> cuotasActivas = _cuotaClienteRepositorio.GetCuotasActivasPorCliente();
            List<Cliente> morosos = new List<Cliente>();

            foreach (Cliente cliente in _clienteRepositorio.GetAllClientes())
            {
                if (!cuotasActivas.TryGetValue(cliente.Id, out CuotaCliente cuotaCliente))
                {
                    continue;
                }

                if (cuotaCliente.FechaVencimiento.HasValue && cuotaCliente.FechaVencimiento.Value >= hoy)
                {
                    continue;
                }

                morosos.Add(cliente);
            }

            if (morosos.Count == 0)
            {
                return resultado;
            }

            Dictionary<int, List<Inscripcion>> inscripcionesPorCliente = _inscripcionRepositorio
                .GetInscripcionesByClientes(morosos.Select(c => c.Id).ToList())
                .GroupBy(i => i.ClienteId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (Cliente cliente in morosos)
            {
                if (!inscripcionesPorCliente.TryGetValue(cliente.Id, out List<Inscripcion> inscripciones) || inscripciones.Count == 0)
                {
                    continue;
                }

                int eliminadas = DarDeBajaDelCliente(inscripciones);

                if (eliminadas > 0)
                {
                    resultado.ClientesAfectados = resultado.ClientesAfectados + 1;
                    resultado.InscripcionesEliminadas = resultado.InscripcionesEliminadas + eliminadas;
                }
                else
                {
                    resultado.NoProcesados.Add(cliente.Apellido + " " + cliente.Nombre + " (DNI " + cliente.Dni + "): no se pudieron dar de baja sus inscripciones.");
                }
            }

            return resultado;
        }

        private int DarDeBajaDelCliente(List<Inscripcion> inscripciones)
        {
            try
            {
                _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

                foreach (Inscripcion inscripcion in inscripciones)
                {
                    if (!_inscripcionRepositorio.DeleteInscripcion(inscripcion.Id))
                    {
                        _transaccion.RevertirTransaccion();
                        return 0;
                    }
                }

                _transaccion.ConfirmarTransaccion();
                return inscripciones.Count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                _transaccion.RevertirTransaccion();
                return 0;
            }
        }

        private Actividad BuscarActividad(int actividadId)
        {
            return _actividadRepositorio.GetActividadById(actividadId);
        }

        private Inscripcion BuscarInscripcion(int clienteId, int actividadId)
        {
            return _inscripcionRepositorio.GetInscripcionesByCliente(clienteId).FirstOrDefault(i => i.ActividadId == actividadId);
        }
    }
}
