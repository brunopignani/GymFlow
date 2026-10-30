using GymFlow.Dominio;

namespace GymFlow.Abstracciones
{
    public interface ICuentaCorrienteRepositorio
    {
        CuentaCorriente GetCuentaCorrienteByCliente(int cliId);
        bool AddCuentaCorriente(CuentaCorriente cuentaCorriente);
    }
}
