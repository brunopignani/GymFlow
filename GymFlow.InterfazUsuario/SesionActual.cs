using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.InterfazUsuario
{
    public static class SesionActual
    {
        public static Empleado EmpleadoActual { get; private set; }

        public static IReadOnlySet<string> Permisos { get; private set; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static event Action PermisosActualizados;

        public static int EmpleadoId
        {
            get { return EmpleadoActual == null ? 0 : EmpleadoActual.Id; }
        }

        public static int UsuarioId
        {
            get { return (EmpleadoActual == null || !EmpleadoActual.UsuarioId.HasValue) ? 0 : EmpleadoActual.UsuarioId.Value; }
        }

        public static string NombreUsuario
        {
            get { return (EmpleadoActual == null || EmpleadoActual.Usuario == null) ? string.Empty : (EmpleadoActual.Usuario.Nombre ?? string.Empty); }
        }

        public static void Iniciar(Empleado empleado, IReadOnlySet<string> permisos)
        {
            EmpleadoActual = empleado;
            Permisos = CrearCopiaPermisos(permisos);
        }

        public static void ActualizarPermisos(IReadOnlySet<string> nuevosPermisos)
        {
            Permisos = CrearCopiaPermisos(nuevosPermisos);
            PermisosActualizados?.Invoke();
        }

        public static bool TienePermiso(string codigo)
        {
            return EmpleadoActual != null
                && !string.IsNullOrWhiteSpace(codigo)
                && Permisos.Contains(codigo);
        }

        public static void CerrarSesion()
        {
            EmpleadoActual = null;
            Permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static IReadOnlySet<string> CrearCopiaPermisos(IReadOnlySet<string> permisos)
        {
            if (permisos == null)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return new HashSet<string>(permisos.Where(p => !string.IsNullOrWhiteSpace(p)), StringComparer.OrdinalIgnoreCase);
        }
    }
}
