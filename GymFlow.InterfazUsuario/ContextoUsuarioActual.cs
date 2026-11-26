using GymFlow.Abstracciones;

namespace GymFlow.InterfazUsuario
{
    public class ContextoUsuarioActual : IContextoUsuarioActual
    {
        public bool EstaAutenticado => SesionActual.Instancia.EmpleadoActual != null;

        public int UsuarioId => SesionActual.Instancia.UsuarioId;

        public int EmpleadoId => SesionActual.Instancia.EmpleadoId;

        public string NombreUsuario => SesionActual.Instancia.NombreUsuario;

        public bool TienePermiso(string codigo) => SesionActual.Instancia.TienePermiso(codigo);
    }
}
