using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.InterfazUsuario
{
    public sealed class SesionActual
    {
        private static readonly Lazy<SesionActual> _instancia =
            new Lazy<SesionActual>(() => new SesionActual());

        private readonly object _bloqueo = new object();
        private Empleado _empleadoActual;
        private IReadOnlySet<string> _permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private SesionActual()
        {
        }

        public static SesionActual Instancia => _instancia.Value;

        public Empleado EmpleadoActual
        {
            get
            {
                lock (_bloqueo)
                {
                    return _empleadoActual;
                }
            }
        }

        public IReadOnlySet<string> Permisos
        {
            get
            {
                lock (_bloqueo)
                {
                    return _permisos;
                }
            }
        }

        public event Action PermisosActualizados;

        public int EmpleadoId
        {
            get
            {
                Empleado empleado = EmpleadoActual;
                return empleado == null ? 0 : empleado.Id;
            }
        }

        public int UsuarioId
        {
            get
            {
                Empleado empleado = EmpleadoActual;
                return (empleado == null || !empleado.UsuarioId.HasValue) ? 0 : empleado.UsuarioId.Value;
            }
        }

        public string NombreUsuario
        {
            get
            {
                Empleado empleado = EmpleadoActual;
                return (empleado == null || empleado.Usuario == null) ? string.Empty : (empleado.Usuario.Nombre ?? string.Empty);
            }
        }

        public void Iniciar(Empleado empleado, IReadOnlySet<string> permisos)
        {
            lock (_bloqueo)
            {
                _empleadoActual = empleado;
                _permisos = CrearCopiaPermisos(permisos);
            }
        }

        public void ActualizarPermisos(IReadOnlySet<string> nuevosPermisos)
        {
            lock (_bloqueo)
            {
                _permisos = CrearCopiaPermisos(nuevosPermisos);
            }

            PermisosActualizados?.Invoke();
        }

        public bool TienePermiso(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return false;
            }

            lock (_bloqueo)
            {
                return _empleadoActual != null && _permisos.Contains(codigo);
            }
        }

        public void CerrarSesion()
        {
            lock (_bloqueo)
            {
                _empleadoActual = null;
                _permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
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
