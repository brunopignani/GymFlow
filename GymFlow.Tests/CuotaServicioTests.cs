using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class CuotaServicioTests
    {
        private CuotaServicio CrearServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto = null)
        {
            return new CuotaServicio(
                new CuotaRepositorioEnMemoria(datos),
                contexto ?? ContextoUsuarioEnMemoria.ConTodosLosPermisos());
        }

        [TestMethod]
        public void ObtenerCuotaMensual_DevuelveLaCuotaMensual()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Cuota cuota = CrearServicio(datos, ContextoUsuarioEnMemoria.SinPermisos()).ObtenerCuotaMensual();
            Assert.IsNotNull(cuota);
            Assert.AreEqual("Mensual", cuota.Nombre);
        }

        [TestMethod]
        public void ActualizarPrecioMensual_PrecioValido_ActualizaSoloElPrecio()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Assert.IsTrue(CrearServicio(datos).ActualizarPrecioMensual(1500.50m));
            Assert.AreEqual(1500.50m, datos.Cuotas.Single().Precio);
            Assert.AreEqual("Mensual", datos.Cuotas.Single().Nombre);
        }

        [TestMethod]
        public void ActualizarPrecioMensual_PrecioNoPositivo_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CuotaServicio servicio = CrearServicio(datos);
            Assert.IsFalse(servicio.ActualizarPrecioMensual(0m));
            Assert.IsFalse(servicio.ActualizarPrecioMensual(-10m));
            Assert.AreEqual(1000m, datos.Cuotas.Single().Precio);
        }

        [TestMethod]
        public void ActualizarPrecioMensual_SinPermisoONoAutenticado_Rechaza()
        {
            foreach (ContextoUsuarioEnMemoria contexto in new[]
            {
                ContextoUsuarioEnMemoria.SinPermisos(),
                ContextoUsuarioEnMemoria.NoAutenticado(),
                ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.CobroRegistrar)
            })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                Assert.IsFalse(CrearServicio(datos, contexto).ActualizarPrecioMensual(2000m));
                Assert.AreEqual(1000m, datos.Cuotas.Single().Precio);
            }
        }

        [TestMethod]
        public void ActualizarPrecioMensual_ConPermisoEspecifico_Actualiza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CuotaServicio servicio = CrearServicio(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.CuotaEditarPrecio));
            Assert.IsTrue(servicio.ActualizarPrecioMensual(1200m));
        }

        [TestMethod]
        public void ActualizarPrecioMensual_NoAlteraComprobantesNiMovimientosHistoricos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today, Total = 1000m, CuotaClienteId = 5 });

            Assert.IsTrue(CrearServicio(datos).ActualizarPrecioMensual(2500m));

            Assert.AreEqual(1000m, datos.Comprobantes.Single().Total);
            Assert.AreEqual(1000m, datos.Movimientos.Single().Importe);
            Assert.AreEqual(2500m, datos.Cuotas.Single().Precio);
        }
    }
}
