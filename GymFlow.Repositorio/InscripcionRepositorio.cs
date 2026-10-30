using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class InscripcionRepositorio : IInscripcionRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public InscripcionRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool SaveInscripcion(Inscripcion inscripcion)
        {
            _contexto.Inscripciones.Add(inscripcion);
            return _contexto.SaveChanges() > 0;
        }

        public List<Inscripcion> GetInscripcionesByActividad(int actId)
        {
            return _contexto.Inscripciones
                .Include(i => i.Cliente)
                .Where(i => i.ActividadId == actId)
                .ToList();
        }

        public List<Inscripcion> GetInscripcionesByCliente(int cliId)
        {
            return _contexto.Inscripciones
                .Include(i => i.Actividad)
                .Where(i => i.ClienteId == cliId)
                .ToList();
        }

        public bool DeleteInscripcion(int id)
        {
            Inscripcion inscripcion = _contexto.Inscripciones.FirstOrDefault(i => i.Id == id);

            if (inscripcion == null)
            {
                return false;
            }

            _contexto.Inscripciones.Remove(inscripcion);
            return _contexto.SaveChanges() > 0;
        }
    }
}
