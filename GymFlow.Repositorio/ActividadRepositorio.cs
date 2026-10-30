using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class ActividadRepositorio : IActividadRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public ActividadRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<Actividad> GetAllActividades()
        {
            return _contexto.Actividades.ToList();
        }

        public bool UpdateCupo(int id, int cupoRestante)
        {
            Actividad actividad = _contexto.Actividades.FirstOrDefault(a => a.Id == id);

            if (actividad == null)
            {
                return false;
            }

            actividad.CupoMaximo = cupoRestante;
            return _contexto.SaveChanges() > 0;
        }

        public bool AddActividad(Actividad actividad)
        {
            _contexto.Actividades.Add(actividad);
            return _contexto.SaveChanges() > 0;
        }

        public bool UpdateActividad(Actividad actividad)
        {
            Actividad actual = _contexto.Actividades.FirstOrDefault(a => a.Id == actividad.Id);

            if (actual == null)
            {
                return false;
            }

            _contexto.Entry(actual).CurrentValues.SetValues(actividad);
            _contexto.SaveChanges();
            return true;
        }

        public bool DeleteActividad(int id)
        {
            Actividad actividad = _contexto.Actividades.FirstOrDefault(a => a.Id == id);

            if (actividad == null)
            {
                return false;
            }

            _contexto.Actividades.Remove(actividad);
            return _contexto.SaveChanges() > 0;
        }
    }
}
