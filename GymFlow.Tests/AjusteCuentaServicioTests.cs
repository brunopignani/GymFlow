using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class AjusteCuentaServicioTests
    {
        private AjusteCuentaServicio CrearServicio(DatosEnMemoria datos)
        {
            return new AjusteCuentaServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new AjusteCuentaRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos());
        }

        [TestMethod]
        public void RegistrarAjuste_Debito_GeneraComprobanteYUnSoloMovimientoDebito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();

            bool resultado = CrearServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 250.50m, "  Cargo omitido  ");

            Assert.IsTrue(resultado);

            AjusteCuenta ajuste = datos.Comprobantes.OfType<AjusteCuenta>().Single();
            Assert.AreEqual("AJ-00000001", ajuste.Numero);
            Assert.AreEqual(250.50m, ajuste.Total);
            Assert.AreEqual("Cargo omitido", ajuste.Motivo);
            Assert.AreEqual(10, ajuste.EmpleadoId);
            Assert.AreEqual(DateTime.Today, ajuste.FechaEmision);

            Movimiento movimiento = datos.Movimientos.Single();
            Assert.AreEqual(1, movimiento.TipoMovimientoId);
            Assert.AreEqual(250.50m, movimiento.Importe);
            Assert.AreEqual(ajuste.Id, movimiento.ComprobanteId);
            Assert.AreEqual(100, movimiento.CuentaCorrienteId);

            Assert.AreEqual(1, datos.Confirmadas);
            Assert.AreEqual(0, datos.Revertidas);
            Assert.AreEqual(IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
            Assert.AreEqual(250.50m, new CuentaCorrienteServicio(new CuentaCorrienteRepositorioEnMemoria(datos), new MovimientoRepositorioEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos()).ConsultarCuentaCorriente(1).Saldo);
        }

        [TestMethod]
        public void RegistrarAjuste_Credito_GeneraComprobanteYUnSoloMovimientoCredito()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();

            Assert.IsTrue(CrearServicio(datos).RegistrarAjuste(1, 10, "CREDITO", 400m, "Recibo duplicado"));

            Movimiento movimiento = datos.Movimientos.Single();
            Assert.AreEqual(2, movimiento.TipoMovimientoId);
            Assert.AreEqual(400m, movimiento.Importe);
            Assert.AreEqual(1, datos.Comprobantes.OfType<AjusteCuenta>().Count());
            Assert.AreEqual(-400m, new CuentaCorrienteServicio(new CuentaCorrienteRepositorioEnMemoria(datos), new MovimientoRepositorioEnMemoria(datos), ContextoUsuarioEnMemoria.ConTodosLosPermisos()).ConsultarCuentaCorriente(1).Saldo);
        }

        [TestMethod]
        public void RegistrarAjuste_VariosAjustes_NumeraSecuencialmenteYNoModificaMovimientosPrevios()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AjusteCuentaServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.RegistrarAjuste(1, 10, "DEBITO", 100m, "Uno"));
            Movimiento primero = datos.Movimientos[0];
            decimal importePrimero = primero.Importe;
            Assert.IsTrue(servicio.RegistrarAjuste(1, 10, "CREDITO", 100m, "Dos"));

            Assert.AreEqual("AJ-00000001", datos.Comprobantes[0].Numero);
            Assert.AreEqual("AJ-00000002", datos.Comprobantes[1].Numero);
            Assert.AreEqual(2, datos.Movimientos.Count);
            Assert.AreEqual(importePrimero, datos.Movimientos[0].Importe);
            Assert.AreEqual(1, datos.Movimientos[0].TipoMovimientoId);
        }

        [TestMethod]
        public void RegistrarAjuste_MotivoVacio_RechazaSinIniciarTransaccion()
        {
            foreach (string motivo in new[] { null, string.Empty, "   " })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();

                Assert.IsFalse(CrearServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 100m, motivo));
                Assert.AreEqual(0, datos.TransaccionesIniciadas);
                Assert.AreEqual(0, datos.Comprobantes.Count);
                Assert.AreEqual(0, datos.Movimientos.Count);
            }
        }

        [TestMethod]
        public void RegistrarAjuste_MotivoMasLargoQueElMaximo_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            string motivo = new string('a', AjusteCuentaServicio.LongitudMaximaMotivo + 1);

            Assert.IsFalse(CrearServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 100m, motivo));
            Assert.AreEqual(0, datos.TransaccionesIniciadas);
        }

        [TestMethod]
        public void RegistrarAjuste_ImporteInvalido_RechazaSinIniciarTransaccion()
        {
            foreach (decimal importe in new[] { 0m, -1m, -100m, 10.005m })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();

                Assert.IsFalse(CrearServicio(datos).RegistrarAjuste(1, 10, "CREDITO", importe, "Motivo"), "Importe " + importe);
                Assert.AreEqual(0, datos.TransaccionesIniciadas);
                Assert.AreEqual(0, datos.Comprobantes.Count);
                Assert.AreEqual(0, datos.Movimientos.Count);
            }
        }

        [TestMethod]
        public void RegistrarAjuste_TipoDesconocido_Rechaza()
        {
            foreach (string tipo in new[] { null, "", "debito", "OTRO" })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();

                Assert.IsFalse(CrearServicio(datos).RegistrarAjuste(1, 10, tipo, 100m, "Motivo"));
                Assert.AreEqual(0, datos.TransaccionesIniciadas);
            }
        }

        [TestMethod]
        public void RegistrarAjuste_EmpleadoClienteOTipoInexistente_RevierteSinCrearRegistros()
        {
            DatosEnMemoria sinEmpleado = DatosEnMemoria.CrearBase();
            Assert.IsFalse(CrearServicio(sinEmpleado).RegistrarAjuste(1, 99, "DEBITO", 100m, "Motivo"));
            Assert.AreEqual(1, sinEmpleado.Revertidas);
            Assert.AreEqual(0, sinEmpleado.Comprobantes.Count);

            DatosEnMemoria sinCuenta = DatosEnMemoria.CrearBase();
            Assert.IsFalse(CrearServicio(sinCuenta).RegistrarAjuste(77, 10, "DEBITO", 100m, "Motivo"));
            Assert.AreEqual(1, sinCuenta.Revertidas);
            Assert.AreEqual(0, sinCuenta.Comprobantes.Count);

            DatosEnMemoria sinTipo = DatosEnMemoria.CrearBase();
            sinTipo.Tipos.RemoveAll(t => t.Nombre == "CREDITO");
            Assert.IsFalse(CrearServicio(sinTipo).RegistrarAjuste(1, 10, "CREDITO", 100m, "Motivo"));
            Assert.AreEqual(0, sinTipo.Comprobantes.Count);
            Assert.AreEqual(0, sinTipo.Movimientos.Count);
        }

        [TestMethod]
        public void RegistrarAjuste_ComprobanteRechazado_RevierteYRetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.RechazarGuardarComprobante = true;

            Assert.IsFalse(CrearServicio(datos).RegistrarAjuste(1, 10, "DEBITO", 100m, "Motivo"));
            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
        }

        [TestMethod]
        public void RegistrarAjuste_FallaAlGuardarMovimiento_RevierteElComprobanteYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.FallarAlGuardarMovimiento = true;
            AjusteCuentaServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.RegistrarAjuste(1, 10, "DEBITO", 100m, "Motivo"));

            Assert.AreEqual(0, datos.Confirmadas);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Comprobantes.Count);
            Assert.AreEqual(0, datos.Movimientos.Count);
        }
    }
}
