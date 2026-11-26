namespace GymFlow.Abstracciones
{
    public interface IContextoUsuarioActual
    {
        bool EstaAutenticado { get; }
        int UsuarioId { get; }
        int EmpleadoId { get; }
        string NombreUsuario { get; }
        bool TienePermiso(string codigo);
    }
}
