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
                    new TransaccionEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos()), ContextoUsuarioEnMemoria.ConTodosLosPermisos());
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
                new TransaccionEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos());
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
        public void RegistrarCobro_ImporteSuperiorAlMinimo_RechazaSinCambiosYNoGeneraSaldoAFavor()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1200m, "Efectivo"));
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.IsNull(datos.CuotasCliente[0].Estado);
            Assert.IsNull(datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void RegistrarCobro_CuotaVencida_SePuedeRenovarConMinimo()
        {
            CobroServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
        }

        private static void PrepararCuotaPendiente(DatosEnMemoria datos)
        {
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
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

        private static NotaVenta CrearNotaVentaDelPeriodo(DatosEnMemoria datos, decimal total)
        {
            return new NotaVenta
            {
                Numero = "NV-0000000" + (datos.Comprobantes.Count + 1),
                FechaEmision = DateTime.Today,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                Total = total,
                CuotaClienteId = 5
            };
        }

        private static void PrepararNotaVentaPrevia(DatosEnMemoria datos)
        {
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "DEBITO", 1000m, CrearNotaVentaDelPeriodo(datos, 1000m));
        }

        private static void PrepararNotaVentaHuerfana(DatosEnMemoria datos)
        {
            PrepararCuotaPendiente(datos);
            NotaVenta nv = CrearNotaVentaDelPeriodo(datos, 1000m);
            nv.Id = datos.Comprobantes.Count + 1;
            datos.Comprobantes.Add(nv);
        }

        private static void AssertSinCambios(DatosEnMemoria datos, int comprobantes, int movimientos)
        {
            Assert.AreEqual(comprobantes, datos.Comprobantes.Count);
            Assert.AreEqual(movimientos, datos.Movimientos.Count);
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.IsNull(datos.CuotasCliente[0].Estado);
            Assert.IsNull(datos.CuotasCliente[0].FechaVencimiento);
        }

        [TestMethod]
        public void RegistrarCobro_SinNotaVenta_ImporteIgualAlMinimo_GeneraNotaVentaDebitoReciboYCredito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void RegistrarCobro_SinNotaVenta_ImporteInferiorOSuperior_RechazaSinCambios()
        {
            foreach (decimal importe in new[] { 999.99m, 1000.01m })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                PrepararCuotaPendiente(datos);

                Assert.IsFalse(CrearServicio(datos).RegistrarCobro(1, 10, importe, "Efectivo"), "Importe " + importe);
                AssertSinCambios(datos, 0, 0);
            }
        }

        [TestMethod]
        public void RegistrarCobro_NotaVentaPrevia_ImporteIgualAlMinimo_NoDuplicaNotaVentaNiDebito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararNotaVentaPrevia(datos);
            CobroServicio servicio = CrearServicio(datos);

            Assert.AreEqual(1000m, servicio.ObtenerResumenCobro(1).MinimoExigible);
            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 1));
            Assert.AreEqual(1, datos.Movimientos.Count(m => m.TipoMovimientoId == 2));
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(EstadosCliente.Activo, datos.CuotasCliente[0].Estado);
        }

        [TestMethod]
        public void RegistrarCobro_NotaVentaPrevia_ImporteInferiorOSuperior_RechazaSinCambios()
        {
            foreach (decimal importe in new[] { 999m, 1001m })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                PrepararNotaVentaPrevia(datos);

                Assert.IsFalse(CrearServicio(datos).RegistrarCobro(1, 10, importe, "Efectivo"), "Importe " + importe);
                AssertSinCambios(datos, 1, 1);
            }
        }

        [TestMethod]
        public void RegistrarCobro_NotaVentaHuerfana_ImporteInferiorOSuperior_RechazaSinCambios()
        {
            foreach (decimal importe in new[] { 999m, 1001m })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                PrepararNotaVentaHuerfana(datos);

                Assert.IsFalse(CrearServicio(datos).RegistrarCobro(1, 10, importe, "Efectivo"), "Importe " + importe);
                AssertSinCambios(datos, 1, 0);
            }
        }

        [TestMethod]
        public void RegistrarCobro_ImporteConMasDeDosDecimales_RechazaPorNoSerIgualAlMinimo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);

            Assert.IsFalse(CrearServicio(datos).RegistrarCobro(1, 10, 1000.001m, "Efectivo"));
            AssertSinCambios(datos, 0, 0);
        }

        [TestMethod]
        public void RegistrarCobro_SaldoAFavorParcial_SoloAceptaLaDiferenciaExacta()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 400m, new ReciboPago { Numero = "RP-1", FechaEmision = DateTime.Today.AddDays(-1), Total = 400m });
            CobroServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 599m, "Efectivo"));
            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 600m, "Efectivo"));
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
        }

        [TestMethod]
        public void RegistrarCobro_NotaVentaPrevia_ConDeudaAnterior_ExigeElSaldoTotalExacto()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "DEBITO", 500m, new ReciboPago { Numero = "X-1", FechaEmision = DateTime.Today.AddDays(-40), Total = 500m });
            datos.AgregarMovimiento(100, "DEBITO", 1000m, CrearNotaVentaDelPeriodo(datos, 1000m));
            CobroServicio servicio = CrearServicio(datos);

            Assert.AreEqual(1500m, servicio.ObtenerResumenCobro(1).MinimoExigible);
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1600m, "Efectivo"));
            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1500m, "Efectivo"));
        }

        [TestMethod]
        public void ObtenerResumenCobro_CuotaActivaConDeudaPendiente_BloqueaPorCuotaVigenteYRegistrarCobroRechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(10);
            datos.CuotasCliente[0].Estado = EstadosCliente.Activo;
            datos.AgregarMovimiento(100, "DEBITO", 300m, new AjusteCuenta { Numero = "AJ-00000001", FechaEmision = DateTime.Today, Total = 300m, Motivo = "Cargo", EmpleadoId = 10 });
            CobroServicio servicio = CrearServicio(datos);

            ResumenCobro resumen = servicio.ObtenerResumenCobro(1);

            Assert.IsTrue(resumen.MinimoExigible > 0m);
            Assert.IsFalse(resumen.PermiteRegistrarCobro);
            Assert.AreEqual(MotivoBloqueoCobro.CuotaVigente, resumen.MotivoBloqueo);
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, resumen.MinimoExigible, "Efectivo"));
            Assert.AreEqual(1, datos.Comprobantes.Count);
            Assert.AreEqual(1, datos.Movimientos.Count);
        }

        [TestMethod]
        public void ObtenerResumenCobro_SaldoAFavorCubreLaCuota_BloqueaPorSaldoAFavorSinHabilitarCobro()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            datos.AgregarMovimiento(100, "CREDITO", 1000m, new ReciboPago { Numero = "RP-1", FechaEmision = DateTime.Today, Total = 1000m });

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.IsFalse(resumen.PermiteRegistrarCobro);
            Assert.AreEqual(MotivoBloqueoCobro.SaldoAFavorCubreCuota, resumen.MotivoBloqueo);
        }

        [TestMethod]
        public void ObtenerResumenCobro_Inconsistencia_BloqueaPorInconsistenciaContable()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            NotaVenta nv = CrearNotaVentaDelPeriodo(datos, 1000m);
            nv.Id = 1;
            datos.Comprobantes.Add(nv);
            datos.Movimientos.Add(new Movimiento { Id = 1, Fecha = DateTime.Today, Importe = 1000m, TipoMovimientoId = 1, ComprobanteId = 1, CuentaCorrienteId = 999 });

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(MotivoBloqueoCobro.InconsistenciaContable, resumen.MotivoBloqueo);
            Assert.IsFalse(resumen.PermiteRegistrarCobro);
        }

        [TestMethod]
        public void ObtenerResumenCobro_CuotaPendienteSinDeudaNiCredito_PermiteCobro()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.IsTrue(resumen.PermiteRegistrarCobro);
            Assert.AreEqual(MotivoBloqueoCobro.Ninguno, resumen.MotivoBloqueo);
            Assert.AreEqual(EstadosCliente.Pendiente, resumen.EstadoCuota);
        }

        [TestMethod]
        public void RegistrarCobro_AjusteDebitoPrevio_ElMinimoIncluyeElAjusteYSeCobraExacto()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            Assert.IsTrue(CrearAjusteServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 300m, "Cargo omitido"));
            CobroServicio servicio = CrearServicio(datos);

            ResumenCobro resumen = servicio.ObtenerResumenCobro(1);
            Assert.AreEqual(300m, resumen.Saldo);
            Assert.AreEqual(1300m, resumen.MinimoExigible);
            Assert.IsFalse(resumen.TieneInconsistenciaContable);

            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 1300m, "Efectivo"));
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(1, datos.Comprobantes.OfType<NotaVenta>().Count());
            Assert.AreEqual(1, datos.Comprobantes.OfType<AjusteCuenta>().Count());
        }

        [TestMethod]
        public void RegistrarCobro_AjusteCreditoPrevio_ElMinimoSeReduceYSeCobraExacto()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            Assert.IsTrue(CrearAjusteServicio(datos).RegistrarAjuste(1, 10, "CREDITO", 400m, "Bonificación"));
            CobroServicio servicio = CrearServicio(datos);

            Assert.AreEqual(-400m, servicio.ObtenerResumenCobro(1).Saldo);
            Assert.AreEqual(600m, servicio.ObtenerResumenCobro(1).MinimoExigible);
            Assert.IsFalse(servicio.RegistrarCobro(1, 10, 1000m, "Efectivo"));
            Assert.IsTrue(servicio.RegistrarCobro(1, 10, 600m, "Efectivo"));
            Assert.AreEqual(0m, servicio.ObtenerResumenCobro(1).Saldo);
        }

        [TestMethod]
        public void ObtenerResumenCobro_AjusteSobreNotaVentaContabilizada_NoGeneraInconsistenciaYSumaAlMinimo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararNotaVentaPrevia(datos);
            Assert.IsTrue(CrearAjusteServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 200m, "Recargo"));

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.IsFalse(resumen.TieneInconsistenciaContable);
            Assert.IsTrue(resumen.NotaVentaContabilizada);
            Assert.AreEqual(1200m, resumen.MinimoExigible);
        }

        [TestMethod]
        public void ObtenerResumenCobro_AjusteCreditoQueCubreLaCuota_DejaMinimoCeroYBloqueaCobro()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            Assert.IsTrue(CrearAjusteServicio(datos).RegistrarAjuste(1, 10, "CREDITO", 1000m, "Regularización"));

            ResumenCobro resumen = CrearServicio(datos).ObtenerResumenCobro(1);

            Assert.AreEqual(0m, resumen.MinimoExigible);
            Assert.AreEqual(MotivoBloqueoCobro.SaldoAFavorCubreCuota, resumen.MotivoBloqueo);
        }

        private AjusteCuentaServicio CrearAjusteServicio(DatosEnMemoria datos)
        {
            return new AjusteCuentaServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new AjusteCuentaRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos());
        }
    }
}