using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class CobroServicioTests
    {
        private CobroServicio CrearServicio(DatosEnMemoria datos)
        {
            return new CobroServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        [TestMethod]
        public void RegistrarCobro_DatosValidos_ActualizaCuotaGeneraReciboYMovimientoYConfirma()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 400m, "Efectivo");

            Assert.IsTrue(resultado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);

            ReciboPago recibo = datos.Comprobantes.OfType<ReciboPago>().Single();
            Assert.AreEqual(400m, recibo.Total);
            Assert.AreEqual("Efectivo", recibo.MetodoPago);
            Assert.AreEqual(10, recibo.EmpleadoId);
            Assert.AreEqual(DateTime.Today, recibo.FechaEmision);

            Movimiento movimiento = datos.Movimientos.Single();
            Assert.AreEqual(recibo.Id, movimiento.ComprobanteId);
            Assert.AreEqual(100, movimiento.CuentaCorrienteId);
            Assert.AreEqual(2, movimiento.TipoMovimientoId);
            Assert.AreEqual(400m, movimiento.Importe);

            Assert.AreEqual(1, datos.Confirmadas);
            Assert.AreEqual(0, datos.Revertidas);
        }

        [TestMethod]
        public void RegistrarCobro_DosCobros_GeneraNumerosDeReciboDistintos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            servicio.RegistrarCobro(1, 10, 100m, "Efectivo");
            servicio.RegistrarCobro(1, 10, 100m, "Efectivo");

            Assert.AreEqual(2, datos.Comprobantes.Select(c => c.Numero).Distinct().Count());
        }

        [TestMethod]
        public void RegistrarCobro_ImporteCero_RetornaFalseSinTocarNada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 0m, "Efectivo");

            Assert.IsFalse(resultado);
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
        }

        [TestMethod]
        public void RegistrarCobro_ImporteNegativo_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, -50m, "Efectivo"));
        }

        [TestMethod]
        public void RegistrarCobro_MetodoPagoVacio_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 100m, "  "));
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void RegistrarCobro_EmpleadoInexistente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 99, 100m, "Efectivo"));
            Assert.AreEqual(0, datos.Comprobantes.Count);
        }

        [TestMethod]
        public void RegistrarCobro_ClienteSinCuentaCorriente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 222, Nombre = "Luis", Apellido = "Sol" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(2, 10, 100m, "Efectivo"));
        }

        [TestMethod]
        public void RegistrarCobro_ClienteSinCuota_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 100m, "Efectivo"));
        }

        [TestMethod]
        public void RegistrarCobro_SinTipoCredito_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Tipos.RemoveAll(t => t.Nombre == "CREDITO");
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 100m, "Efectivo"));
        }

        [TestMethod]
        public void RegistrarCobro_ReciboRechazado_RevierteTodoYRetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.RechazarGuardarComprobante = true;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 100m, "Efectivo");

            Assert.IsFalse(resultado);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void RegistrarCobro_FallaAlGuardarMovimiento_RevierteTodoYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.FallarAlGuardarMovimiento = true;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            CobroServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.RegistrarCobro(1, 10, 100m, "Efectivo"));

            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
        }
    }
}
