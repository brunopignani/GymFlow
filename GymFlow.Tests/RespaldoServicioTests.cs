using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class RespaldoServicioTests
    {
        private static RespaldoServicio CrearServicio(
            RespaldoRepositorioEnMemoria repositorio,
            ContextoUsuarioEnMemoria contexto)
        {
            return new RespaldoServicio(repositorio, contexto);
        }

        [TestMethod]
        public void Operaciones_SinAutenticacion_LanzanUnauthorizedAccessException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.NoAutenticado());

            Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.ObtenerNombreBaseActual());
            Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.ObtenerRutaPorDefecto());
            Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.GenerarRespaldo(@"C:\Backups\gym.bak"));
            Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.RestaurarRespaldo(@"C:\Backups\gym.bak"));

            Assert.AreEqual(0, repo.CantidadRespaldos);
            Assert.AreEqual(0, repo.CantidadRestauraciones);
        }

        [TestMethod]
        public void Operaciones_SinPermisoRespaldoGestionar_LanzanUnauthorizedAccessException()
        {
            ContextoUsuarioEnMemoria[] contextosDenegados =
            {
                ContextoUsuarioEnMemoria.SinPermisos(),
                ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.AuditoriaVer, CodigosPermiso.UsuariosGestionar)
            };

            foreach (ContextoUsuarioEnMemoria contexto in contextosDenegados)
            {
                RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
                RespaldoServicio servicio = CrearServicio(repo, contexto);

                Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.ObtenerNombreBaseActual());
                Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.ObtenerRutaPorDefecto());
                Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.GenerarRespaldo(@"C:\Backups\gym.bak"));
                Assert.ThrowsException<UnauthorizedAccessException>(() => servicio.RestaurarRespaldo(@"C:\Backups\gym.bak"));

                Assert.AreEqual(0, repo.CantidadRespaldos);
                Assert.AreEqual(0, repo.CantidadRestauraciones);
            }
        }

        [TestMethod]
        public void GenerarRespaldo_RutaNulaOVacia_LanzaArgumentException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo(null));
            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo("   "));
            Assert.AreEqual(0, repo.CantidadRespaldos);
        }

        [TestMethod]
        public void GenerarRespaldo_ExtensionDistintaABak_LanzaArgumentException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo(@"C:\Backups\gym.sql"));
            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo(@"C:\Backups\gym.zip"));
            Assert.ThrowsException<ArgumentException>(() => servicio.GenerarRespaldo(@"C:\Backups\gym"));
            Assert.AreEqual(0, repo.CantidadRespaldos);
        }

        [TestMethod]
        public void RestaurarRespaldo_RutaNulaOVacia_LanzaArgumentException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo(null));
            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo("   "));
            Assert.AreEqual(0, repo.CantidadRestauraciones);
        }

        [TestMethod]
        public void RestaurarRespaldo_ExtensionDistintaABak_LanzaArgumentException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo(@"C:\Backups\gym.txt"));
            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo(@"C:\Backups\gym.bak.zip"));
            Assert.ThrowsException<ArgumentException>(() => servicio.RestaurarRespaldo(@"C:\Backups\gym"));
            Assert.AreEqual(0, repo.CantidadRestauraciones);
        }

        [TestMethod]
        public void RestaurarRespaldo_ArchivoInexistente_LanzaFileNotFoundException()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria();
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            string rutaInexistente = Path.Combine(Path.GetTempPath(), $"gymflow_inexistente_{Guid.NewGuid():N}.bak");

            Assert.ThrowsException<FileNotFoundException>(() => servicio.RestaurarRespaldo(rutaInexistente));
            Assert.AreEqual(0, repo.CantidadRestauraciones);
        }

        [TestMethod]
        public void Operaciones_ConAutorizacionYRutaValida_DeleganAlRepositorio()
        {
            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria
            {
                NombreBase = "GymFlowDB_Pruebas",
                RutaPorDefecto = @"C:\SQLBackups"
            };
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            Assert.AreEqual("GymFlowDB_Pruebas", servicio.ObtenerNombreBaseActual());
            Assert.AreEqual(@"C:\SQLBackups", servicio.ObtenerRutaPorDefecto());

            string rutaBackup = @"C:\SQLBackups\GymFlowDB_Pruebas_20261126.BAK";
            servicio.GenerarRespaldo(rutaBackup);
            Assert.AreEqual(1, repo.CantidadRespaldos);
            Assert.AreEqual(rutaBackup, repo.UltimaRutaRespaldo);

            string archivoTemporal = Path.Combine(Path.GetTempPath(), $"gymflow_test_{Guid.NewGuid():N}.bak");
            try
            {
                File.WriteAllBytes(archivoTemporal, new byte[] { 1, 2, 3 });

                servicio.RestaurarRespaldo(archivoTemporal);
                Assert.AreEqual(1, repo.CantidadRestauraciones);
                Assert.AreEqual(archivoTemporal, repo.UltimaRutaRestauracion);
            }
            finally
            {
                if (File.Exists(archivoTemporal))
                {
                    File.Delete(archivoTemporal);
                }
            }
        }

        [TestMethod]
        public void Operaciones_CuandoFallaRepositorio_PropaganExcepcionOriginal()
        {
            InvalidOperationException errorBackup = new InvalidOperationException("Permiso denegado en carpeta destino.");
            InvalidOperationException errorOriginalRestore = new InvalidOperationException("Fallo interno en RESTORE.");
            InvalidOperationException errorRestoreSingleUser = new InvalidOperationException(
                "La base de datos podría haber quedado en modo SINGLE_USER.",
                errorOriginalRestore);

            RespaldoRepositorioEnMemoria repo = new RespaldoRepositorioEnMemoria
            {
                ExcepcionEnRespaldo = errorBackup,
                ExcepcionEnRestauracion = errorRestoreSingleUser
            };
            RespaldoServicio servicio = CrearServicio(repo, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.RespaldoGestionar));

            InvalidOperationException exBackup = Assert.ThrowsException<InvalidOperationException>(
                () => servicio.GenerarRespaldo(@"C:\Backups\gym.bak"));
            Assert.AreSame(errorBackup, exBackup);

            string archivoTemporal = Path.Combine(Path.GetTempPath(), $"gymflow_err_{Guid.NewGuid():N}.bak");
            try
            {
                File.WriteAllBytes(archivoTemporal, new byte[] { 1 });

                InvalidOperationException exRestore = Assert.ThrowsException<InvalidOperationException>(
                    () => servicio.RestaurarRespaldo(archivoTemporal));
                Assert.AreSame(errorRestoreSingleUser, exRestore);
                Assert.AreSame(errorOriginalRestore, exRestore.InnerException);
            }
            finally
            {
                if (File.Exists(archivoTemporal))
                {
                    File.Delete(archivoTemporal);
                }
            }
        }

        [TestMethod]
        public void InicializadorDatos_SeedRespaldoGestionar_EsIdempotenteYNoPisaCambiosManuales()
        {
            DatosEnMemoria datos = new DatosEnMemoria();
            InicializadorDatosServicio inicializador = new InicializadorDatosServicio(
                new TipoMovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                new GrupoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));

            // Simular base preexistente sin la acción RESPALDO_GESTIONAR
            inicializador.Inicializar();
            Grupo admin = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Administrador);
            Grupo recepcionista = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Recepcionista);

            Accion accionRespaldo = datos.Acciones.Single(a => a.Codigo == CodigosPermiso.RespaldoGestionar);
            Formulario formularioRespaldo = datos.Formularios.Single(f => f.Id == accionRespaldo.FormularioId);
            Modulo moduloSeguridad = datos.Modulos.Single(m => m.Id == formularioRespaldo.ModuloId);

            Assert.AreEqual("Seguridad", moduloSeguridad.Nombre);
            Assert.AreEqual("Resguardo y restauración", formularioRespaldo.Nombre);
            Assert.AreEqual("Generar resguardos y restaurar base de datos", accionRespaldo.Nombre);
            Assert.IsFalse(recepcionista.Acciones.Any(a => a.Codigo == CodigosPermiso.RespaldoGestionar));

            admin.Acciones.Remove(accionRespaldo);
            datos.Acciones.Remove(accionRespaldo);

            // Primera ejecución tras incorporar Etapa 3: crea la acción y la asigna al Administrador existente
            inicializador.Inicializar();
            Assert.AreEqual(1, datos.Acciones.Count(a => a.Codigo == CodigosPermiso.RespaldoGestionar));
            Assert.IsTrue(admin.Acciones.Any(a => a.Codigo == CodigosPermiso.RespaldoGestionar));

            // Re-ejecución inmediata: no duplica ni altera
            inicializador.Inicializar();
            Assert.AreEqual(1, datos.Acciones.Count(a => a.Codigo == CodigosPermiso.RespaldoGestionar));
            Assert.AreEqual(1, admin.Acciones.Count(a => a.Codigo == CodigosPermiso.RespaldoGestionar));

            // Un administrador quita manualmente RESPALDO_GESTIONAR del grupo Administrador
            Accion nuevaAccion = admin.Acciones.Single(a => a.Codigo == CodigosPermiso.RespaldoGestionar);
            admin.Acciones.Remove(nuevaAccion);

            // Arranque posterior: la acción ya existe, no debe volver a asignarse automáticamente
            inicializador.Inicializar();
            Assert.IsFalse(admin.Acciones.Any(a => a.Codigo == CodigosPermiso.RespaldoGestionar));
        }
    }
}
