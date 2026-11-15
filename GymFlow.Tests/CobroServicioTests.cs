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
                new TransaccionEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new DebitoMensualServicio(
                    new ClienteRepositorioEnMemoria(datos),
                    new CuentaCorrienteRepositorioEnMemoria(datos),
                    new CuotaClienteRepositorioEnMemoria(datos),
                    new CuotaRepositorioEnMemoria(datos),
                    new MovimientoRepositorioEnMemoria(datos),
                    new ComprobanteRepositorioEnMemoria(datos),
                    new TipoMovimientoRepositorioEnMemoria(datos),
                    new TransaccionEnMemoria(datos)));
        }

        private DebitoMensualServicio CrearDebitoServicio(DatosEnMemoria datos)
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

        [TestMethod]
        public void RegistrarCobro_DatosValidos_ActualizaCuotaGeneraReciboYMovimientoYConfirma()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsTrue(resultado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);

            ReciboPago recibo = datos.Comprobantes.OfType<ReciboPago>().Single();
            Assert.AreEqual(1000m, recibo.Total);
            Assert.AreEqual("RP-00000001", recibo.Numero);
            Assert.AreEqual("Efectivo", recibo.MetodoPago);
            Assert.AreEqual(10, recibo.EmpleadoId);
            Assert.AreEqual(DateTime.Today, recibo.FechaEmision);

            Movimiento movimiento = datos.Movimientos.Single();
            Assert.AreEqual(recibo.Id, movimiento.ComprobanteId);
            Assert.AreEqual(100, movimiento.CuentaCorrienteId);
            Assert.AreEqual(2, movimiento.TipoMovimientoId);
            Assert.AreEqual(1000m, movimiento.Importe);

            Assert.AreEqual(1, datos.Confirmadas);
            Assert.AreEqual(0, datos.Revertidas);
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
        }

        [TestMethod]
        public void RegistrarCobro_CuotaYaVigente_RechazaLaReactivacion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));

            Assert.AreEqual(1, datos.Comprobantes.Select(c => c.Numero).Distinct().Count());
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
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 20111222, Nombre = "Luis", Apellido = "Sol" });
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
            PrepararCuotaPendiente(datos);
            datos.RechazarGuardarComprobante = true;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsFalse(resultado);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void RegistrarCobro_FallaAlGuardarMovimiento_RevierteTodoYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.FallarAlGuardarMovimiento = true;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            CobroServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));

            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void ObtenerResumenCobro_CuentaNueva_ExigePrecioCompleto()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(0m, resumen.Saldo);
            Assert.AreEqual(1000m, resumen.PrecioCuota);
            Assert.AreEqual(1000m, resumen.MinimoExigible);
        }

        [TestMethod]
        public void ObtenerResumenCobro_ConDeuda_ExigeElSaldoDeudor()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            });

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(1000m, resumen.Saldo);
            Assert.AreEqual(1000m, resumen.MinimoExigible);
            Assert.IsTrue(resumen.TieneNotaVentaDelPeriodo);
        }

        [TestMethod]
        public void ObtenerResumenCobro_SaldoAFavorParcial_ExigeLaDiferencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 400m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 400m
            });

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(-400m, resumen.Saldo);
            Assert.AreEqual(600m, resumen.MinimoExigible);
        }

        [TestMethod]
        public void ObtenerResumenCobro_SaldoAFavorSuficiente_MinimoCero()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(-1000m, resumen.Saldo);
            Assert.AreEqual(0m, resumen.MinimoExigible);
        }

        [TestMethod]
        public void RegistrarCobro_MinimoCero_RechazaUnPesoSinModificarRegistros()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1m, "Efectivo"));
            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(-1000m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.IsNull(datos.CuotasCliente[0].FechaVencimiento);
            Assert.IsNull(datos.CuotasCliente[0].Estado);
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
        }

        [TestMethod]
        public void RegistrarCobro_MinimoCero_RechazaCualquierImportePositivo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 500m, "Efectivo"));
            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(1, datos.Revertidas);
        }

        [TestMethod]
        public void RegistrarCobro_AnticipadoYDebitoMensual_ConsumeElCreditoSinDuplicar()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.AreEqual(-1000m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
            Assert.AreEqual(0, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);

            ResultadoDebitoMensual primero = CrearDebitoServicio(datos).EmitirDebitosMensuales();
            ResultadoDebitoMensual segundo = CrearDebitoServicio(datos).EmitirDebitosMensuales();

            Assert.AreEqual(1, primero.Emitidos);
            Assert.AreEqual(1, segundo.YaEmitidos);
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
        }

        [TestMethod]
        public void RegistrarCobro_ImporteInferiorAlMinimo_RechazaSinCambios()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 999m, "Efectivo"));
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
            Assert.AreEqual(1, datos.Revertidas);
        }

        [TestMethod]
        public void RegistrarCobro_ImporteSuperiorAlMinimo_ActivaLaCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1200m, "Efectivo"));
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void RegistrarCobro_CuotaVencida_SePuedeRenovarConMinimo()
        {
            CobroServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
        }

        [TestMethod]
        public void AplicarSaldoAFavor_SinNotaVenta_GeneraDebitoSinReciboNiCredito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });

            Assert.IsTrue(CrearServicio(datos).AplicarSaldoAFavor(1));
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 2));
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaExistente_NoLaDuplica()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "DEBITO", 1000m, new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            });
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });

            Assert.IsTrue(CrearServicio(datos).AplicarSaldoAFavor(1));
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_SaldoInsuficiente_RechazaYRevierte()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 500m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 500m
            });

            Assert.IsFalse(CrearServicio(datos).AplicarSaldoAFavor(1));
            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(1, datos.Revertidas);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_UsaLaUltimaAsignacionDeCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.Cuotas.Add(new Cuota { Id = 2, Nombre = "Premium", Precio = 1500m });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 1, CuotaId = 2 });
            datos.AgregarMovimiento(100, "CREDITO", 1500m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1500m
            });

            Assert.IsTrue(CrearServicio(datos).AplicarSaldoAFavor(1));
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente.Single(cc => cc.Id == 6).Estado);
            Assert.IsNull(datos.CuotasCliente.Single(cc => cc.Id == 5).Estado);
            Assert.AreEqual(6, datos.Comprobantes.OfType<NotaVenta>().Single().CuotaClienteId);
        }

        private static void PrepararCuotaPendiente(DatosEnMemoria datos)
        {
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
        }
    }
}
