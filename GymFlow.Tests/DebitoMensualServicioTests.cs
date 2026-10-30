using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class DebitoMensualServicioTests
    {
        private DebitoMensualServicio CrearServicio(DatosEnMemoria datos)
        {
            return new DebitoMensualServicio(
                new ClienteRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        private void AgregarClienteConCuenta(DatosEnMemoria datos, int clienteId, int cuentaId)
        {
            datos.Clientes.Add(new Cliente { Id = clienteId, Dni = 1000 + clienteId, Nombre = "Cliente" + clienteId, Apellido = "Prueba" });
            datos.Cuentas.Add(new CuentaCorriente { Id = cuentaId, ClienteId = clienteId });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 50 + clienteId, ClienteId = clienteId, CuotaId = 1, FechaVencimiento = DateTime.Today });
        }

        [TestMethod]
        public void EmitirDebitosMensuales_ClienteConCuentaYCuota_EmiteNotaVentaYMovimientoDebito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(1, resultado.Emitidos);
            Assert.AreEqual(0, resultado.YaEmitidos);
            Assert.AreEqual(0, resultado.NoProcesados.Count);

            NotaVenta notaVenta = datos.Comprobantes.OfType<NotaVenta>().Single();
            Assert.AreEqual(1000m, notaVenta.Total);
            Assert.AreEqual(5, notaVenta.CuotaClienteId);
            Assert.AreEqual(DateTime.Today, notaVenta.FechaEmision);

            Movimiento movimiento = datos.Movimientos.Single();
            Assert.AreEqual(notaVenta.Id, movimiento.ComprobanteId);
            Assert.AreEqual(100, movimiento.CuentaCorrienteId);
            Assert.AreEqual(1, movimiento.TipoMovimientoId);
            Assert.AreEqual(1000m, movimiento.Importe);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_SegundaCorridaDelMismoPeriodo_NoDuplicaElDebito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            DebitoMensualServicio servicio = CrearServicio(datos);

            servicio.EmitirDebitosMensuales();
            ResultadoDebitoMensual segunda = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(0, segunda.Emitidos);
            Assert.AreEqual(1, segunda.YaEmitidos);
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(1, datos.Movimientos.Count);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_DebitoDeUnMesAnterior_EmiteElDelMesActual()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta { Numero = "NV-VIEJA", FechaEmision = DateTime.Today.AddMonths(-1), Total = 1000m, CuotaClienteId = 5 });
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(1, resultado.Emitidos);
            Assert.AreEqual(2, datos.Comprobantes.OfType<NotaVenta>().Count());
        }

        [TestMethod]
        public void EmitirDebitosMensuales_ClienteSinCuentaCorriente_LoInformaComoNoProcesado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 222, Nombre = "Luis", Apellido = "Sol" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today });
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(1, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.IsTrue(resultado.NoProcesados[0].Contains("Sol Luis"));
        }

        [TestMethod]
        public void EmitirDebitosMensuales_ClienteSinCuota_LoInformaComoNoProcesado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(0, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_PrecioDeCuotaInvalido_LoInformaComoNoProcesado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Cuotas[0].Precio = 0m;
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(0, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.AreEqual(0, datos.Comprobantes.Count);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_SinTipoDebito_NoProcesaNada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Tipos.RemoveAll(t => t.Nombre == "DEBITO");
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(0, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_FallaUnClienteConExcepcion_RevierteEseClienteYContinuaConLosDemas()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarClienteConCuenta(datos, 2, 200);
            AgregarClienteConCuenta(datos, 3, 300);
            datos.CuentaQueFallaAlGuardarMovimiento = 200;
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(2, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(2, datos.Confirmadas);
            Assert.AreEqual(2, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(0, datos.Movimientos.Count(m => m.CuentaCorrienteId == 200));
        }

        [TestMethod]
        public void EmitirDebitosMensuales_NumerosDeComprobanteUnicos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarClienteConCuenta(datos, 2, 200);
            AgregarClienteConCuenta(datos, 3, 300);
            DebitoMensualServicio servicio = CrearServicio(datos);

            servicio.EmitirDebitosMensuales();

            Assert.AreEqual(3, datos.Comprobantes.Select(c => c.Numero).Distinct().Count());
        }
    }
}
