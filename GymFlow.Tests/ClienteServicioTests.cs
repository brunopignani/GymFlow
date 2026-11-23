using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class ClienteServicioTests
    {
        private ClienteServicio CrearServicio(DatosEnMemoria datos)
        {
            return new ClienteServicio(
                new ClienteRepositorioEnMemoria(datos),
                new PersonaRepositorioEnMemoria(datos),
                new CuotaClienteRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos),
                new CuotaRepositorioEnMemoria(datos),
                new ComprobanteRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        private Cliente NuevoCliente(int dni)
        {
            return new Cliente { Dni = dni, Nombre = "Luis", Apellido = "Sol", Telefono = "1234567" };
        }

        [TestMethod]
        public void AddCliente_DatosValidos_GuardaElClienteYCreaSuCuentaCorriente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            bool resultado = servicio.AddCliente(NuevoCliente(20111222));

            Assert.IsTrue(resultado);
            Cliente guardado = datos.Clientes.Single(c => c.Dni == 20111222);
            Assert.IsTrue(datos.Cuentas.Any(cc => cc.ClienteId == guardado.Id));
            CuotaCliente cuotaCliente = datos.CuotasCliente.Single(cc => cc.ClienteId == guardado.Id);
            Assert.AreEqual(1, cuotaCliente.CuotaId);
            Assert.IsNull(cuotaCliente.FechaVencimiento);
            Assert.IsNull(cuotaCliente.Estado);
            Assert.AreEqual(0, datos.Movimientos.Count);
        }

        [TestMethod]
        public void AddCliente_AltaRepetida_NoDuplicaLaCuotaAsignada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente cliente = NuevoCliente(20111222);

            Assert.IsTrue(servicio.AddCliente(cliente));
            Assert.IsFalse(servicio.AddCliente(cliente));

            Assert.AreEqual(1, datos.CuotasCliente.Count(cc => cc.ClienteId == cliente.Id));
        }

        [TestMethod]
        public void AddCliente_CuotaMensualNoUnica_NoGuardaElCliente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Cuotas.Add(new Cuota { Id = 2, Nombre = "Anual", Precio = 10000m });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(20111222)));
            Assert.IsFalse(datos.Clientes.Any(c => c.Dni == 20111222));
        }

        [TestMethod]
        public void AddCliente_DniDuplicado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(10111222)));
            Assert.AreEqual(1, datos.Clientes.Count);
        }

        [TestMethod]
        public void AddCliente_DniInvalido_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(0)));
            Assert.IsFalse(servicio.AddCliente(NuevoCliente(-5)));
        }

        [TestMethod]
        public void AddCliente_NombreOApellidoVacios_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente sinNombre = NuevoCliente(20111222);
            sinNombre.Nombre = " ";
            Cliente sinApellido = NuevoCliente(30111222);
            sinApellido.Apellido = "";

            Assert.IsFalse(servicio.AddCliente(sinNombre));
            Assert.IsFalse(servicio.AddCliente(sinApellido));
        }

        [TestMethod]
        public void AddCliente_Null_RetornaFalse()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.AddCliente(null));
        }

        [TestMethod]
        public void AddCliente_DniFueraDeRango_LongitudONombreExcesivo_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(999999)));
            Assert.IsFalse(servicio.AddCliente(NuevoCliente(100000000)));

            Cliente nombreLargo = NuevoCliente(20111222);
            nombreLargo.Nombre = new string('A', ValidacionesDatos.LongitudMaximaNombre + 1);
            Assert.IsFalse(servicio.AddCliente(nombreLargo));
        }

        [TestMethod]
        public void AddCliente_TelefonoConFormatoInvalido_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente cliente = NuevoCliente(20111222);
            cliente.Telefono = "12ABC";

            Assert.IsFalse(servicio.AddCliente(cliente));
        }

        [TestMethod]
        public void AddCliente_TelefonoVacio_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente cliente = NuevoCliente(20111222);
            cliente.Telefono = " ";

            Assert.IsFalse(servicio.AddCliente(cliente));
        }

        [TestMethod]
        public void UpdateCliente_DatosValidos_ActualizaElCliente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente copia = new Cliente { Id = 1, Dni = 10111222, Nombre = "Ana María", Apellido = "Paz", Telefono = "9999999" };

            Assert.IsTrue(servicio.UpdateCliente(copia));
            Assert.AreEqual("Ana María", datos.Clientes[0].Nombre);
            Assert.AreEqual("9999999", datos.Clientes[0].Telefono);
        }

        [TestMethod]
        public void UpdateCliente_DniDeOtroCliente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            servicio.AddCliente(NuevoCliente(20111222));
            Cliente copia = new Cliente { Id = datos.Clientes.Single(c => c.Dni == 20111222).Id, Dni = 10111222, Nombre = "Luis", Apellido = "Sol" };

            Assert.IsFalse(servicio.UpdateCliente(copia));
        }

        [TestMethod]
        public void UpdateCliente_ClienteInexistente_RetornaFalse()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.UpdateCliente(new Cliente { Id = 99, Dni = 555, Nombre = "X", Apellido = "Y" }));
        }

        [TestMethod]
        public void UpdateCliente_DatosInvalidos_RetornaFalseYNoModifica()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.UpdateCliente(new Cliente { Id = 1, Dni = 10111222, Nombre = "", Apellido = "Paz" }));
            Assert.AreEqual("Ana", datos.Clientes[0].Nombre);
        }

        [TestMethod]
        public void DeleteCliente_SinHistorial_EliminaElClienteYSuCuenta()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            servicio.AddCliente(NuevoCliente(20111222));
            int id = datos.Clientes.Single(c => c.Dni == 20111222).Id;

            Assert.IsTrue(servicio.DeleteCliente(id));
            Assert.IsFalse(datos.Clientes.Any(c => c.Id == id));
            Assert.IsFalse(datos.Cuentas.Any(cc => cc.ClienteId == id));
            Assert.IsFalse(datos.CuotasCliente.Any(cc => cc.ClienteId == id));
            Assert.AreEqual(IsolationLevel.Serializable, datos.UltimoNivelAislamiento);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void DeleteCliente_ConCuotaAsignada_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteCliente(1));
            Assert.AreEqual(1, datos.Clientes.Count);
        }

        [TestMethod]
        public void DeleteCliente_ConVariasCuotasPendientesIniciales_PermiteLaBaja()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 1, CuotaId = 1 });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.DeleteCliente(1));
            Assert.IsFalse(datos.Clientes.Any(c => c.Id == 1));
            Assert.IsFalse(datos.CuotasCliente.Any(cc => cc.ClienteId == 1));
        }

        [TestMethod]
        public void DeleteCliente_ConCuotaVencida_RechazaLaBaja()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.AreEqual(ResultadoBajaCliente.CuotasNoIniciales, servicio.DeleteClienteConResultado(1));
        }

        [TestMethod]
        public void DeleteCliente_ConCuotaActiva_RechazaLaBaja()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(1);
            datos.CuotasCliente[0].Estado = "ACTIVO";
            ClienteServicio servicio = CrearServicio(datos);

            Assert.AreEqual(ResultadoBajaCliente.CuotasNoIniciales, servicio.DeleteClienteConResultado(1));
        }

        [TestMethod]
        public void DeleteCliente_ConInscripciones_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            datos.Inscripciones.Add(new Inscripcion { Id = 1, ClienteId = 1, ActividadId = 1, Fecha = DateTime.Today });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteCliente(1));
        }

        [TestMethod]
        public void DeleteCliente_ConMovimientosEnLaCuenta_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            datos.AgregarMovimiento(100, "DEBITO", 500m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today, Total = 500m });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteCliente(1));
            Assert.AreEqual(1, datos.Clientes.Count);
        }

        [TestMethod]
        public void DeleteCliente_ConComprobanteFinanciero_RechazaLaBaja()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            datos.Comprobantes.Add(new NotaVenta { Id = 1, CuotaClienteId = 5, Numero = "NV-1", FechaEmision = DateTime.Today, Total = 1000m });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.AreEqual(ResultadoBajaCliente.Comprobantes, servicio.DeleteClienteConResultado(1));
        }

        [TestMethod]
        public void DeleteCliente_ConUsuarioAsociado_RechazaLaBaja()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes[0].UsuarioId = 7;
            ClienteServicio servicio = CrearServicio(datos);

            Assert.AreEqual(ResultadoBajaCliente.UsuarioAsociado, servicio.DeleteClienteConResultado(1));
        }

        [TestMethod]
        public void DeleteCliente_FallaDuranteLaEliminacion_RevierteLosCambios()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = null;
            datos.FallarAlEliminarCliente = true;
            ClienteServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.DeleteCliente(1));
            Assert.IsTrue(datos.Clientes.Any(c => c.Id == 1));
            Assert.IsTrue(datos.Cuentas.Any(cc => cc.ClienteId == 1));
            Assert.IsTrue(datos.CuotasCliente.Any(cc => cc.ClienteId == 1));
            Assert.AreEqual(1, datos.Revertidas);
        }

        [TestMethod]
        public void DeleteCliente_Inexistente_RetornaFalse()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.DeleteCliente(99));
            Assert.AreEqual(ResultadoBajaCliente.ClienteInexistente, servicio.DeleteClienteConResultado(99));
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaVigente_PermiteElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(1);
            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.SocioEncontrado);
            Assert.IsTrue(resultado.AccesoPermitido);
            Assert.AreEqual("Ana Paz", resultado.NombreCompleto);
            Assert.AreEqual(10111222, resultado.Dni);
            Assert.AreEqual(EstadosCliente.Activo, resultado.EstadoCuota);
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaQueVenceHoy_PermiteElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today;
            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.SocioEncontrado);
            Assert.IsTrue(resultado.AccesoPermitido);
            Assert.AreEqual(EstadosCliente.Activo, resultado.EstadoCuota);
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaVencida_DeniegaElAcceso()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.SocioEncontrado);
            Assert.IsFalse(resultado.AccesoPermitido);
            Assert.AreEqual(EstadosCliente.Vencido, resultado.EstadoCuota);
            Assert.AreEqual("Ana Paz", resultado.NombreCompleto);
            Assert.AreEqual(10111222, resultado.Dni);
        }

        [TestMethod]
        public void ValidarAutoingreso_DniInexistente_DeniegaElAcceso()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(999);

            Assert.IsFalse(resultado.SocioEncontrado);
            Assert.IsFalse(resultado.AccesoPermitido);
            Assert.AreEqual(0, resultado.Actividades.Count);
        }

        [TestMethod]
        public void ValidarAutoingreso_ClienteSinCuota_DeniegaElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.SocioEncontrado);
            Assert.IsFalse(resultado.AccesoPermitido);
            Assert.AreEqual(EstadosCliente.SinCuota, resultado.EstadoCuota);
        }

        [TestMethod]
        public void ValidarAutoingreso_ClienteConInscripciones_RetornaActividadesCorrectamente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(5);
            datos.Actividades.Add(new Actividad { Id = 2, Nombre = "Spinning", CapacidadMaxima = 20 });
            datos.Inscripciones.Add(new Inscripcion { Id = 1, ClienteId = 1, ActividadId = 1 });
            datos.Inscripciones.Add(new Inscripcion { Id = 2, ClienteId = 1, ActividadId = 2 });

            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.AccesoPermitido);
            Assert.AreEqual(2, resultado.Actividades.Count);
            CollectionAssert.Contains(resultado.Actividades, "Yoga");
            CollectionAssert.Contains(resultado.Actividades, "Spinning");
        }

        [TestMethod]
        public void ValidarAutoingreso_ClienteSinInscripciones_RetornaListaVacia()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(5);
            datos.Inscripciones.Clear();

            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.AccesoPermitido);
            Assert.IsNotNull(resultado.Actividades);
            Assert.AreEqual(0, resultado.Actividades.Count);
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaPendiente_DeniegaPeroRetornaDatosYEstadoPendiente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            datos.CuotasCliente[0].Estado = EstadosCliente.Pendiente;
            datos.Inscripciones.Add(new Inscripcion { Id = 1, ClienteId = 1, ActividadId = 1 });

            ClienteServicio servicio = CrearServicio(datos);

            ResultadoAutoingreso resultado = servicio.ValidarAutoingreso(10111222);

            Assert.IsTrue(resultado.SocioEncontrado);
            Assert.IsFalse(resultado.AccesoPermitido);
            Assert.AreEqual(EstadosCliente.Pendiente, resultado.EstadoCuota);
            Assert.IsNull(resultado.FechaVencimiento);
            Assert.AreEqual("Ana Paz", resultado.NombreCompleto);
            Assert.AreEqual(1, resultado.Actividades.Count);
            Assert.AreEqual("Yoga", resultado.Actividades[0]);
        }

        [TestMethod]
        public void GetEstadoClientes_ClienteSinCuotas_EsSinCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            ClienteServicio servicio = CrearServicio(datos);

            EstadoCliente estado = servicio.GetEstadoClientes().Single();

            Assert.AreEqual(EstadosCliente.SinCuota, estado.Estado);
            Assert.IsFalse(estado.Vigente);
        }

        [TestMethod]
        public void GetEstadoClientes_CuotaPendienteSinVencimiento_EsPendiente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = null;
            ClienteServicio servicio = CrearServicio(datos);

            EstadoCliente estado = servicio.GetEstadoClientes().Single();

            Assert.AreEqual(EstadosCliente.Pendiente, estado.Estado);
            Assert.IsFalse(estado.Vigente);
        }

        [TestMethod]
        public void GetEstadoClientes_CuotaQueVenceHoy_EsActivo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today;
            ClienteServicio servicio = CrearServicio(datos);

            EstadoCliente estado = servicio.GetEstadoClientes().Single();

            Assert.AreEqual(EstadosCliente.Activo, estado.Estado);
            Assert.IsTrue(estado.Vigente);
        }

        [TestMethod]
        public void GetEstadoClientes_CuotaVencida_EsVencido()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(-1);
            ClienteServicio servicio = CrearServicio(datos);

            EstadoCliente estado = servicio.GetEstadoClientes().Single();

            Assert.AreEqual(EstadosCliente.Vencido, estado.Estado);
            Assert.IsFalse(estado.Vigente);
        }

        [TestMethod]
        public void GetEstadoClientes_CuotaPendienteMasReciente_PriorizaLaUltimaAsignacion()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Add(new CuotaCliente
            {
                Id = 6,
                ClienteId = 1,
                CuotaId = 1,
                FechaVencimiento = null
            });
            ClienteServicio servicio = CrearServicio(datos);

            EstadoCliente estado = servicio.GetEstadoClientes().Single();

            Assert.AreEqual(6, estado.CuotaCliente.Id);
            Assert.AreEqual(EstadosCliente.Pendiente, estado.Estado);
        }

        [TestMethod]
        public void GetCuotaActivaByCliente_SeleccionaLaDeMayorIdAunqueSuFechaSeaMenor()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Add(new CuotaCliente
            {
                Id = 6,
                ClienteId = 1,
                CuotaId = 1,
                FechaVencimiento = DateTime.Today.AddDays(-10)
            });
            ClienteServicio servicio = CrearServicio(datos);

            CuotaCliente cuota = servicio.GetCuotaActivaByCliente(1);

            Assert.AreEqual(6, cuota.Id);
        }

        [TestMethod]
        public void GetEstadoClientes_ClasificaVigentesVencidosYSinCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 20111222, Nombre = "Luis", Apellido = "Sol" });
            datos.Clientes.Add(new Cliente { Id = 3, Dni = 30111222, Nombre = "Eva", Apellido = "Rio" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(10) });
            ClienteServicio servicio = CrearServicio(datos);

            var estados = servicio.GetEstadoClientes();

            EstadoCliente ana = estados.Single(e => e.Cliente.Dni == 10111222);
            EstadoCliente luis = estados.Single(e => e.Cliente.Dni == 20111222);
            EstadoCliente eva = estados.Single(e => e.Cliente.Dni == 30111222);
            Assert.IsTrue(ana.TieneCuota);
            Assert.AreEqual(EstadosCliente.Vencido, ana.Estado);
            Assert.IsFalse(ana.Vigente);
            Assert.IsTrue(luis.TieneCuota);
            Assert.AreEqual(EstadosCliente.Activo, luis.Estado);
            Assert.IsTrue(luis.Vigente);
            Assert.IsFalse(eva.TieneCuota);
            Assert.AreEqual(EstadosCliente.SinCuota, eva.Estado);
            Assert.IsFalse(eva.Vigente);
        }

        [TestMethod]
        public void GetEstadoClientes_ConVariasCuotasHistoricasPorCliente_SeleccionaLaDeMayorIdParaCadaUno()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 1, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(15), Estado = EstadosCliente.Activo });
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 20111222, Nombre = "Luis", Apellido = "Sol" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 7, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(20), Estado = EstadosCliente.Activo });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 8, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(-1), Estado = EstadosCliente.Vencido });
            ClienteServicio servicio = CrearServicio(datos);

            var estados = servicio.GetEstadoClientes();

            EstadoCliente ana = estados.Single(e => e.Cliente.Id == 1);
            EstadoCliente luis = estados.Single(e => e.Cliente.Id == 2);
            Assert.AreEqual(6, ana.CuotaCliente.Id);
            Assert.AreEqual(EstadosCliente.Activo, ana.Estado);
            Assert.AreEqual(8, luis.CuotaCliente.Id);
            Assert.AreEqual(EstadosCliente.Vencido, luis.Estado);
        }

        [TestMethod]
        public void AddCliente_DniRegistradoPorEmpleado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Empleados.Add(new Empleado { Id = 11, Dni = 30111222, Nombre = "Marta", Apellido = "Gomez", Telefono = "5551234" });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(30111222)));
            Assert.AreEqual(1, datos.Clientes.Count);
        }

        [TestMethod]
        public void UpdateCliente_ConservaSuPropioDni_RetornaTrue()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente actualizado = new Cliente { Id = 1, Dni = 10111222, Nombre = "Ana Maria", Apellido = "Paz", Telefono = "1234567" };

            Assert.IsTrue(servicio.UpdateCliente(actualizado));
            Assert.AreEqual("Ana Maria", datos.Clientes[0].Nombre);
        }

        [TestMethod]
        public void UpdateCliente_DniRegistradoPorEmpleado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Empleados.Add(new Empleado { Id = 11, Dni = 30111222, Nombre = "Marta", Apellido = "Gomez", Telefono = "5551234" });
            ClienteServicio servicio = CrearServicio(datos);
            Cliente actualizado = new Cliente { Id = 1, Dni = 30111222, Nombre = "Ana", Apellido = "Paz", Telefono = "1234567" };

            Assert.IsFalse(servicio.UpdateCliente(actualizado));
            Assert.AreEqual(10111222, datos.Clientes[0].Dni);
        }
    }
}
