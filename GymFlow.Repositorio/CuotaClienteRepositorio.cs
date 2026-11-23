using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class CuotaClienteRepositorio : ICuotaClienteRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public CuotaClienteRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool AddCuotaCliente(CuotaCliente cuotaCliente)
        {
            _contexto.CuotasCliente.Add(cuotaCliente);
            return _contexto.SaveChanges() > 0;
        }

        public bool DeleteCuotaCliente(int id)
        {
            CuotaCliente cuotaCliente = _contexto.CuotasCliente.FirstOrDefault(cc => cc.Id == id);

            if (cuotaCliente == null)
            {
                return false;
            }

            _contexto.CuotasCliente.Remove(cuotaCliente);
            return _contexto.SaveChanges() > 0;
        }

        public List<CuotaCliente> GetCuotasByCliente(int cliId)
        {
            return _contexto.CuotasCliente.Where(cc => cc.ClienteId == cliId).ToList();
        }

        public CuotaCliente GetCuotaActivaByCliente(int cliId)
        {
            return _contexto.CuotasCliente
                .Where(cc => cc.ClienteId == cliId)
                .OrderByDescending(cc => cc.Id)
                .FirstOrDefault();
        }

        public Dictionary<int, CuotaCliente> GetCuotasActivasPorCliente()
        {
            return _contexto.CuotasCliente
                .GroupBy(cc => cc.ClienteId)
                .Select(g => g.OrderByDescending(cc => cc.Id).First())
                .ToDictionary(cc => cc.ClienteId);
        }

        public CuotaCliente GetCuotaClienteById(int id)
        {
            return _contexto.CuotasCliente.FirstOrDefault(cc => cc.Id == id);
        }

        public bool UpdateCuotaCliente(CuotaCliente cuotaCliente)
        {
            _contexto.CuotasCliente.Update(cuotaCliente);
            return _contexto.SaveChanges() > 0;
        }
    }
}
