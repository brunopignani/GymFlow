namespace GymFlow.Abstracciones
{
    public interface IRespaldoRepositorio
    {
        string ObtenerNombreBaseActual();
        string ObtenerRutaPorDefecto();
        void GenerarRespaldo(string rutaArchivo);
        void RestaurarRespaldo(string rutaArchivo);
    }
}
