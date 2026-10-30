namespace GymFlow.Abstracciones
{
    public interface ITransaccionBD
    {
        void IniciarTransaccion();
        void ConfirmarTransaccion();
        void RevertirTransaccion();
    }
}
