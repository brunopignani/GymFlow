using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IMovimientoRepositorio
    {
        bool AddMovimiento(Movimiento movimiento);
        List<Movimiento> GetMovimientosByCuenta(int cuentaId);
    }
}
