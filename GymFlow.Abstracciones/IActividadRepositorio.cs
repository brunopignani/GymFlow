using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IActividadRepositorio
    {
        List<Actividad> GetAllActividades();
        bool AddActividad(Actividad actividad);
        bool UpdateActividad(Actividad actividad);
        bool DeleteActividad(int id);
    }
}
