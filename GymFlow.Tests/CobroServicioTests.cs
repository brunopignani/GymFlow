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

            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 1));
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 2));
            Assert.AreEqual(recibo.Id, datos.Movimientos.Single(m => m.ComprobanteId == recibo.Id).ComprobanteId);
            Assert.AreEqual(100, datos.Movimientos.Single(m => m.TipoMovimientoId == 2).CuentaCorrienteId);
            Assert.AreEqual(2, datos.Movimientos.Single(m => m.TipoMovimientoId == 2).TipoMovimientoId);
            Assert.AreEqual(1000m, datos.Movimientos.Single(m => m.TipoMovimientoId == 2).Importe);
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);

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
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(System.Data.IsolationLevel.Serializable, datos.UltimoNivelAislamiento);

            ResultadoDebitoMensual primero = CrearDebitoServicio(datos).EmitirDebitosMensuales();
            ResultadoDebitoMensual segundo = CrearDebitoServicio(datos).EmitirDebitosMensuales();

            Assert.AreEqual(0, primero.Emitidos);
            Assert.AreEqual(1, primero.YaEmitidos);
            Assert.AreEqual(0, segundo.Emitidos);
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
        public void AplicarSaldoAFavor_CuotaVencidaConSaldo5000_NoAlcanzaLaCuota_Deuda15000()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Cuotas[0].Precio = 15000m;
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(-3);
            datos.CuotasCliente[0].Estado = EstadosCliente.Vencido;
            datos.AgregarMovimiento(100, "CREDITO", 5000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 5000m
            });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AplicarSaldoAFavor(1));
            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(-5000m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Vencido, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(-3), datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_CuotaVencidaConSaldo15000_CubreLaDeudaYSaldoFinalCero()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Cuotas[0].Precio = 15000m;
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(-3);
            datos.CuotasCliente[0].Estado = EstadosCliente.Vencido;
            datos.AgregarMovimiento(100, "CREDITO", 15000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 15000m
            });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AplicarSaldoAFavor(1));

            NotaVenta notaVenta = datos.Comprobantes.OfType<NotaVenta>().Single();
            Assert.AreEqual(15000m, notaVenta.Total);
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.ComprobanteId == notaVenta.Id
                && m.TipoMovimientoId == 1));
            Assert.AreEqual(15000m, datos.Movimientos.Single(m => m.ComprobanteId == notaVenta.Id
                && m.TipoMovimientoId == 1).Importe);
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            CuentaCorrienteServicio cuentaCorrienteServicio = new CuentaCorrienteServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos));
            Assert.AreEqual(0m, cuentaCorrienteServicio.ConsultarCuentaCorriente(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
        }
        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaContabilizadaYSaldoCero_ActivaCuotaSinNuevosMovimientos()
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

            Assert.AreEqual(2, datos.Comprobantes.Count);
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_SaldoDeudorPositivo_RechazaSinCrearRegistros()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "DEBITO", 500m, new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 500m,
                CuotaClienteId = 5
            });
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            string estadoOriginal = datos.CuotasCliente[0].Estado;

            Assert.IsFalse(CrearServicio(datos).AplicarSaldoAFavor(1));

            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
            Assert.AreEqual(estadoOriginal, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_CuotaActiva_RechazaSinCrearRegistros()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(30);
            datos.CuotasCliente[0].Estado = EstadosCliente.Activo;
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;

            Assert.IsFalse(CrearServicio(datos).AplicarSaldoAFavor(1));

            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaHuerfanaConSaldoSuficiente_RegistraDebitoYActivaCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            // NV huérfana: existe pero sin movimiento de débito asociado.
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);
            // Solo crédito, sin débito asociado a la NV.
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 1000m
            });

            Assert.IsTrue(CrearServicio(datos).AplicarSaldoAFavor(1));

            // Se debe haber registrado el débito faltante por la NV.
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.ComprobanteId == nv.Id && m.TipoMovimientoId == 1));
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
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

        [TestMethod]
        public void AplicarSaldoAFavor_DebitoParcia_RechazaPorInconsistencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            // NV por $1000, débito por solo $500 (parcial) → inconsistencia.
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);
            Movimiento debitoParcial = new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 500m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            };
            datos.Movimientos.Add(debitoParcial);
            datos.AgregarMovimiento(100, "CREDITO", 2000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 2000m
            });
            string estadoOriginal = datos.CuotasCliente[0].Estado;

            Assert.IsFalse(CrearServicio(datos).AplicarSaldoAFavor(1));
            Assert.AreEqual(estadoOriginal, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void ObtenerResumenCobro_InconsistenciaContable_MarcarFlagYMinimoEnCero()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            // NV con débito en otra cuenta → inconsistencia.
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);
            // Movimiento asociado a la NV pero en otra cuenta (Id=999).
            Movimiento debitoOtraCuenta = new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 1000m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 999
            };
            datos.Movimientos.Add(debitoOtraCuenta);

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.IsNotNull(resumen);
            Assert.IsTrue(resumen.TieneInconsistenciaContable);
            Assert.AreEqual(0m, resumen.MinimoExigible);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_ConNotaVentaYDebitoPrevio_SaldoExcedenteAFavor_ActivaCuotaYConservaRemanente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.Cuotas[0].Precio = 15000m;
            // NV por 15000 y débito correspondiente ya registrado
            datos.AgregarMovimiento(100, "DEBITO", 15000m, new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 15000m,
                CuotaClienteId = 5
            });
            // Créditos por 30000 -> saldo inicial = -15000
            datos.AgregarMovimiento(100, "CREDITO", 30000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 30000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsTrue(servicio.AplicarSaldoAFavor(1));

            // No debe haber comprobantes ni movimientos adicionales
            Assert.AreEqual(2, datos.Comprobantes.Count);
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(-15000m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_SinNotaVenta_CreditoExcedente_ConservaRemanente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.Cuotas[0].Precio = 15000m;
            // Crédito previo por 20000
            datos.AgregarMovimiento(100, "CREDITO", 20000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 20000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsTrue(servicio.AplicarSaldoAFavor(1));

            NotaVenta nv = datos.Comprobantes.OfType<NotaVenta>().Single();
            Assert.AreEqual(15000m, nv.Total);
            // Saldo final debe ser -5000 (remanente favorable)
            Assert.AreEqual(-5000m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaHuerfana_PrecioHistoricoDiferente_RegistraDebitoPorTotalNota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            // Cuota actual aumentó a 15000, pero la NV histórica emitida es de 12000
            datos.Cuotas[0].Precio = 15000m;
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 12000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Crédito por 12000
            datos.AgregarMovimiento(100, "CREDITO", 12000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 12000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsTrue(servicio.AplicarSaldoAFavor(1));

            // Debe registrar el débito por el total histórico de la nota (12000), no por 15000
            Movimiento debito = datos.Movimientos.Single(m => m.ComprobanteId == nv.Id && m.TipoMovimientoId == 1);
            Assert.AreEqual(12000m, debito.Importe);
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaConMultiplesDebitos_RechazaPorInconsistencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Dos débitos asociados al mismo comprobante
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 500m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            });
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 500m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            });
            // Crédito por 2000
            datos.AgregarMovimiento(100, "CREDITO", 2000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 2000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsFalse(servicio.AplicarSaldoAFavor(1));
            Assert.IsNull(datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaConDebitoExcesivo_RechazaPorInconsistencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Débito excesivo de 2000 para una nota de 1000
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 2000m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            });
            // Crédito por 3000
            datos.AgregarMovimiento(100, "CREDITO", 3000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 3000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsFalse(servicio.AplicarSaldoAFavor(1));
        }

        [TestMethod]
        public void AplicarSaldoAFavor_MovimientoEnOtraCuentaCorriente_RechazaPorInconsistencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Débito registrado con CuentaCorrienteId de otra cuenta (ej: 999)
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 1000m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 999
            });
            // Crédito en la cuenta del cliente por 2000
            datos.AgregarMovimiento(100, "CREDITO", 2000m, new ReciboPago
            {
                Numero = "RP-1",
                FechaEmision = DateTime.Today,
                Total = 2000m
            });

            CobroServicio servicio = CrearServicio(datos);
            Assert.IsFalse(servicio.AplicarSaldoAFavor(1));
            Assert.IsNull(datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void RegistrarCobro_ConInconsistenciaContable_RechazaOperacion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Débito registrado en otra cuenta corriente (inconsistencia)
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 1000m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 999
            });

            CobroServicio servicio = CrearServicio(datos);
            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsFalse(resultado);
            // No debe haberse registrado recibo
            Assert.IsFalse(datos.Comprobantes.OfType<ReciboPago>().Any());
        }

        [TestMethod]
        public void AplicarSaldoAFavor_NotaVentaConDebitoYCreditoAsociado_RechazaPorInconsistencia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.Cuotas[0].Precio = 15000m;

            // NotaVenta de $15.000
            NotaVenta nv = new NotaVenta
            {
                Numero = "NV-1",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 15000m,
                CuotaClienteId = 5
            };
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);

            // Débito correcto de $15.000 en la cuenta
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 15000m,
                TipoMovimientoId = 1,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            });

            // Movimiento adicional CREDITO asociado al mismo comprobante y cuenta
            datos.Movimientos.Add(new Movimiento
            {
                Id = datos.Movimientos.Count + 1,
                Fecha = DateTime.Today,
                Importe = 15000m,
                TipoMovimientoId = 2,
                ComprobanteId = nv.Id,
                CuentaCorrienteId = 100
            });

            int comprobantesOriginales = datos.Comprobantes.Count;
            int movimientosOriginales = datos.Movimientos.Count;
            DateTime? vencimientoOriginal = datos.CuotasCliente[0].FechaVencimiento;
            string estadoOriginal = datos.CuotasCliente[0].Estado;

            CobroServicio servicio = CrearServicio(datos);

            // 1. Detectar inconsistencia contable en el resumen
            ResumenCobro resumen = servicio.ObtenerResumenCobro(1);
            Assert.IsNotNull(resumen);
            Assert.IsTrue(resumen.TieneInconsistenciaContable);
            Assert.AreEqual(0m, resumen.MinimoExigible);

            // 2. Rechazar la operación
            Assert.IsFalse(servicio.AplicarSaldoAFavor(1));

            // 3. No modificar ningún dato
            Assert.AreEqual(comprobantesOriginales, datos.Comprobantes.Count);
            Assert.AreEqual(movimientosOriginales, datos.Movimientos.Count);
            Assert.AreEqual(vencimientoOriginal, datos.CuotasCliente[0].FechaVencimiento);
            Assert.AreEqual(estadoOriginal, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void RegistrarCobro_MetodoPagoNoPermitido_RetornaFalseSinIniciarTransaccion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "Cheque");

            Assert.IsFalse(resultado);
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
        }

        [TestMethod]
        public void RegistrarCobro_MetodoPagoConEspaciosYMinusculas_NormalizaElNombreEnElRecibo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "  tarjeta de débito  ");

            Assert.IsTrue(resultado);
            ReciboPago recibo = datos.Comprobantes.OfType<ReciboPago>().Single();
            Assert.AreEqual("Tarjeta de Débito", recibo.MetodoPago);
        }

        [TestMethod]
        public void RegistrarCobro_NotaVentaHuerfanaDelPeriodo_RegistraDebitoFaltanteYActivaCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = new NotaVenta
            {
                Id = 1,
                Numero = "NV-00000001",
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = 1000m,
                CuotaClienteId = 5
            };
            datos.Comprobantes.Add(nv);
            CobroServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsTrue(resultado);
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.ComprobanteId == nv.Id && m.TipoMovimientoId == 1 && m.Importe == 1000m));
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 2 && m.Importe == 1000m));
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(DateTime.Today.AddDays(30), datos.CuotasCliente[0].FechaVencimiento);
        }
    }
}
