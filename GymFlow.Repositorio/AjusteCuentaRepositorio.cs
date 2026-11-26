using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class AjusteCuentaRepositorio : IAjusteCuentaRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public AjusteCuentaRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool AddAjusteCuenta(AjusteCuenta ajuste)
        {
            _contexto.AjustesCuenta.Add(ajuste);
            return _contexto.SaveChanges() > 0;
        }

        public int ObtenerSiguienteNumero()
        {
            return _contexto.Database
                .SqlQueryRaw<int>("SELECT NEXT VALUE FOR dbo.Seq_AjusteCuenta")
                .AsEnumerable()
                .Single();
        }
    }
}
