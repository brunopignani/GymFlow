using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;

namespace GymFlow.Aplicacion
{
    public class UsuarioServicio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;
        private readonly IPersonaRepositorio _personaRepositorio;
        private readonly IGrupoRepositorio _grupoRepositorio;
        private readonly IAccionRepositorio _accionRepositorio;
        private readonly IAuditoriaRepositorio _auditoriaRepositorio;
        private readonly ITransaccionBD _transaccion;
        private readonly IContextoUsuarioActual _contextoUsuario;


        public UsuarioServicio(
            IUsuarioRepositorio usuarioRepositorio,
            IEmpleadoRepositorio empleadoRepositorio,
            IPersonaRepositorio personaRepositorio,
            IGrupoRepositorio grupoRepositorio,
            IAccionRepositorio accionRepositorio,
            IAuditoriaRepositorio auditoriaRepositorio,
            ITransaccionBD transaccion,
            IContextoUsuarioActual contextoUsuario)
        {
            _contextoUsuario = contextoUsuario;
            _usuarioRepositorio = usuarioRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
            _personaRepositorio = personaRepositorio;
            _grupoRepositorio = grupoRepositorio;
            _accionRepositorio = accionRepositorio;
            _auditoriaRepositorio = auditoriaRepositorio;
            _transaccion = transaccion;
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

            Empleado empleado = _empleadoRepositorio.GetEmpleadoByUsuarioId(usuario.Id);

            if (empleado == null)
            {
                return null;
            }

            RegistrarAuditoriaLogin(usuario, empleado);

            return empleado;
        }

        public bool RegistrarLogout()
        {
            if (_contextoUsuario == null || !_contextoUsuario.EstaAutenticado || _contextoUsuario.UsuarioId <= 0)
            {
                return false;
            }

            AuditoriaSesion registro = new AuditoriaSesion
            {
                FechaHora = DateTime.Now,
                TipoEvento = EventosAuditoriaSesion.Logout,
                UsuarioId = _contextoUsuario.UsuarioId,
                EmpleadoId = _contextoUsuario.EmpleadoId > 0 ? _contextoUsuario.EmpleadoId : (int?)null,
                NombreUsuario = string.IsNullOrWhiteSpace(_contextoUsuario.NombreUsuario) ? "Desconocido" : _contextoUsuario.NombreUsuario
            };

            return _auditoriaRepositorio.RegistrarSesion(registro);
        }

        private void RegistrarAuditoriaLogin(Usuario usuario, Empleado empleado)
        {
            AuditoriaSesion registro = new AuditoriaSesion
            {
                FechaHora = DateTime.Now,
                TipoEvento = EventosAuditoriaSesion.Login,
                UsuarioId = usuario.Id,
                EmpleadoId = empleado.Id,
                NombreUsuario = usuario.Nombre
            };

            bool registrado;
            try
            {
                registrado = _auditoriaRepositorio.RegistrarSesion(registro);
            }
            catch (Exception)
            {
                throw new AuditoriaLoginException();
            }

            if (!registrado)
            {
                throw new AuditoriaLoginException();
            }
        }

        public bool RegistrarUsuario(int dni, string nombre, string apellido, string telefono, string nombreUsuario, string clave)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return false;
            }

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
            if (_contextoUsuario == null || !_contextoUsuario.EstaAutenticado || _contextoUsuario.UsuarioId != usuarioId)
            {
                return false;
            }

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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return new List<Empleado>();
            }

            return _empleadoRepositorio.GetAllEmpleados();
        }

        public List<Grupo> GetGruposDisponibles()
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return new List<Grupo>();
            }

            return _grupoRepositorio.GetAllGruposConRelaciones();
        }

        public List<Accion> GetAccionesDisponibles()
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return new List<Accion>();
            }

            return _accionRepositorio.GetAllAccionesConFormularioYModulo();
        }

        public bool ModificarUsuario(int empleadoId, int dni, string nombre, string apellido, string telefono, string nombreUsuario, string claveNueva = null)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return false;
            }

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
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return false;
            }

            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            empleado.Usuario.Estado = EstadosUsuario.Inactivo;

            return _usuarioRepositorio.UpdateUsuario(empleado.Usuario);
        }

        public IReadOnlySet<string> ObtenerPermisosEfectivos(int usuarioId)
        {
            Usuario usuario = CargarUsuarioConPermisos(usuarioId);

            if (usuario == null || usuario.Estado != EstadosUsuario.Activo)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            return usuario.ObtenerPermisosEfectivos();
        }

        public bool ExisteAdministradorActivo()
        {
            // Carga todos los grupos con relaciones para que el grafo de subgrupos quede completo.
            _grupoRepositorio.GetAllGruposConRelaciones();

            foreach (Usuario candidato in _usuarioRepositorio.GetAllUsuarios())
            {
                if (candidato.Estado != EstadosUsuario.Activo)
                {
                    continue;
                }

                Usuario usuario = _usuarioRepositorio.GetUsuarioConPermisosById(candidato.Id);

                if (usuario != null
                    && usuario.TienePermiso(CodigosPermiso.UsuariosGestionar)
                    && usuario.TienePermiso(CodigosPermiso.GruposGestionar))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CrearAdministradorInicial(int dni, string nombre, string apellido, string telefono, string nombreUsuario, string clave)
        {
            if (!TieneDatosPersonalesValidos(dni, nombre, apellido, telefono)
                || !ValidacionesDatos.EsNombreUsuarioValido(nombreUsuario)
                || string.IsNullOrEmpty(clave))
            {
                return false;
            }

            _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

            try
            {
                if (ExisteAdministradorActivo())
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Grupo grupoAdministrador = _grupoRepositorio.GetGrupoByNombre(NombresGrupo.Administrador);

                if (grupoAdministrador == null
                    || grupoAdministrador.Estado != EstadosGrupo.Activo
                    || _personaRepositorio.ExisteDni(dni)
                    || _usuarioRepositorio.GetUsuarioByNombre(nombreUsuario.Trim()) != null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Usuario usuario = new Usuario();
                usuario.Nombre = nombreUsuario.Trim();
                usuario.Clave = ClaveSegura.Hashear(clave);
                usuario.Estado = EstadosUsuario.Activo;
                usuario.Grupos.Add(grupoAdministrador);

                Empleado empleado = new Empleado();
                empleado.Dni = dni;
                empleado.Nombre = nombre.Trim();
                empleado.Apellido = apellido.Trim();
                empleado.Telefono = telefono.Trim();
                empleado.Usuario = usuario;

                if (!_empleadoRepositorio.AddEmpleado(empleado))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                _transaccion.ConfirmarTransaccion();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                _transaccion.RevertirTransaccion();
                return false;
            }
        }

        public bool ReactivarUsuario(int empleadoId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return false;
            }

            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            empleado.Usuario.Estado = EstadosUsuario.Activo;

            return _usuarioRepositorio.UpdateUsuario(empleado.Usuario);
        }

        public string ResetearClave(int empleadoId)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return null;
            }

            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return null;
            }

            string claveTemporal = GenerarClaveTemporal(12);
            empleado.Usuario.Clave = ClaveSegura.Hashear(claveTemporal);

            return _usuarioRepositorio.UpdateUsuario(empleado.Usuario) ? claveTemporal : null;
        }

        public bool AsignarPermisosUsuario(int empleadoId, List<int> grupoIds, List<int> accionDirectaIds)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.UsuariosGestionar))
            {
                return false;
            }

            grupoIds = grupoIds ?? new List<int>();
            accionDirectaIds = accionDirectaIds ?? new List<int>();

            if (grupoIds.Distinct().Count() != grupoIds.Count
                || accionDirectaIds.Distinct().Count() != accionDirectaIds.Count)
            {
                return false;
            }

            Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);

            if (empleado == null || empleado.Usuario == null)
            {
                return false;
            }

            List<Grupo> todosLosGrupos = _grupoRepositorio.GetAllGruposConRelaciones();
            List<Accion> todasLasAcciones = _accionRepositorio.GetAllAccionesConFormularioYModulo();

            List<Grupo> grupos = grupoIds
                .Select(id => todosLosGrupos.FirstOrDefault(g => g.Id == id))
                .ToList();
            List<Accion> acciones = accionDirectaIds
                .Select(id => todasLasAcciones.FirstOrDefault(a => a.Id == id))
                .ToList();

            if (grupos.Any(g => g == null) || acciones.Any(a => a == null))
            {
                return false;
            }

            Usuario usuario = _usuarioRepositorio.GetUsuarioConPermisosById(empleado.Usuario.Id);

            if (usuario == null)
            {
                return false;
            }

            usuario.Grupos.Clear();
            foreach (Grupo grupo in grupos)
            {
                usuario.Grupos.Add(grupo);
            }

            usuario.AccionesDirectas.Clear();
            foreach (Accion accion in acciones)
            {
                usuario.AccionesDirectas.Add(accion);
            }

            return _usuarioRepositorio.UpdateUsuario(usuario);
        }

        private Usuario CargarUsuarioConPermisos(int usuarioId)
        {
            _grupoRepositorio.GetAllGruposConRelaciones();
            return _usuarioRepositorio.GetUsuarioConPermisosById(usuarioId);
        }

        private static string GenerarClaveTemporal(int longitud)
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            char[] resultado = new char[longitud];

            for (int i = 0; i < longitud; i++)
            {
                resultado[i] = caracteres[RandomNumberGenerator.GetInt32(caracteres.Length)];
            }

            return new string(resultado);
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
