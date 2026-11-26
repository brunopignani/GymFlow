using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class InicializadorDatosServicioTests
    {
        private static InicializadorDatosServicio CrearServicio(DatosEnMemoria datos)
        {
            return new InicializadorDatosServicio(
                new TipoMovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                new GrupoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        [TestMethod]
        public void Inicializar_BaseVacia_CreaTiposCuotaCatalogoYGruposExactos()
        {
            DatosEnMemoria datos = new DatosEnMemoria();
            InicializadorDatosServicio servicio = CrearServicio(datos);

            servicio.Inicializar();

            Assert.AreEqual(1, datos.TransaccionesIniciadas);
            Assert.AreEqual(1, datos.Confirmadas);
            Assert.AreEqual(0, datos.Revertidas);

            // Tipos de movimiento
            CollectionAssert.AreEquivalent(
                new[] { NombresTipoMovimiento.Debito, NombresTipoMovimiento.Credito },
                datos.Tipos.Select(t => t.Nombre).ToList());

            // Cuota base
            Cuota mensual = datos.Cuotas.Single();
            Assert.AreEqual(CuotaServicio.NombreCuotaMensual, mensual.Nombre);
            Assert.AreEqual(15000m, mensual.Precio);

            // Módulos, formularios y 15 acciones
            CollectionAssert.AreEquivalent(
                new[] { "Mostrador", "Gestión", "Finanzas", "Seguridad" },
                datos.Modulos.Select(m => m.Nombre).ToList());
            Assert.AreEqual(15, datos.Formularios.Count);
            Assert.AreEqual(15, datos.Acciones.Count);

            string[] todosLosCodigos =
            {
                CodigosPermiso.ClientesGestionar,
                CodigosPermiso.ActividadesGestionar,
                CodigosPermiso.InscripcionesGestionar,
                CodigosPermiso.AutoingresoUsar,
                CodigosPermiso.CobroRegistrar,
                CodigosPermiso.CuentaCorrienteVer,
                CodigosPermiso.NotaCreditoEmitir,
                CodigosPermiso.AjusteCuentaRegistrar,
                CodigosPermiso.DebitoMensualEmitir,
                CodigosPermiso.MorososBaja,
                CodigosPermiso.UsuariosGestionar,
                CodigosPermiso.GruposGestionar,
                CodigosPermiso.CuotaEditarPrecio,
                CodigosPermiso.AuditoriaVer,
                CodigosPermiso.RespaldoGestionar
            };
            CollectionAssert.AreEquivalent(todosLosCodigos, datos.Acciones.Select(a => a.Codigo).ToList());

            // Grupos base
            Assert.AreEqual(2, datos.Grupos.Count);

            Grupo administrador = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Administrador);
            Assert.AreEqual(EstadosGrupo.Activo, administrador.Estado);
            CollectionAssert.AreEquivalent(todosLosCodigos, administrador.ObtenerCodigosPermiso().ToList());

            Grupo recepcionista = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Recepcionista);
            Assert.AreEqual(EstadosGrupo.Activo, recepcionista.Estado);
            string[] codigosRecepcionista =
            {
                CodigosPermiso.ClientesGestionar,
                CodigosPermiso.ActividadesGestionar,
                CodigosPermiso.InscripcionesGestionar,
                CodigosPermiso.AutoingresoUsar,
                CodigosPermiso.CobroRegistrar,
                CodigosPermiso.CuentaCorrienteVer,
                CodigosPermiso.DebitoMensualEmitir,
                CodigosPermiso.MorososBaja
            };
            CollectionAssert.AreEquivalent(codigosRecepcionista, recepcionista.ObtenerCodigosPermiso().ToList());

            // No crea ningún usuario administrador
            Assert.AreEqual(0, datos.Usuarios.Count);
            Assert.AreEqual(0, datos.Empleados.Count);
        }

        [TestMethod]
        public void Inicializar_EjecutadoDosVeces_EsIdempotenteYNoDuplica()
        {
            DatosEnMemoria datos = new DatosEnMemoria();
            InicializadorDatosServicio servicio = CrearServicio(datos);

            servicio.Inicializar();
            servicio.Inicializar();

            Assert.AreEqual(2, datos.Tipos.Count);
            Assert.AreEqual(1, datos.Cuotas.Count);
            Assert.AreEqual(4, datos.Modulos.Count);
            Assert.AreEqual(15, datos.Formularios.Count);
            Assert.AreEqual(15, datos.Acciones.Count);
            Assert.AreEqual(2, datos.Grupos.Count);
            Assert.AreEqual(15, datos.Grupos.Single(g => g.Nombre == NombresGrupo.Administrador).Acciones.Count);
            Assert.AreEqual(8, datos.Grupos.Single(g => g.Nombre == NombresGrupo.Recepcionista).Acciones.Count);
        }

        [TestMethod]
        public void Inicializar_ConModificacionesManuales_NoSobrescribePrecioNiGruposExistentes()
        {
            DatosEnMemoria datos = new DatosEnMemoria();
            InicializadorDatosServicio servicio = CrearServicio(datos);

            servicio.Inicializar();

            // Modificación manual del precio de cuota
            Cuota mensual = datos.Cuotas.Single();
            mensual.Precio = 27500m;

            // Modificación manual de Recepcionista (se quita una acción y se desactiva)
            Grupo recepcionista = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Recepcionista);
            Accion quitar = recepcionista.Acciones.Single(a => a.Codigo == CodigosPermiso.MorososBaja);
            recepcionista.Acciones.Remove(quitar);
            recepcionista.Estado = EstadosGrupo.Inactivo;

            // Modificación manual de Administrador (se quita una acción)
            Grupo administrador = datos.Grupos.Single(g => g.Nombre == NombresGrupo.Administrador);
            Accion quitarAdmin = administrador.Acciones.Single(a => a.Codigo == CodigosPermiso.AutoingresoUsar);
            administrador.Acciones.Remove(quitarAdmin);

            // Re-ejecución
            servicio.Inicializar();

            Assert.AreEqual(27500m, datos.Cuotas.Single().Precio);
            Assert.AreEqual(EstadosGrupo.Inactivo, recepcionista.Estado);
            Assert.AreEqual(7, recepcionista.Acciones.Count);
            Assert.IsFalse(recepcionista.Acciones.Any(a => a.Codigo == CodigosPermiso.MorososBaja));
            Assert.AreEqual(14, administrador.Acciones.Count);
            Assert.IsFalse(administrador.Acciones.Any(a => a.Codigo == CodigosPermiso.AutoingresoUsar));
        }
    }
}
