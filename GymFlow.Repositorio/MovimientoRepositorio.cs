using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class MovimientoRepositorio : IMovimientoRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public MovimientoRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool AddMovimiento(Movimiento movimiento)
        {
            _contexto.Movimientos.Add(movimiento);
            return _contexto.SaveChanges() > 0;
        }

        public List<Movimiento> GetMovimientosByCuenta(int cuentaId)
        {
            return _contexto.Movimientos
                .Include(m => m.TipoMovimiento)
                .Include(m => m.Comprobante)
                .Where(m => m.CuentaCorrienteId == cuentaId)
                .OrderBy(m => m.Fecha)
                .ToList();
        }
    }
}
