using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class PagoServicioTests
    {
        private PagoServicio CrearServicio(DatosEnMemoria datos)
        {
            return new PagoServicio(
                new PagoRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        private Pago NuevoPago()
        {
            return new Pago { EmpleadoId = 10, CuotaClienteId = 5, Monto = 0m, Fecha = DateTime.Today };
        }

        [TestMethod]
        public void RegistrarPago_DatosValidos_RenuevaElVencimientoGuardaElPagoYConfirma()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PagoServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarPago(NuevoPago());

            Assert.IsTrue(resultado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
            Assert.AreEqual(1, datos.Pagos.Count);
            Assert.AreEqual(1, datos.Confirmadas);
            Assert.AreEqual(0, datos.Revertidas);
        }

        [TestMethod]
        public void RegistrarPago_CuotaInexistente_RetornaFalseSinTocarNada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PagoServicio servicio = CrearServicio(datos);
            Pago pago = NuevoPago();
            pago.CuotaClienteId = 99;

            Assert.IsFalse(servicio.RegistrarPago(pago));
            Assert.AreEqual(0, datos.Pagos.Count);
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void RegistrarPago_FallaAlGuardarElPago_RevierteElVencimientoYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.FallarAlGuardarPago = true;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            PagoServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.RegistrarPago(NuevoPago()));

            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
            Assert.AreEqual(0, datos.Pagos.Count);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
        }

        [TestMethod]
        public void GetPagosByCliente_DevuelveSoloLosPagosDeEseCliente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PagoServicio servicio = CrearServicio(datos);
            servicio.RegistrarPago(NuevoPago());

            Assert.AreEqual(1, servicio.GetPagosByCliente(1).Count);
            Assert.AreEqual(0, servicio.GetPagosByCliente(2).Count);
        }
    }
}
