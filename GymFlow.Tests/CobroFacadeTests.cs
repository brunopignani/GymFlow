using GymFlow.Abstracciones;
using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class CobroFacadeTests
    {
        private CobroFacade CrearFacade(DatosEnMemoria datos, IContextoUsuarioActual contexto = null)
        {
            IContextoUsuarioActual ctx = contexto ?? ContextoUsuarioEnMemoria.ConTodosLosPermisos();

            ClienteServicio clienteServicio = new ClienteServicio(
                new ClienteRepositorioEnMemoria(datos),
                new PersonaRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new AuditoriaRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                ctx);

            DebitoMensualServicio debitoMensualServicio = new DebitoMensualServicio(
                new ClienteRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                ctx);

            CobroServicio cobroServicio = new CobroServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new TipoMovimientoRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                debitoMensualServicio,
                ctx);

            CuentaCorrienteServicio cuentaCorrienteServicio = new CuentaCorrienteServicio(
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                ctx);

            return new CobroFacade(clienteServicio, cobroServicio, cuentaCorrienteServicio);
        }

        private static void PrepararCuotaPendiente(DatosEnMemoria datos)
        {
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
        }

        [TestMethod]
        public void BuscarClienteParaCobro_ClienteExistente_RetornaClienteYResumen()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroFacade facade = CrearFacade(datos);

            ResultadoConsultaCobro consulta = facade.BuscarClienteParaCobro(10111222);

            Assert.IsNotNull(consulta);
            Assert.IsTrue(consulta.ClienteEncontrado);
            Assert.IsNotNull(consulta.Cliente);
            Assert.AreEqual(1, consulta.Cliente.Id);
            Assert.AreEqual("Ana", consulta.Cliente.Nombre);
            Assert.IsNotNull(consulta.Resumen);
            Assert.AreEqual(1000m, consulta.Resumen.MinimoExigible);
            Assert.IsTrue(consulta.Resumen.PermiteRegistrarCobro);
        }

        [TestMethod]
        public void BuscarClienteParaCobro_ClienteInexistente_RetornaNoEncontrado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroFacade facade = CrearFacade(datos);

            ResultadoConsultaCobro consulta = facade.BuscarClienteParaCobro(99999999);

            Assert.IsNotNull(consulta);
            Assert.IsFalse(consulta.ClienteEncontrado);
            Assert.IsNull(consulta.Cliente);
            Assert.IsNull(consulta.Resumen);
        }

        [TestMethod]
        public void BuscarClienteParaCobro_SinPermisoClientes_RetornaNoEncontrado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroFacade facade = CrearFacade(datos, ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.CobroRegistrar));

            ResultadoConsultaCobro consulta = facade.BuscarClienteParaCobro(10111222);

            Assert.IsNotNull(consulta);
            Assert.IsFalse(consulta.ClienteEncontrado);
            Assert.IsNull(consulta.Cliente);
            Assert.IsNull(consulta.Resumen);
        }

        [TestMethod]
        public void RegistrarCobro_DatosValidos_RegistraCobroRetornaReciboYResumenActualizado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroFacade facade = CrearFacade(datos);

            ResultadoRegistroCobro resultado = facade.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.Registrado);
            Assert.IsNotNull(resultado.Recibo);
            Assert.AreEqual("RP-00000001", resultado.Recibo.Numero);
            Assert.AreEqual(1000m, resultado.Recibo.Total);
            Assert.AreEqual("Efectivo", resultado.Recibo.MetodoPago);
            Assert.AreEqual(10, resultado.Recibo.EmpleadoId);
            Assert.AreEqual(DateTime.Today, resultado.Recibo.FechaEmision);

            Assert.IsNotNull(resultado.ResumenActualizado);
            Assert.AreEqual(0m, resultado.ResumenActualizado.Saldo);
            Assert.IsFalse(resultado.ResumenActualizado.PermiteRegistrarCobro);
            Assert.AreEqual(MotivoBloqueoCobro.CuotaVigente, resultado.ResumenActualizado.MotivoBloqueo);
        }

        [TestMethod]
        public void RegistrarCobro_CuotaVigente_RechazaYRetornaResumenConMotivoBloqueo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(15);
            datos.CuotasCliente[0].Estado = EstadosCliente.Activo;
            CobroFacade facade = CrearFacade(datos);

            ResultadoRegistroCobro resultado = facade.RegistrarCobro(1, 10, 1000m, "Efectivo");

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.Registrado);
            Assert.IsNull(resultado.Recibo);
            Assert.IsNotNull(resultado.ResumenActualizado);
            Assert.IsFalse(resultado.ResumenActualizado.PermiteRegistrarCobro);
            Assert.AreEqual(MotivoBloqueoCobro.CuotaVigente, resultado.ResumenActualizado.MotivoBloqueo);
            Assert.AreEqual(0, datos.Comprobantes.OfType<ReciboPago>().Count());
        }

        [TestMethod]
        public void RegistrarCobro_ImporteIncorrecto_RechazaSinGenerarRecibo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroFacade facade = CrearFacade(datos);

            ResultadoRegistroCobro resultado = facade.RegistrarCobro(1, 10, 500m, "Efectivo");

            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado.Registrado);
            Assert.IsNull(resultado.Recibo);
            Assert.IsNotNull(resultado.ResumenActualizado);
            Assert.IsTrue(resultado.ResumenActualizado.PermiteRegistrarCobro);
            Assert.AreEqual(1000m, resultado.ResumenActualizado.MinimoExigible);
            Assert.AreEqual(0, datos.Comprobantes.OfType<ReciboPago>().Count());
        }

        [TestMethod]
        public void RegistrarCobro_SinPermisoCuentaCorriente_RegistraCobroConReciboNulo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            PrepararCuotaPendiente(datos);
            CobroFacade facade = CrearFacade(
                datos,
                ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.ClientesGestionar, CodigosPermiso.CobroRegistrar));

            ResultadoRegistroCobro resultado = facade.RegistrarCobro(1, 10, 1000m, "Transferencia");

            Assert.IsNotNull(resultado);
            Assert.IsTrue(resultado.Registrado);
            Assert.IsNull(resultado.Recibo);
            Assert.IsNotNull(resultado.ResumenActualizado);
            Assert.AreEqual(0m, resultado.ResumenActualizado.Saldo);
            Assert.AreEqual(1, datos.Comprobantes.OfType<ReciboPago>().Count());
        }

        [TestMethod]
        public void ObtenerUltimoRecibo_ConVariosRecibos_RetornaElDeMayorId()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.AgregarMovimiento(100, NombresTipoMovimiento.Credito, 1000m, new ReciboPago
            {
                Numero = "RP-00000001",
                FechaEmision = DateTime.Today.AddDays(-30),
                Total = 1000m,
                MetodoPago = "Efectivo",
                EmpleadoId = 10
            });
            datos.AgregarMovimiento(100, NombresTipoMovimiento.Debito, 1000m, new NotaVenta
            {
                Numero = "NV-00000001",
                FechaEmision = DateTime.Today,
                Total = 1000m,
                CuotaClienteId = 5,
                Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            });
            datos.AgregarMovimiento(100, NombresTipoMovimiento.Credito, 1000m, new ReciboPago
            {
                Numero = "RP-00000002",
                FechaEmision = DateTime.Today,
                Total = 1000m,
                MetodoPago = "Transferencia",
                EmpleadoId = 10
            });

            CobroFacade facade = CrearFacade(datos);

            ReciboPago ultimo = facade.ObtenerUltimoRecibo(1);

            Assert.IsNotNull(ultimo);
            Assert.AreEqual("RP-00000002", ultimo.Numero);
            Assert.AreEqual("Transferencia", ultimo.MetodoPago);
        }

        [TestMethod]
        public void ObtenerUltimoRecibo_SinRecibos_RetornaNull()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroFacade facade = CrearFacade(datos);

            ReciboPago ultimo = facade.ObtenerUltimoRecibo(1);

            Assert.IsNull(ultimo);
        }

        [TestMethod]
        public void ObtenerMediosPagoValidos_RetornaMediosDefinidosEnCobroServicio()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            CobroFacade facade = CrearFacade(datos);

            IReadOnlyList<string> medios = facade.ObtenerMediosPagoValidos();

            CollectionAssert.AreEqual(CobroServicio.MediosPagoValidos, medios.ToList());
        }
    }
}
