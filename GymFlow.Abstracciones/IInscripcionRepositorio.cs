using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IInscripcionRepositorio
    {
        bool SaveInscripcion(Inscripcion inscripcion);
        List<Inscripcion> GetInscripcionesByActividad(int actId);
        List<Inscripcion> GetInscripcionesByCliente(int cliId);
        bool DeleteInscripcion(int id);
    }
}
