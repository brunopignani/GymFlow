using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Data;

namespace GymFlow.Aplicacion
{
    public class ActividadServicio
    {
        public const int CapacidadMaximaPermitida = 30;

        private readonly IActividadRepositorio _actividadRepositorio;
        private readonly IInscripcionRepositorio _inscripcionRepositorio;
        private readonly ITransaccionBD _transaccion;
        private readonly IContextoUsuarioActual _contextoUsuario;


        public ActividadServicio(
            IActividadRepositorio actividadRepositorio,
            IInscripcionRepositorio inscripcionRepositorio,
            ITransaccionBD transaccion,
            IContextoUsuarioActual contextoUsuario)
        {
            _contextoUsuario = contextoUsuario;
            _actividadRepositorio = actividadRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
            _transaccion = transaccion;
        }

        public List<Actividad> GetAllActividades()
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ActividadesGestionar))
            {
                return new List<Actividad>();
            }

            return _actividadRepositorio.GetAllActividades();
        }

        public bool AddActividad(Actividad actividad)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ActividadesGestionar))
            {
                return false;
            }

            if (actividad == null || string.IsNullOrWhiteSpace(actividad.Nombre))
            {
                return false;
            }

            if (actividad.CapacidadMaxima < 1 || actividad.CapacidadMaxima > CapacidadMaximaPermitida)
            {
                return false;
            }

            return _actividadRepositorio.AddActividad(actividad);
        }

        public bool UpdateActividad(Actividad actividad)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ActividadesGestionar))
            {
                return false;
            }

            if (actividad == null || string.IsNullOrWhiteSpace(actividad.Nombre))
            {
                return false;
            }

            if (actividad.CapacidadMaxima < 1 || actividad.CapacidadMaxima > CapacidadMaximaPermitida)
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

                if (!Existe(actividad.Id))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                int inscriptos = _inscripcionRepositorio.GetInscripcionesByActividad(actividad.Id).Count;

                if (actividad.CapacidadMaxima < inscriptos)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                if (!_actividadRepositorio.UpdateActividad(actividad))
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

        public bool DeleteActividad(int id)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.ActividadesGestionar))
            {
                return false;
            }

            if (!Existe(id))
            {
                return false;
            }

            if (_inscripcionRepositorio.GetInscripcionesByActividad(id).Count > 0)
            {
                return false;
            }

            return _actividadRepositorio.DeleteActividad(id);
        }

        private bool Existe(int id)
        {
            return _actividadRepositorio.GetActividadById(id) != null;
        }
    }
}
