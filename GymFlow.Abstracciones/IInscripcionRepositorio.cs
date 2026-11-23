using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IInscripcionRepositorio
    {
        bool SaveInscripcion(Inscripcion inscripcion);
        List<Inscripcion> GetInscripcionesByActividad(int actId);
        List<Inscripcion> GetInscripcionesByCliente(int cliId);
        List<Inscripcion> GetInscripcionesByClientes(List<int> clienteIds);
        bool DeleteInscripcion(int id);
    }
}
