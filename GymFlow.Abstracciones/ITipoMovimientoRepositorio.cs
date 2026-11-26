using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface ITipoMovimientoRepositorio
    {
        List<TipoMovimiento> GetAllTiposMovimiento();
        bool AddTipoMovimiento(TipoMovimiento tipoMovimiento);
    }
}

