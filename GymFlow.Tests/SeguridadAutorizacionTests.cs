using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class SeguridadAutorizacionTests
    {
        private static IEnumerable<ContextoUsuarioEnMemoria> ContextosDenegados()
        {
            yield return ContextoUsuarioEnMemoria.SinPermisos();
            yield return ContextoUsuarioEnMemoria.NoAutenticado();
            yield return ContextoUsuarioEnMemoria.ConPermisos("OTRO_PERMISO");
        }

        private static UsuarioServicio CrearUsuarioServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto)
        {
            return new UsuarioServicio(
                new UsuarioRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new PersonaRepositorioEnMemoria(datos),
                new GrupoRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                new AuditoriaRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                contexto);
        }

        private static ClienteServicio CrearClienteServicio(DatosEnMemoria d, ContextoUsuarioEnMemoria c)
        {
            return new ClienteServicio(
                new ClienteRepositorioEnMemoria(d), new PersonaRepositorioEnMemoria(d), new CuotaClienteRepositorioEnMemoria(d),
                new InscripcionRepositorioEnMemoria(d), new CuentaCorrienteRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d),
                new CuotaRepositorioEnMemoria(d), new ComprobanteRepositorioEnMemoria(d), new AuditoriaRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
        }

        private static DebitoMensualServicio CrearDebitoServicio(DatosEnMemoria d, ContextoUsuarioEnMemoria c)
        {
            return new DebitoMensualServicio(
                new ClienteRepositorioEnMemoria(d), new CuentaCorrienteRepositorioEnMemoria(d), new CuotaClienteRepositorioEnMemoria(d),
                new CuotaRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d), new ComprobanteRepositorioEnMemoria(d),
                new TipoMovimientoRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
        }

        private static void SembrarAdministrador(DatosEnMemoria datos, string estadoGrupo = EstadosGrupo.Activo)
        {
            Accion usuarios = new Accion { Id = 1, Nombre = "Usuarios", Codigo = CodigosPermiso.UsuariosGestionar };
            Accion grupos = new Accion { Id = 2, Nombre = "Grupos", Codigo = CodigosPermiso.GruposGestionar };
            datos.Acciones.Add(usuarios);
            datos.Acciones.Add(grupos);
            Grupo admin = new Grupo { Id = 1, Nombre = NombresGrupo.Administrador, Estado = estadoGrupo };
            admin.Acciones.Add(usuarios);
            admin.Acciones.Add(grupos);
            datos.Grupos.Add(admin);
        }

        private static Empleado Registrar(DatosEnMemoria datos, string usuario = "marta", string clave = "Clave123", int dni = 30111222)
        {
            Assert.IsTrue(CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConTodosLosPermisos())
                .RegistrarUsuario(dni, "Marta", "Gomez", "5551234", usuario, clave));
            return datos.Empleados.Single(e => e.Dni == dni);
        }

        [TestMethod]
        public void UsuarioAutenticadoSinPermisos_LoguaPeroLosServiciosLoDenegan()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Registrar(datos);
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.SinPermisos());

            Empleado empleado = servicio.Login("marta", "Clave123");

            Assert.IsNotNull(empleado);
            Assert.AreEqual(0, servicio.ObtenerPermisosEfectivos(empleado.UsuarioId.Value).Count);
            Assert.AreEqual(0, servicio.GetEmpleados().Count);
            Assert.IsFalse(CrearClienteServicio(datos, ContextoUsuarioEnMemoria.SinPermisos()).AddCliente(new Cliente()));
        }

        [TestMethod]
        public void UsuarioNoAutenticado_DenegadoEnServiciosProtegidos()
        {
            foreach (ContextoUsuarioEnMemoria c in ContextosDenegados())
            {
                DatosEnMemoria d = DatosEnMemoria.CrearBase();

                ClienteServicio clientes = CrearClienteServicio(d, c);
                Assert.AreEqual(0, clientes.GetAllClientes().Count);
                Assert.IsNull(clientes.GetClienteByDNI(10111222));
                Assert.IsFalse(clientes.UpdateCliente(d.Clientes[0]));
                Assert.IsFalse(clientes.DeleteCliente(1));
                Assert.AreEqual(ResultadoBajaCliente.SinPermiso, clientes.DeleteClienteConResultado(1));
                Assert.IsFalse(clientes.ValidarAutoingreso(10111222).SocioEncontrado);
                Assert.AreEqual(0, clientes.GetEstadoClientes().Count);

                ActividadServicio actividades = new ActividadServicio(
                    new ActividadRepositorioEnMemoria(d), new InscripcionRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
                Assert.AreEqual(0, actividades.GetAllActividades().Count);
                Assert.IsFalse(actividades.AddActividad(new Actividad { Nombre = "Box", CapacidadMaxima = 5 }));
                Assert.IsFalse(actividades.DeleteActividad(1));

                InscripcionServicio inscripciones = new InscripcionServicio(
                    new CuotaClienteRepositorioEnMemoria(d), new ActividadRepositorioEnMemoria(d), new InscripcionRepositorioEnMemoria(d),
                    new ClienteRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
                Assert.IsFalse(inscripciones.InscribirCliente(1, 1));
                Assert.IsFalse(inscripciones.DesinscribirCliente(1, 1));
                Assert.AreEqual(0, inscripciones.GetInscripcionesByCliente(1).Count);
                Assert.AreEqual(0, inscripciones.DarDeBajaInscripcionesDeMorosos().ClientesAfectados);

                CobroServicio cobro = new CobroServicio(
                    new CuentaCorrienteRepositorioEnMemoria(d), new CuotaClienteRepositorioEnMemoria(d), new EmpleadoRepositorioEnMemoria(d),
                    new ComprobanteRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d), new TipoMovimientoRepositorioEnMemoria(d),
                    new TransaccionEnMemoria(d), new CuotaRepositorioEnMemoria(d), CrearDebitoServicio(d, ContextoUsuarioEnMemoria.ConTodosLosPermisos()), c);
                Assert.IsFalse(cobro.RegistrarCobro(1, 10, 1000m, "Efectivo"));
                Assert.IsNull(cobro.ObtenerResumenCobro(1));

                Assert.IsNull(new CuentaCorrienteServicio(new CuentaCorrienteRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d), c).ConsultarCuentaCorriente(1));

                Assert.AreEqual(0, CrearDebitoServicio(d, c).EmitirDebitosMensuales().Emitidos);
                Assert.AreEqual(0, d.Comprobantes.Count);
            }
        }

        [TestMethod]
        public void NotaCreditoYAjuste_SinPermiso_DenegadosAunqueSeInvoquenDirecto()
        {
            foreach (ContextoUsuarioEnMemoria c in ContextosDenegados())
            {
                DatosEnMemoria d = DatosEnMemoria.CrearBase();
                d.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today, Total = 1000m, CuotaClienteId = 5 });
                int comprobantes = d.Comprobantes.Count;
                int movimientos = d.Movimientos.Count;

                NotaCreditoServicio nc = new NotaCreditoServicio(
                    new CuentaCorrienteRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d), new ComprobanteRepositorioEnMemoria(d),
                    new TipoMovimientoRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
                Assert.IsFalse(nc.EmitirNotaCredito(1, 1, 300m, "Bonificación"));
                Assert.AreEqual(0m, nc.ObtenerImporteAcreditable(1, 1));

                AjusteCuentaServicio ajuste = new AjusteCuentaServicio(
                    new CuentaCorrienteRepositorioEnMemoria(d), new EmpleadoRepositorioEnMemoria(d), new MovimientoRepositorioEnMemoria(d),
                    new AjusteCuentaRepositorioEnMemoria(d), new TipoMovimientoRepositorioEnMemoria(d), new TransaccionEnMemoria(d), c);
                Assert.IsFalse(ajuste.RegistrarAjuste(1, 10, "DEBITO", 100m, "Motivo"));

                Assert.AreEqual(comprobantes, d.Comprobantes.Count);
                Assert.AreEqual(movimientos, d.Movimientos.Count);
            }
        }

        [TestMethod]
        public void UsuarioServicio_AdministracionDenegadaSinPermiso()
        {
            foreach (ContextoUsuarioEnMemoria c in ContextosDenegados())
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                Empleado marta = Registrar(datos);
                UsuarioServicio servicio = CrearUsuarioServicio(datos, c);

                Assert.IsFalse(servicio.RegistrarUsuario(30999888, "Luis", "Paz", "5559999", "luis", "Clave123"));
                Assert.IsFalse(servicio.ModificarUsuario(marta.Id, 30111222, "Marta", "Gomez", "5551234", "otra"));
                Assert.IsFalse(servicio.DarDeBajaUsuario(marta.Id));
                Assert.IsFalse(servicio.ReactivarUsuario(marta.Id));
                Assert.IsNull(servicio.ResetearClave(marta.Id));
                Assert.IsFalse(servicio.AsignarPermisosUsuario(marta.Id, new List<int>(), new List<int>()));
                Assert.AreEqual(0, servicio.GetEmpleados().Count);
                Assert.AreEqual(EstadosUsuario.Activo, datos.Usuarios.Single().Estado);
                Assert.IsTrue(ClaveSegura.Verificar("Clave123", datos.Usuarios.Single().Clave));
            }
        }

        [TestMethod]
        public void CambiarClave_SoloElPropioUsuarioAutenticado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Registrar(datos);
            int usuarioId = datos.Usuarios.Single().Id;

            Assert.IsFalse(CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConTodosLosPermisos(usuarioId + 5)).CambiarClave(usuarioId, "Clave123", "Nueva456"));
            Assert.IsFalse(CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.NoAutenticado()).CambiarClave(usuarioId, "Clave123", "Nueva456"));
            Assert.IsTrue(CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.SinPermisos(usuarioId)).CambiarClave(usuarioId, "Clave123", "Nueva456"));
            Assert.IsTrue(ClaveSegura.Verificar("Nueva456", datos.Usuarios.Single().Clave));
        }

        [TestMethod]
        public void ReactivarUsuario_VuelveAActivarUnUsuarioDadoDeBaja()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Empleado marta = Registrar(datos);
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.UsuariosGestionar));

            Assert.IsTrue(servicio.DarDeBajaUsuario(marta.Id));
            Assert.IsNull(servicio.Login("marta", "Clave123"));
            Assert.IsTrue(servicio.ReactivarUsuario(marta.Id));
            Assert.IsNotNull(servicio.Login("marta", "Clave123"));
            Assert.IsFalse(servicio.ReactivarUsuario(9999));
        }

        [TestMethod]
        public void ResetearClave_ClaveViejaFallaYTemporalFunciona()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Empleado marta = Registrar(datos);
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.UsuariosGestionar));

            string temporal = servicio.ResetearClave(marta.Id);

            Assert.IsNotNull(temporal);
            Assert.IsTrue(temporal.Length >= 10 && temporal.All(char.IsLetterOrDigit));
            Assert.IsNull(servicio.Login("marta", "Clave123"));
            Assert.IsNotNull(servicio.Login("marta", temporal));
            Assert.AreNotEqual(temporal, datos.Usuarios.Single().Clave);
            Assert.IsNull(servicio.ResetearClave(9999));
            Assert.AreNotEqual(temporal, servicio.ResetearClave(marta.Id));
        }

        [TestMethod]
        public void AsignarPermisosUsuario_ReemplazaYValida()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Empleado marta = Registrar(datos);
            SembrarAdministrador(datos);
            datos.Acciones.Add(new Accion { Id = 3, Nombre = "Cobro", Codigo = CodigosPermiso.CobroRegistrar });
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.UsuariosGestionar));
            int usuarioId = datos.Usuarios.Single().Id;

            Assert.IsTrue(servicio.AsignarPermisosUsuario(marta.Id, new List<int> { 1 }, new List<int> { 3 }));
            IReadOnlySet<string> permisos = servicio.ObtenerPermisosEfectivos(usuarioId);
            Assert.IsTrue(permisos.Contains(CodigosPermiso.UsuariosGestionar));
            Assert.IsTrue(permisos.Contains(CodigosPermiso.CobroRegistrar));

            Assert.IsTrue(servicio.AsignarPermisosUsuario(marta.Id, new List<int>(), new List<int> { 3 }));
            permisos = servicio.ObtenerPermisosEfectivos(usuarioId);
            Assert.AreEqual(1, permisos.Count);

            Assert.IsFalse(servicio.AsignarPermisosUsuario(marta.Id, new List<int> { 1, 1 }, new List<int>()));
            Assert.IsFalse(servicio.AsignarPermisosUsuario(marta.Id, new List<int> { 77 }, new List<int>()));
            Assert.IsFalse(servicio.AsignarPermisosUsuario(marta.Id, new List<int>(), new List<int> { 3, 3 }));
            Assert.IsFalse(servicio.AsignarPermisosUsuario(marta.Id, new List<int>(), new List<int> { 88 }));
            Assert.AreEqual(1, servicio.ObtenerPermisosEfectivos(usuarioId).Count);
        }

        [TestMethod]
        public void ObtenerPermisosEfectivos_UsuarioInactivoOInexistente_Vacio()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Registrar(datos);
            SembrarAdministrador(datos);
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.ConTodosLosPermisos());
            Usuario usuario = datos.Usuarios.Single();
            usuario.Grupos.Add(datos.Grupos.Single());

            Assert.AreEqual(2, servicio.ObtenerPermisosEfectivos(usuario.Id).Count);
            Assert.AreEqual(0, servicio.ObtenerPermisosEfectivos(999).Count);
            usuario.Estado = EstadosUsuario.Inactivo;
            Assert.AreEqual(0, servicio.ObtenerPermisosEfectivos(usuario.Id).Count);
        }

        [TestMethod]
        public void ExisteAdministradorActivo_RequiereAmbosPermisosYUsuarioActivo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.NoAutenticado());
            Assert.IsFalse(servicio.ExisteAdministradorActivo());

            Registrar(datos);
            SembrarAdministrador(datos);
            Usuario usuario = datos.Usuarios.Single();
            Assert.IsFalse(servicio.ExisteAdministradorActivo());

            usuario.AccionesDirectas.Add(datos.Acciones.Single(a => a.Codigo == CodigosPermiso.UsuariosGestionar));
            Assert.IsFalse(servicio.ExisteAdministradorActivo());

            usuario.Grupos.Add(datos.Grupos.Single());
            Assert.IsTrue(servicio.ExisteAdministradorActivo());

            usuario.Estado = EstadosUsuario.Inactivo;
            Assert.IsFalse(servicio.ExisteAdministradorActivo());
        }

        [TestMethod]
        public void CrearAdministradorInicial_SinSesion_CreaAdminYRechazaUnSegundo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            SembrarAdministrador(datos);
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.NoAutenticado());

            Assert.IsTrue(servicio.CrearAdministradorInicial(30111222, "Ada", "Root", "5551234", "admin", "Clave123"));

            Usuario admin = datos.Usuarios.Single();
            Assert.AreEqual(EstadosUsuario.Activo, admin.Estado);
            Assert.IsTrue(ClaveSegura.Verificar("Clave123", admin.Clave));
            Assert.IsTrue(admin.Grupos.Any(g => g.Nombre == NombresGrupo.Administrador));
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
            Assert.IsTrue(servicio.ExisteAdministradorActivo());
            Assert.IsNotNull(servicio.Login("admin", "Clave123"));

            Assert.IsFalse(servicio.CrearAdministradorInicial(30999888, "Otro", "Root", "5551234", "admin2", "Clave123"));
            Assert.AreEqual(1, datos.Usuarios.Count);
            Assert.IsTrue(datos.Revertidas >= 1);
        }

        [TestMethod]
        public void CrearAdministradorInicial_SinGrupoAdministradorODatosInvalidos_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.NoAutenticado());
            Assert.IsFalse(servicio.CrearAdministradorInicial(30111222, "Ada", "Root", "5551234", "admin", "Clave123"));

            SembrarAdministrador(datos, EstadosGrupo.Inactivo);
            Assert.IsFalse(servicio.CrearAdministradorInicial(30111222, "Ada", "Root", "5551234", "admin", "Clave123"));

            datos.Grupos.Single().Estado = EstadosGrupo.Activo;
            Assert.IsFalse(servicio.CrearAdministradorInicial(30111222, "Ada", "Root", "5551234", "admin", ""));
            Assert.AreEqual(0, datos.Usuarios.Count);
        }

        [TestMethod]
        public void AsegurarNotaVentaDelPeriodo_NoExigeDebitoMensualEmitir()
        {
            DatosEnMemoria d = DatosEnMemoria.CrearBase();
            DebitoMensualServicio servicio = CrearDebitoServicio(d, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.CobroRegistrar));

            bool resultado = servicio.AsegurarNotaVentaDelPeriodo(
                d.Cuentas.Single(), d.CuotasCliente.Single(), 1000m, d.Tipos.Single(t => t.Nombre == "DEBITO"), DateTime.Today);

            Assert.IsTrue(resultado);
            Assert.AreEqual(1, d.Comprobantes.OfType<NotaVenta>().Count());
        }
    }
}
