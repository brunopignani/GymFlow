using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class NotaCreditoServicioTests
    {
        private NotaCreditoServicio CrearServicio(DatosEnMemoria datos)
        {
            return new NotaCreditoServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        private DatosEnMemoria CrearConNotaVenta()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today, Total = 1000m, CuotaClienteId = 5 });
            return datos;
        }

        [TestMethod]
        public void EmitirNotaCredito_DatosValidos_GeneraNotaYMovimientoCredito()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            bool resultado = servicio.EmitirNotaCredito(1, 1, 300m, "Bonificación");

            Assert.IsTrue(resultado);

            NotaCredito notaCredito = datos.Comprobantes.OfType<NotaCredito>().Single();
            Assert.AreEqual(300m, notaCredito.Total);
            Assert.AreEqual("Bonificación", notaCredito.Motivo);

            Movimiento movimiento = datos.Movimientos.Last();
            Assert.AreEqual(notaCredito.Id, movimiento.ComprobanteId);
            Assert.AreEqual(100, movimiento.CuentaCorrienteId);
            Assert.AreEqual(2, movimiento.TipoMovimientoId);
            Assert.AreEqual(300m, movimiento.Importe);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void EmitirNotaCredito_ImporteIgualAlTotal_EsValido()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.EmitirNotaCredito(1, 1, 1000m, "Cancelación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_ImporteMayorAlTotalDeLaNotaDeVenta_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(1, 1, 1000.01m, "Cancelación"));
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void EmitirNotaCredito_ImporteCeroONegativo_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(1, 1, 0m, "Bonificación"));
            Assert.IsFalse(servicio.EmitirNotaCredito(1, 1, -10m, "Bonificación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_MotivoVacio_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(1, 1, 100m, ""));
        }

        [TestMethod]
        public void EmitirNotaCredito_ComprobanteInexistente_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(1, 999, 100m, "Bonificación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_ComprobanteQueNoEsNotaDeVenta_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            datos.AgregarMovimiento(100, "CREDITO", 200m, new ReciboPago { Numero = "RP-1", FechaEmision = DateTime.Today, Total = 200m });
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(1, 2, 100m, "Bonificación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_NotaDeVentaDeOtraCuenta_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 222, Nombre = "Luis", Apellido = "Sol" });
            datos.Cuentas.Add(new CuentaCorriente { Id = 200, ClienteId = 2 });
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(2, 1, 100m, "Bonificación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_ClienteSinCuenta_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EmitirNotaCredito(999, 1, 100m, "Bonificación"));
        }

        [TestMethod]
        public void EmitirNotaCredito_FallaAlGuardarMovimiento_RevierteYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = CrearConNotaVenta();
            datos.FallarAlGuardarMovimiento = true;
            NotaCreditoServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.EmitirNotaCredito(1, 1, 100m, "Bonificación"));

            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(0, datos.Comprobantes.OfType<NotaCredito>().Count());
            Assert.AreEqual(1, datos.Movimientos.Count);
        }
    }
}
