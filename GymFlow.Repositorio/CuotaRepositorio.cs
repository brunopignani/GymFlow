using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class CuotaRepositorio : ICuotaRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public CuotaRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<Cuota> GetAllCuotas()
        {
            return _contexto.Cuotas.ToList();
        }

        public Cuota GetCuotaById(int id)
        {
            return _contexto.Cuotas.FirstOrDefault(c => c.Id == id);
        }
    }
}
