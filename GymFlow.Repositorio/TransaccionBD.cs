using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GymFlow.Repositorio
{
    public class TransaccionBD : ITransaccionBD
    {
        private readonly GymFlowDbContext _contexto;
        private IDbContextTransaction _transaccion;

        public TransaccionBD(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public void IniciarTransaccion()
        {
            _transaccion = _contexto.Database.BeginTransaction();
        }

        public void ConfirmarTransaccion()
        {
            _transaccion.Commit();
            _transaccion.Dispose();
            _transaccion = null;
        }

        public void RevertirTransaccion()
        {
            if (_transaccion != null)
            {
                _transaccion.Rollback();
                _transaccion.Dispose();
                _transaccion = null;
            }

            _contexto.ChangeTracker.Clear();
        }
    }
}
