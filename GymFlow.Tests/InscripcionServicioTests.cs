using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class InscripcionServicioTests
    {
        private InscripcionServicio CrearServicio(DatosEnMemoria datos)
        {
            return new InscripcionServicio(
                new CuotaClienteRepositorioEnMemoria(datos),
                new ActividadRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos),
                new ClienteRepositorioEnMemoria(datos),
                new TransaccionEnMemoria(datos));
        }

        private DatosEnMemoria CrearConCuotaVigente()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today.AddDays(5);
            return datos;
        }

        private void AgregarClienteMoroso(DatosEnMemoria datos, int clienteId, int diasVencida)
        {
            datos.Clientes.Add(new Cliente { Id = clienteId, Dni = 1000 + clienteId, Nombre = "Moroso" + clienteId, Apellido = "Prueba" });
            datos.CuotasCliente.Add(new CuotaCliente { Id = 50 + clienteId, ClienteId = clienteId, CuotaId = 1, FechaVencimiento = DateTime.Today.AddDays(-diasVencida), Estado = "VENCIDO" });
        }

        private void Inscribir(DatosEnMemoria datos, int clienteId, int actividadId)
        {
            datos.Inscripciones.Add(new Inscripcion { Id = datos.Inscripciones.Count + 1, ClienteId = clienteId, ActividadId = actividadId, Fecha = DateTime.Today });
            datos.Actividades.First(a => a.Id == actividadId).CupoMaximo--;
        }

        [TestMethod]
        public void InscribirCliente_CuotaVencida_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            InscripcionServicio servicio = CrearServicio(datos);

            bool resultado = servicio.InscribirCliente(1, 1);

            Assert.IsFalse(resultado);
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(0, datos.Inscripciones.Count);
        }

        [TestMethod]
        public void InscribirCliente_CuotaVigente_DescuentaCupoGuardaInscripcionYConfirma()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            InscripcionServicio servicio = CrearServicio(datos);

            bool resultado = servicio.InscribirCliente(1, 1);

            Assert.IsTrue(resultado);
            Assert.AreEqual(29, datos.Actividades[0].CupoMaximo);
            Inscripcion inscripcion = datos.Inscripciones.Single();
            Assert.AreEqual(1, inscripcion.ClienteId);
            Assert.AreEqual(1, inscripcion.ActividadId);
            Assert.AreEqual(DateTime.Today, inscripcion.Fecha);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void InscribirCliente_CuotaQueVenceHoy_EsValido()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente[0].FechaVencimiento = DateTime.Today;
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.InscribirCliente(1, 1));
        }

        [TestMethod]
        public void InscribirCliente_ClienteSinCuota_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            datos.CuotasCliente.Clear();
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.InscribirCliente(1, 1));
        }

        [TestMethod]
        public void InscribirCliente_ActividadInexistente_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.InscribirCliente(1, 99));
        }

        [TestMethod]
        public void InscribirCliente_SinCupoDisponible_RetornaFalse()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            datos.Actividades[0].CupoMaximo = 0;
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.InscribirCliente(1, 1));
            Assert.AreEqual(0, datos.Inscripciones.Count);
        }

        [TestMethod]
        public void InscribirCliente_UltimoCupo_LoPermiteYQuedaEnCero()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            datos.Actividades[0].CupoMaximo = 1;
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.InscribirCliente(1, 1));
            Assert.AreEqual(0, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void InscribirCliente_ClienteYaInscriptoEnLaActividad_RetornaFalseSinConsumirCupo()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            InscripcionServicio servicio = CrearServicio(datos);
            servicio.InscribirCliente(1, 1);

            bool segunda = servicio.InscribirCliente(1, 1);

            Assert.IsFalse(segunda);
            Assert.AreEqual(1, datos.Inscripciones.Count);
            Assert.AreEqual(29, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void InscribirCliente_FallaAlGuardarLaInscripcion_RevierteElCupoYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            datos.FallarAlGuardarInscripcion = true;
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.InscribirCliente(1, 1));

            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(0, datos.Inscripciones.Count);
            Assert.AreEqual(1, datos.Revertidas);
            Assert.AreEqual(0, datos.Confirmadas);
        }

        [TestMethod]
        public void DesinscribirCliente_Inscripto_EliminaLaInscripcionYLiberaElCupo()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            Inscribir(datos, 1, 1);
            InscripcionServicio servicio = CrearServicio(datos);

            bool resultado = servicio.DesinscribirCliente(1, 1);

            Assert.IsTrue(resultado);
            Assert.AreEqual(0, datos.Inscripciones.Count);
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(1, datos.Confirmadas);
        }

        [TestMethod]
        public void DesinscribirCliente_SinInscripcion_RetornaFalseSinTocarElCupo()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DesinscribirCliente(1, 1));
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void DesinscribirCliente_FallaAlEliminar_RevierteYRelanzaLaExcepcion()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            Inscribir(datos, 1, 1);
            datos.FallarAlEliminarInscripcion = true;
            InscripcionServicio servicio = CrearServicio(datos);

            Assert.ThrowsException<InvalidOperationException>(() => servicio.DesinscribirCliente(1, 1));

            Assert.AreEqual(1, datos.Inscripciones.Count);
            Assert.AreEqual(29, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(1, datos.Revertidas);
        }

        [TestMethod]
        public void GetInscripcionesByActividad_DevuelveLosClientesInscriptos()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            Inscribir(datos, 1, 1);
            InscripcionServicio servicio = CrearServicio(datos);

            var inscripciones = servicio.GetInscripcionesByActividad(1);

            Assert.AreEqual(1, inscripciones.Count);
            Assert.AreEqual("Ana", inscripciones[0].Cliente.Nombre);
        }

        [TestMethod]
        public void DarDeBajaInscripcionesDeMorosos_EliminaLasDeClientesConCuotaVencidaYLiberaCupos()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Actividades.Add(new Actividad { Id = 2, Nombre = "Spinning", CupoMaximo = 20 });
            Inscribir(datos, 1, 1);
            Inscribir(datos, 1, 2);
            InscripcionServicio servicio = CrearServicio(datos);

            ResultadoBajaMorosos resultado = servicio.DarDeBajaInscripcionesDeMorosos();

            Assert.AreEqual(1, resultado.ClientesAfectados);
            Assert.AreEqual(2, resultado.InscripcionesEliminadas);
            Assert.AreEqual(0, resultado.NoProcesados.Count);
            Assert.AreEqual(0, datos.Inscripciones.Count);
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(20, datos.Actividades[1].CupoMaximo);
        }

        [TestMethod]
        public void DarDeBajaInscripcionesDeMorosos_ClienteConCuotaVigente_NoSeToca()
        {
            DatosEnMemoria datos = CrearConCuotaVigente();
            Inscribir(datos, 1, 1);
            InscripcionServicio servicio = CrearServicio(datos);

            ResultadoBajaMorosos resultado = servicio.DarDeBajaInscripcionesDeMorosos();

            Assert.AreEqual(0, resultado.ClientesAfectados);
            Assert.AreEqual(1, datos.Inscripciones.Count);
            Assert.AreEqual(29, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void DarDeBajaInscripcionesDeMorosos_ClienteSinCuota_NoSeToca()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.CuotasCliente.Clear();
            Inscribir(datos, 1, 1);
            InscripcionServicio servicio = CrearServicio(datos);

            ResultadoBajaMorosos resultado = servicio.DarDeBajaInscripcionesDeMorosos();

            Assert.AreEqual(0, resultado.InscripcionesEliminadas);
            Assert.AreEqual(1, datos.Inscripciones.Count);
        }

        [TestMethod]
        public void DarDeBajaInscripcionesDeMorosos_SegundaCorrida_NoHaceNada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Inscribir(datos, 1, 1);
            InscripcionServicio servicio = CrearServicio(datos);
            servicio.DarDeBajaInscripcionesDeMorosos();

            ResultadoBajaMorosos segunda = servicio.DarDeBajaInscripcionesDeMorosos();

            Assert.AreEqual(0, segunda.ClientesAfectados);
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void DarDeBajaInscripcionesDeMorosos_FallaUnCliente_RevierteEseClienteYContinuaConLosDemas()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarClienteMoroso(datos, 2, 10);
            Inscribir(datos, 1, 1);
            Inscribir(datos, 2, 1);
            datos.FallarAlEliminarInscripcion = true;
            InscripcionServicio servicio = CrearServicio(datos);

            ResultadoBajaMorosos resultado = servicio.DarDeBajaInscripcionesDeMorosos();

            Assert.AreEqual(0, resultado.ClientesAfectados);
            Assert.AreEqual(2, resultado.NoProcesados.Count);
            Assert.AreEqual(2, datos.Inscripciones.Count);
            Assert.AreEqual(28, datos.Actividades[0].CupoMaximo);
            Assert.AreEqual(2, datos.Revertidas);
        }
    }
}
