using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class PersonaRepositorio : IPersonaRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public PersonaRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool ExisteDni(int dni, int? excluirPersonaId = null)
        {
            return _contexto.Personas.Any(p =>
                p.Dni == dni && (!excluirPersonaId.HasValue || p.Id != excluirPersonaId.Value));
        }
    }
}
