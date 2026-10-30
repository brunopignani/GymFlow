using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GymFlow.Tests
{
    [TestClass]
    public class ActividadServicioTests
    {
        private ActividadServicio CrearServicio(DatosEnMemoria datos)
        {
            return new ActividadServicio(
                new ActividadRepositorioEnMemoria(datos),
                new InscripcionRepositorioEnMemoria(datos));
        }

        [TestMethod]
        public void GetAllActividades_DevuelveLasActividadesCargadas()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.AreEqual(1, servicio.GetAllActividades().Count);
        }

        [TestMethod]
        public void AddActividad_DatosValidos_LaGuarda()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AddActividad(new Actividad { Nombre = "Spinning", CupoMaximo = 20 }));
            Assert.AreEqual(2, datos.Actividades.Count);
        }

        [TestMethod]
        public void AddActividad_CupoDe30_EsValido()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsTrue(servicio.AddActividad(new Actividad { Nombre = "Pilates", CupoMaximo = 30 }));
        }

        [TestMethod]
        public void AddActividad_NombreVacio_RetornaFalse()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.AddActividad(new Actividad { Nombre = "  ", CupoMaximo = 10 }));
            Assert.IsFalse(servicio.AddActividad(null));
        }

        [TestMethod]
        public void AddActividad_CupoCeroONegativo_RetornaFalse()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.AddActividad(new Actividad { Nombre = "Zumba", CupoMaximo = 0 }));
            Assert.IsFalse(servicio.AddActividad(new Actividad { Nombre = "Zumba", CupoMaximo = -3 }));
        }

        [TestMethod]
        public void AddActividad_CupoMayorA30_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AddActividad(new Actividad { Nombre = "Zumba", CupoMaximo = 31 }));
            Assert.AreEqual(1, datos.Actividades.Count);
        }

        [TestMethod]
        public void UpdateActividad_DatosValidos_LaActualiza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga avanzado", CupoMaximo = 25 }));
            Assert.AreEqual("Yoga avanzado", datos.Actividades[0].Nombre);
            Assert.AreEqual(25, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void UpdateActividad_CupoDisponibleMasInscriptosSuperaLos30_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarInscriptos(datos, 20);
            datos.Actividades[0].CupoMaximo = 10;
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = 11 }));
            Assert.AreEqual(10, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void UpdateActividad_CupoDisponibleMasInscriptosIgualA30_EsValido()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarInscriptos(datos, 20);
            datos.Actividades[0].CupoMaximo = 10;
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = 10 }));
        }

        [TestMethod]
        public void UpdateActividad_ReducirElCupoConInscriptos_EsValido()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarInscriptos(datos, 20);
            datos.Actividades[0].CupoMaximo = 10;
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = 3 }));
        }

        private void AgregarInscriptos(DatosEnMemoria datos, int cantidad)
        {
            for (int i = 1; i <= cantidad; i++)
            {
                datos.Inscripciones.Add(new Inscripcion { Id = i, ClienteId = 100 + i, ActividadId = 1, Fecha = DateTime.Today });
            }
        }

        [TestMethod]
        public void UpdateActividad_Inexistente_RetornaFalse()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.UpdateActividad(new Actividad { Id = 99, Nombre = "X", CupoMaximo = 10 }));
        }

        [TestMethod]
        public void UpdateActividad_DatosInvalidos_RetornaFalseYNoModifica()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "", CupoMaximo = 10 }));
            Assert.IsFalse(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = 31 }));
            Assert.IsFalse(servicio.UpdateActividad(new Actividad { Id = 1, Nombre = "Yoga", CupoMaximo = -1 }));
            Assert.AreEqual(30, datos.Actividades[0].CupoMaximo);
        }

        [TestMethod]
        public void DeleteActividad_SinInscripciones_LaElimina()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.DeleteActividad(1));
            Assert.AreEqual(0, datos.Actividades.Count);
        }

        [TestMethod]
        public void DeleteActividad_ConInscripciones_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Inscripciones.Add(new Inscripcion { Id = 1, ClienteId = 1, ActividadId = 1, Fecha = DateTime.Today });
            ActividadServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.DeleteActividad(1));
            Assert.AreEqual(1, datos.Actividades.Count);
        }

        [TestMethod]
        public void DeleteActividad_Inexistente_RetornaFalse()
        {
            ActividadServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.DeleteActividad(99));
        }
    }
}
