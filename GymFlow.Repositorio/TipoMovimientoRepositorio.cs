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

        public bool AddTipoMovimiento(TipoMovimiento tipoMovimiento)
        {
            _contexto.TiposMovimiento.Add(tipoMovimiento);
            return _contexto.SaveChanges() > 0;
        }

        public List<TipoMovimiento> GetAllTiposMovimiento()
        {
            return _contexto.TiposMovimiento.ToList();
        }
    }
}

