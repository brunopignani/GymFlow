using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
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

        public InscripcionServicio(
            ICuotaClienteRepositorio cuotaClienteRepositorio,
            IActividadRepositorio actividadRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            IClienteRepositorio clienteRepositorio,
            ITransaccionBD transaccion)
        {
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _actividadRepositorio = actividadRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _clienteRepositorio = clienteRepositorio;
            _transaccion = transaccion;
        }

        public bool InscribirCliente(int clienteId, int actividadId)
        {
            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(clienteId);

            if (cuotaCliente == null || !cuotaCliente.FechaVencimiento.HasValue || cuotaCliente.FechaVencimiento.Value < DateTime.Today)
            {
                return false;
            }

            Actividad actividad = BuscarActividad(actividadId);

            if (actividad == null || actividad.CupoMaximo <= 0)
            {
                return false;
            }

            if (BuscarInscripcion(clienteId, actividadId) != null)
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion();

                if (!_actividadRepositorio.UpdateCupo(actividad.Id, actividad.CupoMaximo - 1))
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
            Inscripcion inscripcion = BuscarInscripcion(clienteId, actividadId);
            Actividad actividad = BuscarActividad(actividadId);

            if (inscripcion == null || actividad == null)
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion();

                if (!EliminarYLiberarCupo(inscripcion))
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
            return _inscripcionRepositorio.GetInscripcionesByActividad(actId);
        }

        public List<Inscripcion> GetInscripcionesByCliente(int cliId)
        {
            return _inscripcionRepositorio.GetInscripcionesByCliente(cliId);
        }

        public ResultadoBajaMorosos DarDeBajaInscripcionesDeMorosos()
        {
            ResultadoBajaMorosos resultado = new ResultadoBajaMorosos();
            DateTime hoy = DateTime.Today;

            foreach (Cliente cliente in _clienteRepositorio.GetAllClientes())
            {
                CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaActivaByCliente(cliente.Id);

                if (cuotaCliente == null || (cuotaCliente.FechaVencimiento.HasValue && cuotaCliente.FechaVencimiento.Value >= hoy))
                {
                    continue;
                }

                List<Inscripcion> inscripciones = _inscripcionRepositorio.GetInscripcionesByCliente(cliente.Id);

                if (inscripciones.Count == 0)
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
                _transaccion.IniciarTransaccion();

                foreach (Inscripcion inscripcion in inscripciones)
                {
                    if (!EliminarYLiberarCupo(inscripcion))
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

        private bool EliminarYLiberarCupo(Inscripcion inscripcion)
        {
            Actividad actividad = BuscarActividad(inscripcion.ActividadId);

            if (actividad == null)
            {
                return false;
            }

            if (!_inscripcionRepositorio.DeleteInscripcion(inscripcion.Id))
            {
                return false;
            }

            return _actividadRepositorio.UpdateCupo(actividad.Id, actividad.CupoMaximo + 1);
        }

        private Actividad BuscarActividad(int actividadId)
        {
            return _actividadRepositorio.GetAllActividades().FirstOrDefault(a => a.Id == actividadId);
        }

        private Inscripcion BuscarInscripcion(int clienteId, int actividadId)
        {
            return _inscripcionRepositorio.GetInscripcionesByCliente(clienteId).FirstOrDefault(i => i.ActividadId == actividadId);
        }
    }
}
