using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class ActividadServicio
    {
        public const int CupoMaximoPermitido = 30;

        private readonly IActividadRepositorio _actividadRepositorio;
        private readonly IInscripcionRepositorio _inscripcionRepositorio;

        public ActividadServicio(IActividadRepositorio actividadRepositorio, IInscripcionRepositorio inscripcionRepositorio)
        {
            _actividadRepositorio = actividadRepositorio;
            _inscripcionRepositorio = inscripcionRepositorio;
        }

        public List<Actividad> GetAllActividades()
        {
            return _actividadRepositorio.GetAllActividades();
        }

        public bool AddActividad(Actividad actividad)
        {
            if (actividad == null || string.IsNullOrWhiteSpace(actividad.Nombre))
            {
                return false;
            }

            if (actividad.CupoMaximo < 1 || actividad.CupoMaximo > CupoMaximoPermitido)
            {
                return false;
            }

            return _actividadRepositorio.AddActividad(actividad);
        }

        public bool UpdateActividad(Actividad actividad)
        {
            if (actividad == null || string.IsNullOrWhiteSpace(actividad.Nombre))
            {
                return false;
            }

            if (actividad.CupoMaximo < 0 || actividad.CupoMaximo > CupoMaximoPermitido)
            {
                return false;
            }

            if (!Existe(actividad.Id))
            {
                return false;
            }

            int inscriptos = _inscripcionRepositorio.GetInscripcionesByActividad(actividad.Id).Count;

            if (actividad.CupoMaximo + inscriptos > CupoMaximoPermitido)
            {
                return false;
            }

            return _actividadRepositorio.UpdateActividad(actividad);
        }

        public bool DeleteActividad(int id)
        {
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
            return _actividadRepositorio.GetAllActividades().Exists(a => a.Id == id);
        }
    }
}
