using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class PagoRepositorio : IPagoRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public PagoRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool AddPago(Pago pago)
        {
            _contexto.Pagos.Add(pago);
            return _contexto.SaveChanges() > 0;
        }

        public List<Pago> GetAllPagos()
        {
            return _contexto.Pagos.ToList();
        }

        public List<Pago> GetPagosByCliente(int cliId)
        {
            return _contexto.Pagos.Where(p => p.CuotaCliente.ClienteId == cliId).ToList();
        }
    }
}
