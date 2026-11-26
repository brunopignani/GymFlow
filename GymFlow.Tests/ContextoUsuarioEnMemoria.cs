using GymFlow.Abstracciones;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    public class ContextoUsuarioEnMemoria : IContextoUsuarioActual
    {
        private readonly HashSet<string> _permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly bool _todos;

        public bool EstaAutenticado { get; private set; }
        public int UsuarioId { get; private set; }
        public int EmpleadoId { get; private set; }
        public string NombreUsuario { get; private set; }

        private ContextoUsuarioEnMemoria(bool autenticado, int usuarioId, int empleadoId, string nombreUsuario, bool todos, IEnumerable<string> permisos)
        {
            EstaAutenticado = autenticado;
            UsuarioId = usuarioId;
            EmpleadoId = empleadoId;
            NombreUsuario = nombreUsuario;
            _todos = todos;
            foreach (string permiso in permisos)
            {
                _permisos.Add(permiso);
            }
        }

        public bool TienePermiso(string codigo)
        {
            return _todos || _permisos.Contains(codigo);
        }

        public static ContextoUsuarioEnMemoria ConTodosLosPermisos(int usuarioId = 1, int empleadoId = 1, string nombreUsuario = "admin")
        {
            return new ContextoUsuarioEnMemoria(true, usuarioId, empleadoId, nombreUsuario, true, Enumerable.Empty<string>());
        }

        public static ContextoUsuarioEnMemoria SinPermisos(int usuarioId = 1, int empleadoId = 1, string nombreUsuario = "usuario")
        {
            return new ContextoUsuarioEnMemoria(true, usuarioId, empleadoId, nombreUsuario, false, Enumerable.Empty<string>());
        }

        public static ContextoUsuarioEnMemoria NoAutenticado()
        {
            // Aunque tuviera permisos, no debe autorizarse sin sesión.
            return new ContextoUsuarioEnMemoria(false, 0, 0, string.Empty, true, Enumerable.Empty<string>());
        }

        public static ContextoUsuarioEnMemoria ConPermisos(params string[] codigos)
        {
            return new ContextoUsuarioEnMemoria(true, 1, 1, "operador", false, codigos);
        }
    }
}