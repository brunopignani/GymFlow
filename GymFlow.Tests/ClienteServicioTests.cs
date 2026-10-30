using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
                new CuotaClienteRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos),
                new CuentaCorrienteRepositorioEnMemoria(datos),
                new MovimientoRepositorioEnMemoria(datos));
        }

        private Cliente NuevoCliente(int dni)
        {
            return new Cliente { Dni = dni, Nombre = "Luis", Apellido = "Sol", Telefono = "1234" };
        }

        [TestMethod]
        public void AddCliente_DatosValidos_GuardaElClienteYCreaSuCuentaCorriente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            bool resultado = servicio.AddCliente(NuevoCliente(222));

            Assert.IsTrue(resultado);
            Cliente guardado = datos.Clientes.Single(c => c.Dni == 222);
            Assert.IsTrue(datos.Cuentas.Any(cc => cc.ClienteId == guardado.Id));
        }

        [TestMethod]
        public void AddCliente_DniDuplicado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddCliente(NuevoCliente(111)));
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
            Cliente sinNombre = NuevoCliente(222);
            sinNombre.Nombre = " ";
            Cliente sinApellido = NuevoCliente(333);
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
        public void UpdateCliente_DatosValidos_ActualizaElCliente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            Cliente copia = new Cliente { Id = 1, Dni = 111, Nombre = "Ana María", Apellido = "Paz", Telefono = "999" };

            Assert.IsTrue(servicio.UpdateCliente(copia));
            Assert.AreEqual("Ana María", datos.Clientes[0].Nombre);
            Assert.AreEqual("999", datos.Clientes[0].Telefono);
        }

        [TestMethod]
        public void UpdateCliente_DniDeOtroCliente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            servicio.AddCliente(NuevoCliente(222));
            Cliente copia = new Cliente { Id = datos.Clientes.Single(c => c.Dni == 222).Id, Dni = 111, Nombre = "Luis", Apellido = "Sol" };

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

            Assert.IsFalse(servicio.UpdateCliente(new Cliente { Id = 1, Dni = 111, Nombre = "", Apellido = "Paz" }));
            Assert.AreEqual("Ana", datos.Clientes[0].Nombre);
        }

        [TestMethod]
        public void DeleteCliente_SinHistorial_EliminaElClienteYSuCuenta()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ClienteServicio servicio = CrearServicio(datos);
            servicio.AddCliente(NuevoCliente(222));
            int id = datos.Clientes.Single(c => c.Dni == 222).Id;

            Assert.IsTrue(servicio.DeleteCliente(id));
            Assert.IsFalse(datos.Clientes.Any(c => c.Id == id));
            Assert.IsFalse(datos.Cuentas.Any(cc => cc.ClienteId == id));
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
        public void DeleteCliente_ConInscripciones_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            datos.Inscripciones.Add(new Inscripcion { Id = 1, ClienteId = 1, ActividadId = 1, Fecha = DateTime.Today });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteCliente(1));
        }

        [TestMethod]
        public void DeleteCliente_ConMovimientosEnLaCuenta_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            datos.AgregarMovimiento(100, "DEBITO", 500m, new NotaVenta { Numero = "NV-1", FechaEmision = DateTime.Today, Total = 500m });
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteCliente(1));
            Assert.AreEqual(1, datos.Clientes.Count);
        }

        [TestMethod]
        public void DeleteCliente_Inexistente_RetornaFalse()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.DeleteCliente(99));
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaVigente_PermiteElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(1);
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.ValidarAutoingreso(111));
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaQueVenceHoy_PermiteElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today;
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.ValidarAutoingreso(111));
        }

        [TestMethod]
        public void ValidarAutoingreso_CuotaVencida_DeniegaElAcceso()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.ValidarAutoingreso(111));
        }

        [TestMethod]
        public void ValidarAutoingreso_DniInexistente_DeniegaElAcceso()
        {
            ClienteServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.ValidarAutoingreso(999));
        }

        [TestMethod]
        public void ValidarAutoingreso_ClienteSinCuota_DeniegaElAcceso()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            ClienteServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.ValidarAutoingreso(111));
        }

        [TestMethod]
        public void GetEstadoClientes_ClasificaVigentesVencidosYSinCuota()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Clientes.Add(new Cliente { Id = 2, Dni = 222, Nombre = "Luis", Apellido = "Sol" });
            datos.Clientes.Add(new Cliente { Id = 3, Dni = 333, Nombre = "Eva", Apellido = "Rio" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 6, ClienteId = 2, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(10) });
            ClienteServicio servicio = CrearServicio(datos);

            var estados = servicio.GetEstadoClientes();

            EstadoCliente ana = estados.Single(e => e.Cliente.Dni == 111);
            EstadoCliente luis = estados.Single(e => e.Cliente.Dni == 222);
            EstadoCliente eva = estados.Single(e => e.Cliente.Dni == 333);
            Assert.IsTrue(ana.TieneCuota);
            Assert.IsFalse(ana.Vigente);
            Assert.IsTrue(luis.TieneCuota);
            Assert.IsTrue(luis.Vigente);
            Assert.IsFalse(eva.TieneCuota);
            Assert.IsFalse(eva.Vigente);
        }
    }
}
