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
                new TransaccionEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos());
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
            Assert.AreEqual(1, resultado.Detalles.Count);
            Assert.AreEqual("Emitido", resultado.Detalles[0].Resultado);
            Assert.AreEqual("NV-00000001", resultado.Detalles[0].NumeroComprobante);
            Assert.AreEqual(10111222, resultado.Detalles[0].Dni);

            NotaVenta notaVenta = datos.Comprobantes.OfType<NotaVenta>().Single();
            Assert.AreEqual(1000m, notaVenta.Total);
            Assert.AreEqual(5, notaVenta.CuotaClienteId);
            Assert.AreEqual(DateTime.Today, notaVenta.FechaEmision);
            Assert.AreEqual(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), notaVenta.Periodo);

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
            Assert.AreEqual(1, segunda.Detalles.Count);
            Assert.AreEqual("Ya emitido", segunda.Detalles[0].Resultado);
            Assert.AreEqual("NV-00000001", segunda.Detalles[0].NumeroComprobante);
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
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 20111222, Nombre = "Luis", Apellido = "Sol" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today });
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(1, resultado.Emitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.IsTrue(resultado.NoProcesados[0].Contains("Sol Luis"));
            Assert.AreEqual(2, resultado.Detalles.Count);
            Assert.AreEqual(1, resultado.Detalles.Count(d => d.Resultado == "Emitido"));
            Assert.AreEqual(1, resultado.Detalles.Count(d => d.Resultado == "No procesado"));
            Assert.AreEqual("No tiene cuenta corriente.", resultado.Detalles.Single(d => d.Resultado == "No procesado").Motivo);
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
            Assert.AreEqual(1, resultado.Detalles.Count);
            Assert.AreEqual("No procesado", resultado.Detalles[0].Resultado);
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Detalles[0].Motivo));
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

        [TestMethod]
        public void EmitirDebitosMensuales_ClientePendienteSinPrimeraActivacion_LoExcluyeYNoGeneraDebito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(0, resultado.Emitidos);
            Assert.AreEqual(0, resultado.YaEmitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.AreEqual(1, resultado.Detalles.Count);
            Assert.AreEqual("No procesado", resultado.Detalles[0].Resultado);
            Assert.AreEqual("La cuota está pendiente de primera activación.", resultado.Detalles[0].Motivo);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void EmitirDebitosMensuales_ClientesActivoVencidoYPendiente_SoloDebitaActivados()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            // Cliente 1 en CrearBase está VENCIDO (FechaVencimiento = hoy - 3 días)
            // Cliente 2: ACTIVO (FechaVencimiento = hoy + 10 días)
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 20111222, Nombre = "Luis", Apellido = "Sol" });
            datos.Cuentas.Add(new CuentaCorriente { Id = 200, ClienteId = 2 });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(10), Estado = EstadosCliente.Activo });
            // Cliente 3: PENDIENTE (nunca activado)
            datos.Clientes.Add(new Cliente { Id = 3, Dni = 30111222, Nombre = "Eva", Apellido = "Rio" });
            datos.Cuentas.Add(new CuentaCorriente { Id = 300, ClienteId = 3 });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 7, ClienteId = 3, CuotaId = 1, FechaVencimiento = null, Estado = null });

            DebitoMensualServicio servicio = CrearServicio(datos);

            ResultadoDebitoMensual resultado = servicio.EmitirDebitosMensuales();

            Assert.AreEqual(2, resultado.Emitidos);
            Assert.AreEqual(0, resultado.YaEmitidos);
            Assert.AreEqual(1, resultado.NoProcesados.Count);
            Assert.AreEqual(3, resultado.Detalles.Count);
            Assert.AreEqual(2, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.IsFalse(datos.Movimientos.Any(m => m.CuentaCorrienteId == 300));
            Assert.AreEqual("No procesado", resultado.Detalles.Single(d => d.Dni == 30111222).Resultado);
        }
    }
}
