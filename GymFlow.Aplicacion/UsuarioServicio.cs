using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class UsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;
        private readonly IPersonaRepositorio _personaRepositorio;

        public UsuarioServicio(
            IUsuarioRepositorio usuarioRepositorio,
            IEmpleadoRepositorio empleadoRepositorio,
            IPersonaRepositorio personaRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
            _personaRepositorio = personaRepositorio;
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
            if (!TieneDatosPersonalesValidos(dni, nombre, apellido, telefono)
                || !ValidacionesDatos.EsNombreUsuarioValido(nombreUsuario)
                || string.IsNullOrEmpty(clave))
            {
                return false;
            }

            if (_personaRepositorio.ExisteDni(dni))
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
            empleado.Telefono = telefono.Trim();
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

        public bool ModificarUsuario(int empleadoId, int dni, string nombre, string apellido, string telefono, string nombreUsuario, string claveNueva = null)
        {
            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            if (!TieneDatosPersonalesValidos(dni, nombre, apellido, telefono)
                || !ValidacionesDatos.EsNombreUsuarioValido(nombreUsuario))
            {
                return false;
            }

            if (_personaRepositorio.ExisteDni(dni, empleadoId))
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
            empleado.Telefono = telefono.Trim();
            empleado.Usuario.Nombre = nombreUsuario.Trim();

            if (!string.IsNullOrEmpty(claveNueva))
            {
                if (string.IsNullOrWhiteSpace(claveNueva))
                {
                    return false;
                }

                empleado.Usuario.Clave = ClaveSegura.Hashear(claveNueva);
            }

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

        private bool TieneDatosPersonalesValidos(int dni, string nombre, string apellido, string telefono)
        {
            return ValidacionesDatos.EsDniValido(dni)
                && ValidacionesDatos.EsNombreValido(nombre)
                && ValidacionesDatos.EsNombreValido(apellido)
                && ValidacionesDatos.EsTelefonoValido(telefono);
        }
    }
}
