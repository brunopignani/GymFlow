using GymFlow.Abstracciones;

namespace GymFlow.InterfazUsuario
{
    public class ContextoUsuarioActual : IContextoUsuarioActual
    {
        public bool EstaAutenticado => SesionActual.EmpleadoActual != null;

        public int UsuarioId => SesionActual.UsuarioId;

        public int EmpleadoId => SesionActual.EmpleadoId;

        public string NombreUsuario => SesionActual.NombreUsuario;

        public bool TienePermiso(string codigo) => SesionActual.TienePermiso(codigo);
    }
}
