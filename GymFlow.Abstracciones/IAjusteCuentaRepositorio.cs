using GymFlow.Dominio;

namespace GymFlow.Abstracciones
{
    public interface IAjusteCuentaRepositorio
    {
        bool AddAjusteCuenta(AjusteCuenta ajuste);
        int ObtenerSiguienteNumero();
    }
}
