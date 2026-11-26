using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GymFlow.Tests
{
    [TestClass]
    public class AuditoriaTests
    {
        private static ClienteServicio CrearClienteServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto = null)
        {
            return new ClienteServicio(
                new ClienteRepositorioEnMemoria(datos),
                new PersonaRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new AuditoriaRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                contexto ?? ContextoUsuarioEnMemoria.ConTodosLosPermisos(5, 10, "operador.admin"));
        }

        private static UsuarioServicio CrearUsuarioServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto = null)
        {
            return new UsuarioServicio(
                new UsuarioRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new PersonaRepositorioEnMemoria(datos),
                new GrupoRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                new AuditoriaRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                contexto ?? ContextoUsuarioEnMemoria.ConTodosLosPermisos(5, 10, "operador.admin"));
        }

        private static AuditoriaServicio CrearAuditoriaServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto)
        {
            return new AuditoriaServicio(new AuditoriaRepositorioEnMemoria(datos), contexto);
        }

        [TestMethod]
        public void AddCliente_RegistraAuditoriaAltaConValoresNuevosYAntesNulo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearClienteServicio(datos);
            Cliente nuevo = new Cliente { Dni = 20333444, Nombre = "Lucía", Apellido = "Gómez", Telefono = "1144556677" };

            Assert.IsTrue(servicio.AddCliente(nuevo));

            AuditoriaCliente registro = datos.AuditoriasCliente.Single();
            Assert.AreEqual(OperacionesAuditoria.Alta, registro.Operacion);
            Assert.AreEqual(nuevo.Id, registro.ClienteId);
            Assert.AreEqual(20333444, registro.DniCliente);
            Assert.AreEqual(5, registro.UsuarioId);
            Assert.AreEqual(10, registro.EmpleadoId);
            Assert.AreEqual("operador.admin", registro.NombreUsuario);
            Assert.IsNull(registro.ValoresAnteriores);
            Assert.IsNotNull(registro.ValoresNuevos);

            using (JsonDocument doc = JsonDocument.Parse(registro.ValoresNuevos))
            {
                Assert.AreEqual(20333444, doc.RootElement.GetProperty("Dni").GetInt32());
                Assert.AreEqual("Lucía", doc.RootElement.GetProperty("Nombre").GetString());
                Assert.AreEqual("Gómez", doc.RootElement.GetProperty("Apellido").GetString());
                Assert.AreEqual("1144556677", doc.RootElement.GetProperty("Telefono").GetString());
            }
        }

        [TestMethod]
        public void UpdateCliente_ConCambios_RegistraAuditoriaModificacionConAntesYDespues()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes[0].Telefono = "1122334455";
            ClienteServicio servicio = CrearClienteServicio(datos);
            Cliente modificado = new Cliente { Id = 1, Dni = 10111222, Nombre = "Ana María", Apellido = "Paz", Telefono = "1199887766" };

            Assert.IsTrue(servicio.UpdateCliente(modificado));

            AuditoriaCliente registro = datos.AuditoriasCliente.Single();
            Assert.AreEqual(OperacionesAuditoria.Modificacion, registro.Operacion);
            Assert.AreEqual(1, registro.ClienteId);
            Assert.AreEqual(10111222, registro.DniCliente);
            Assert.IsNotNull(registro.ValoresAnteriores);
            Assert.IsNotNull(registro.ValoresNuevos);

            using (JsonDocument antes = JsonDocument.Parse(registro.ValoresAnteriores))
            using (JsonDocument despues = JsonDocument.Parse(registro.ValoresNuevos))
            {
                Assert.AreEqual("Ana", antes.RootElement.GetProperty("Nombre").GetString());
                Assert.AreEqual("1122334455", antes.RootElement.GetProperty("Telefono").GetString());
                Assert.AreEqual("Ana María", despues.RootElement.GetProperty("Nombre").GetString());
                Assert.AreEqual("1199887766", despues.RootElement.GetProperty("Telefono").GetString());
            }
        }

        [TestMethod]
        public void UpdateCliente_SinCambiosEfectivos_NoRegistraAuditoria()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes[0].Telefono = "1122334455";
            ClienteServicio servicio = CrearClienteServicio(datos);
            Cliente sinCambios = new Cliente { Id = 1, Dni = 10111222, Nombre = "Ana", Apellido = "Paz", Telefono = "1122334455" };

            Assert.IsTrue(servicio.UpdateCliente(sinCambios));
            Assert.AreEqual(0, datos.AuditoriasCliente.Count);
        }

        [TestMethod]
        public void DeleteCliente_BajaFisica_ConservaTrazaConValoresAnterioresYDespuesNulo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes[0].Telefono = "1122334455";
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            ClienteServicio servicio = CrearClienteServicio(datos);

            Assert.AreEqual(ResultadoBajaCliente.Eliminado, servicio.DeleteClienteConResultado(1));
            Assert.IsFalse(datos.Clientes.Any(c => c.Id == 1));

            AuditoriaCliente registro = datos.AuditoriasCliente.Single();
            Assert.AreEqual(OperacionesAuditoria.Baja, registro.Operacion);
            Assert.AreEqual(1, registro.ClienteId);
            Assert.AreEqual(10111222, registro.DniCliente);
            Assert.IsNotNull(registro.ValoresAnteriores);
            Assert.IsNull(registro.ValoresNuevos);

            using (JsonDocument antes = JsonDocument.Parse(registro.ValoresAnteriores))
            {
                Assert.AreEqual(10111222, antes.RootElement.GetProperty("Dni").GetInt32());
                Assert.AreEqual("Ana", antes.RootElement.GetProperty("Nombre").GetString());
                Assert.AreEqual("Paz", antes.RootElement.GetProperty("Apellido").GetString());
                Assert.AreEqual("1122334455", antes.RootElement.GetProperty("Telefono").GetString());
            }
        }

        [TestMethod]
        public void AddUpdateDeleteCliente_SiFallaAuditoria_RevierteTransaccion()
        {
            // Alta con falla de auditoría
            DatosEnMemoria datosAlta = DatosEnMemoria.CrearBase();
            datosAlta.FallarAlRegistrarAuditoriaCliente = true;
            ClienteServicio servAlta = CrearClienteServicio(datosAlta);
            Assert.ThrowsException<InvalidOperationException>(() =>
                servAlta.AddCliente(new Cliente { Dni = 20555666, Nombre = "Juan", Apellido = "Perez", Telefono = "1155667788" }));
            Assert.IsFalse(datosAlta.Clientes.Any(c => c.Dni == 20555666));
            Assert.AreEqual(1, datosAlta.Revertidas);

            // Modificación con rechazo (false) de auditoría
            DatosEnMemoria datosMod = DatosEnMemoria.CrearBase();
            datosMod.Clientes[0].Telefono = "1122334455";
            datosMod.RechazarRegistrarAuditoriaCliente = true;
            ClienteServicio servMod = CrearClienteServicio(datosMod);
            Assert.ThrowsException<InvalidOperationException>(() =>
                servMod.UpdateCliente(new Cliente { Id = 1, Dni = 10111222, Nombre = "Modificado", Apellido = "Paz", Telefono = "1122334455" }));
            Assert.AreEqual("Ana", datosMod.Clientes[0].Nombre);
            Assert.AreEqual(1, datosMod.Revertidas);

            // Baja con falla de auditoría
            DatosEnMemoria datosBaja = DatosEnMemoria.CrearBase();
            datosBaja.Clientes[0].Telefono = "1122334455";
            datosBaja.CuotasCliente[0].FechaVencimiento = null;
            datosBaja.CuotasCliente[0].Estado = null;
            datosBaja.FallarAlRegistrarAuditoriaCliente = true;
            ClienteServicio servBaja = CrearClienteServicio(datosBaja);
            Assert.ThrowsException<InvalidOperationException>(() => servBaja.DeleteClienteConResultado(1));
            Assert.IsTrue(datosBaja.Clientes.Any(c => c.Id == 1));
            Assert.IsTrue(datosBaja.Cuentas.Any(cc => cc.ClienteId == 1));
            Assert.AreEqual(1, datosBaja.Revertidas);
        }

        [TestMethod]
        public void Login_Exitoso_RegistraEventoSinClaveHashNiSalt()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearUsuarioServicio(datos);
            Assert.IsTrue(servicio.RegistrarUsuario(30111222, "Marta", "Gomez", "5551234", "marta", "ClaveSecreta99!"));

            Empleado empleado = servicio.Login("marta", "ClaveSecreta99!");

            Assert.IsNotNull(empleado);
            AuditoriaSesion registro = datos.AuditoriasSesion.Single();
            Assert.AreEqual(EventosAuditoriaSesion.Login, registro.TipoEvento);
            Assert.AreEqual(empleado.UsuarioId.Value, registro.UsuarioId);
            Assert.AreEqual(empleado.Id, registro.EmpleadoId);
            Assert.AreEqual("marta", registro.NombreUsuario);

            string serializado = JsonSerializer.Serialize(registro);
            Assert.IsFalse(serializado.Contains("ClaveSecreta99!"));
            Assert.IsFalse(serializado.Contains(datos.Usuarios.Single().Clave));
        }

        [TestMethod]
        public void Login_FallaAlRegistrarAuditoria_LanzaExcepcionControladaSinDetallesInternos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearUsuarioServicio(datos);
            Assert.IsTrue(servicio.RegistrarUsuario(30111222, "Marta", "Gomez", "5551234", "marta", "Clave123"));

            datos.FallarAlRegistrarAuditoriaSesion = true;
            AuditoriaLoginException ex1 = Assert.ThrowsException<AuditoriaLoginException>(() => servicio.Login("marta", "Clave123"));
            Assert.IsNull(ex1.InnerException);
            Assert.IsFalse(ex1.Message.Contains("simulada"));

            datos.FallarAlRegistrarAuditoriaSesion = false;
            datos.RechazarRegistrarAuditoriaSesion = true;
            AuditoriaLoginException ex2 = Assert.ThrowsException<AuditoriaLoginException>(() => servicio.Login("marta", "Clave123"));
            Assert.IsNull(ex2.InnerException);
            Assert.AreEqual(0, datos.AuditoriasSesion.Count);
        }

        [TestMethod]
        public void RegistrarLogout_AutenticadoRegistraYSinAutenticarRechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicioAutenticado = CrearUsuarioServicio(
                datos,
                ContextoUsuarioEnMemoria.ConTodosLosPermisos(7, 15, "recep1"));

            Assert.IsTrue(servicioAutenticado.RegistrarLogout());
            AuditoriaSesion registro = datos.AuditoriasSesion.Single();
            Assert.AreEqual(EventosAuditoriaSesion.Logout, registro.TipoEvento);
            Assert.AreEqual(7, registro.UsuarioId);
            Assert.AreEqual(15, registro.EmpleadoId);
            Assert.AreEqual("recep1", registro.NombreUsuario);

            UsuarioServicio servicioNoAutenticado = CrearUsuarioServicio(datos, ContextoUsuarioEnMemoria.NoAutenticado());
            Assert.IsFalse(servicioNoAutenticado.RegistrarLogout());
            Assert.AreEqual(1, datos.AuditoriasSesion.Count);
        }

        [TestMethod]
        public void AuditoriaServicio_RequierePermisoAuditoriaVerYAplicaFiltros()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            DateTime baseFecha = new DateTime(2026, 11, 20, 10, 0, 0);
            datos.AuditoriasSesion.Add(new AuditoriaSesion { Id = 1, FechaHora = baseFecha, TipoEvento = EventosAuditoriaSesion.Login, UsuarioId = 1, NombreUsuario = "admin" });
            datos.AuditoriasSesion.Add(new AuditoriaSesion { Id = 2, FechaHora = baseFecha.AddHours(2), TipoEvento = EventosAuditoriaSesion.Logout, UsuarioId = 1, NombreUsuario = "admin" });
            datos.AuditoriasCliente.Add(new AuditoriaCliente { Id = 1, FechaHora = baseFecha, Operacion = OperacionesAuditoria.Alta, ClienteId = 1, DniCliente = 10111222, UsuarioId = 1, NombreUsuario = "admin" });
            datos.AuditoriasCliente.Add(new AuditoriaCliente { Id = 2, FechaHora = baseFecha.AddDays(1), Operacion = OperacionesAuditoria.Baja, ClienteId = 2, DniCliente = 20111222, UsuarioId = 2, NombreUsuario = "recep" });

            // Sin permiso o no autenticado: siempre vacío
            foreach (ContextoUsuarioEnMemoria denegado in new[]
            {
                ContextoUsuarioEnMemoria.NoAutenticado(),
                ContextoUsuarioEnMemoria.SinPermisos(),
                ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.ClientesGestionar)
            })
            {
                AuditoriaServicio servDenegado = CrearAuditoriaServicio(datos, denegado);
                Assert.AreEqual(0, servDenegado.ConsultarSesiones(null, null, null, null).Count);
                Assert.AreEqual(0, servDenegado.ConsultarClientes(null, null, null, null, null).Count);
            }

            // Con permiso AUDITORIA_VER
            AuditoriaServicio servPermitido = CrearAuditoriaServicio(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.AuditoriaVer));
            Assert.AreEqual(1, servPermitido.ConsultarSesiones(null, null, "admin", EventosAuditoriaSesion.Logout).Count);
            Assert.AreEqual(1, servPermitido.ConsultarClientes(null, null, "recep", OperacionesAuditoria.Baja, 20111222).Count);
        }

        [TestMethod]
        public void InicializadorDatos_VinculaAuditoriaVerAlAdminExistenteUnaSolaVezSinPisarCambiosManuales()
        {
            DatosEnMemoria datos = new DatosEnMemoria();
            InicializadorDatosServicio inicializador = new InicializadorDatosServicio(
                new TipoMovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                new GrupoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));

            // Simular base preexistente sin la acción AUDITORIA_VER
            inicializador.Inicializar();
            Grupo admin = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Administrador);
            Accion accionAuditoria = datos.Acciones.Single(a => a.Codigo == CodigosPermiso.AuditoriaVer);
            admin.Acciones.Remove(accionAuditoria);
            datos.Acciones.Remove(accionAuditoria);

            // Primera ejecución tras incorporar T06: crea la acción y la vincula al Administrador existente
            inicializador.Inicializar();
            Assert.IsTrue(datos.Acciones.Any(a => a.Codigo == CodigosPermiso.AuditoriaVer));
            Assert.IsTrue(admin.Acciones.Any(a => a.Codigo == CodigosPermiso.AuditoriaVer));

            // Un administrador quita manualmente AUDITORIA_VER del grupo Administrador
            Accion nuevaAccion = admin.Acciones.Single(a => a.Codigo == CodigosPermiso.AuditoriaVer);
            admin.Acciones.Remove(nuevaAccion);

            // Arranque posterior: la acción ya existe, no debe volver a agregarse al grupo
            inicializador.Inicializar();
            Assert.IsFalse(admin.Acciones.Any(a => a.Codigo == CodigosPermiso.AuditoriaVer));
        }
    }
}
