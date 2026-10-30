using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class ComprobanteRepositorio : IComprobanteRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public ComprobanteRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool AddComprobante(Comprobante comprobante)
        {
            _contexto.Comprobantes.Add(comprobante);
            return _contexto.SaveChanges() > 0;
        }

        public List<Comprobante> GetAllComprobantes()
        {
            return _contexto.Comprobantes
                .Include(c => ((ReciboPago)c).Empleado)
                .Include(c => ((NotaVenta)c).CuotaCliente)
                    .ThenInclude(cc => cc.Cuota)
                .ToList();
        }

        public Comprobante GetComprobanteById(int id)
        {
            return _contexto.Comprobantes
                .Include(c => ((ReciboPago)c).Empleado)
                .Include(c => ((NotaVenta)c).CuotaCliente)
                    .ThenInclude(cc => cc.Cuota)
                .FirstOrDefault(c => c.Id == id);
        }
    }
}
