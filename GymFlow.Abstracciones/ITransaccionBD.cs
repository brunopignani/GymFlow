using System.Data;

namespace GymFlow.Abstracciones
{
    public interface ITransaccionBD
    {
        void IniciarTransaccion();
        void IniciarTransaccion(IsolationLevel isolationLevel);
        void ConfirmarTransaccion();
        void RevertirTransaccion();
    }
}
