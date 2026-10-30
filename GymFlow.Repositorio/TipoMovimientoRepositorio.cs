using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class TipoMovimientoRepositorio : ITipoMovimientoRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public TipoMovimientoRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<TipoMovimiento> GetAllTiposMovimiento()
        {
            return _contexto.TiposMovimiento.ToList();
        }
    }
}
