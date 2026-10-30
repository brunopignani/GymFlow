using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class UsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;

        public UsuarioServicio(IUsuarioRepositorio usuarioRepositorio, IEmpleadoRepositorio empleadoRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
        }

        public Empleado Login(string nombreUsuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(clave))
            {
                return null;
            }

            Usuario usuario = _usuarioRepositorio.GetUsuarioByNombre(nombreUsuario.Trim());

            if (usuario == null || usuario.Estado != EstadosUsuario.Activo)
            {
                return null;
            }

            if (!ClaveSegura.Verificar(clave, usuario.Clave))
            {
                return null;
            }

            return _empleadoRepositorio.GetEmpleadoByUsuarioId(usuario.Id);
        }

        public bool RegistrarUsuario(int dni, string nombre, string apellido, string telefono, string nombreUsuario, string clave)
        {
            if (!TieneDatosPersonalesValidos(dni, nombre, apellido) || string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(clave))
            {
                return false;
            }

            if (_empleadoRepositorio.GetEmpleadoByDni(dni) != null)
            {
                return false;
            }

            if (_usuarioRepositorio.GetUsuarioByNombre(nombreUsuario.Trim()) != null)
            {
                return false;
            }

            Usuario usuario = new Usuario();
            usuario.Nombre = nombreUsuario.Trim();
            usuario.Clave = ClaveSegura.Hashear(clave);
            usuario.Estado = EstadosUsuario.Activo;

            Empleado empleado = new Empleado();
            empleado.Dni = dni;
            empleado.Nombre = nombre.Trim();
            empleado.Apellido = apellido.Trim();
            empleado.Telefono = telefono;
            empleado.Usuario = usuario;

            return _empleadoRepositorio.AddEmpleado(empleado);
        }

        public bool CambiarClave(int usuarioId, string claveActual, string claveNueva)
        {
            Usuario usuario = _usuarioRepositorio.GetUsuarioById(usuarioId);

            if (usuario == null || string.IsNullOrEmpty(claveNueva) || claveNueva == claveActual)
            {
                return false;
            }

            if (!ClaveSegura.Verificar(claveActual, usuario.Clave))
            {
                return false;
            }

            usuario.Clave = ClaveSegura.Hashear(claveNueva);

            return _usuarioRepositorio.UpdateUsuario(usuario);
        }

        public List<Empleado> GetEmpleados()
        {
            return _empleadoRepositorio.GetAllEmpleados();
        }

        public bool ModificarUsuario(int empleadoId, int dni, string nombre, string apellido, string telefono, string nombreUsuario)
        {
            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            if (!TieneDatosPersonalesValidos(dni, nombre, apellido) || string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return false;
            }

            Empleado conMismoDni = _empleadoRepositorio.GetEmpleadoByDni(dni);

            if (conMismoDni != null && conMismoDni.Id != empleadoId)
            {
                return false;
            }

            Usuario conMismoNombre = _usuarioRepositorio.GetUsuarioByNombre(nombreUsuario.Trim());

            if (conMismoNombre != null && conMismoNombre.Id != empleado.Usuario.Id)
            {
                return false;
            }

            empleado.Dni = dni;
            empleado.Nombre = nombre.Trim();
            empleado.Apellido = apellido.Trim();
            empleado.Telefono = telefono;
            empleado.Usuario.Nombre = nombreUsuario.Trim();

            return _empleadoRepositorio.UpdateEmpleado(empleado);
        }

        public bool DarDeBajaUsuario(int empleadoId)
        {
            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            empleado.Usuario.Estado = EstadosUsuario.Inactivo;

            return _usuarioRepositorio.UpdateUsuario(empleado.Usuario);
        }

        private bool TieneDatosPersonalesValidos(int dni, string nombre, string apellido)
        {
            return dni > 0 && !string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(apellido);
        }
    }
}
