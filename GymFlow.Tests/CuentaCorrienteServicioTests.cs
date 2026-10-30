using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GymFlow.Tests
{
    [TestClass]
    public class CuentaCorrienteServicioTests
    {
        private CuentaCorrienteServicio CrearServicio(DatosEnMemoria datos)
        {
            return new CuentaCorrienteServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos));
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_CuentaInexistente_RetornaNull()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            ResumenCuentaCorriente resumen = servicio.ConsultarCuentaCorriente(999);

            Assert.IsNull(resumen);
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_SinMovimientos_SaldoCeroYListaVacia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            ResumenCuentaCorriente resumen = servicio.ConsultarCuentaCorriente(1);

            Assert.IsNotNull(resumen);
            Assert.AreEqual(0m, resumen.Saldo);
            Assert.AreEqual(0, resumen.Movimientos.Count);
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_DebitosYCreditos_SaldoEsDebitosMenosCreditos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today.AddDays(-10), Total = 1000m });
            datos.AgregarMovimiento(100, "CREDITO", 400m, new ReciboPago { Numero = "RP-1", FechaEmision = DateTime.Today.AddDays(-5), Total = 400m });
            datos.AgregarMovimiento(100, "CREDITO", 100m, new NotaCredito { Numero = "NC-1", FechaEmision = DateTime.Today.AddDays(-2), Total = 100m });
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            ResumenCuentaCorriente resumen = servicio.ConsultarCuentaCorriente(1);

            Assert.AreEqual(500m, resumen.Saldo);
            Assert.AreEqual(3, resumen.Movimientos.Count);
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_SoloCreditos_SaldoNegativo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "CREDITO", 300m, new ReciboPago { Numero = "RP-1", FechaEmision = DateTime.Today, Total = 300m });
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            ResumenCuentaCorriente resumen = servicio.ConsultarCuentaCorriente(1);

            Assert.AreEqual(-300m, resumen.Saldo);
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_MovimientosOrdenadosPorFecha()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "CREDITO", 100m, new ReciboPago { Numero = "RP-2", FechaEmision = DateTime.Today, Total = 100m });
            datos.AgregarMovimiento(100, "DEBITO", 500m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today.AddDays(-20), Total = 500m });
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            ResumenCuentaCorriente resumen = servicio.ConsultarCuentaCorriente(1);

            Assert.AreEqual("NV-1", resumen.Movimientos[0].Comprobante.Numero);
            Assert.AreEqual("RP-2", resumen.Movimientos[1].Comprobante.Numero);
        }

        [TestMethod]
        public void ConsultarCuentaCorriente_TipoDeMovimientoDesconocido_LanzaExcepcion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Tipos.Add(new TipoMovimiento { Id = 3, Nombre = "OTRO" });
            datos.AgregarMovimiento(100, "OTRO", 50m, new NotaCredito { Numero = "NC-1", FechaEmision = DateTime.Today, Total = 50m });
            CuentaCorrienteServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.ConsultarCuentaCorriente(1));
        }
    }
}
