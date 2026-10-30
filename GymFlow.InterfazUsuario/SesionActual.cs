using GymFlow.Dominio;

namespace GymFlow.InterfazUsuario
{
    public static class SesionActual
    {
        public static Empleado EmpleadoActual { get; private set; }

        public static int EmpleadoId
        {
            get { return EmpleadoActual == null ? 0 : EmpleadoActual.Id; }
        }

        public static int UsuarioId
        {
            get { return (EmpleadoActual == null || !EmpleadoActual.UsuarioId.HasValue) ? 0 : EmpleadoActual.UsuarioId.Value; }
        }

        public static void Iniciar(Empleado empleado)
        {
            EmpleadoActual = empleado;
        }

        public static void CerrarSesion()
        {
            EmpleadoActual = null;
        }
    }
}
