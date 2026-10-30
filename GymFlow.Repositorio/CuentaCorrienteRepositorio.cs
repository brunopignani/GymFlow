using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class CuentaCorrienteRepositorio : ICuentaCorrienteRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public CuentaCorrienteRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public CuentaCorriente GetCuentaCorrienteByCliente(int cliId)
        {
            return _contexto.CuentasCorriente.FirstOrDefault(cc => cc.ClienteId == cliId);
        }

        public bool AddCuentaCorriente(CuentaCorriente cuentaCorriente)
        {
            _contexto.CuentasCorriente.Add(cuentaCorriente);
            return _contexto.SaveChanges() > 0;
        }
    }
}
