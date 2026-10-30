using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class UsuarioServicioTests
    {
        private UsuarioServicio CrearServicio(DatosEnMemoria datos)
        {
            return new UsuarioServicio(
                new UsuarioRepositorioEnMemoria(datos),
                new EmpleadoRepositorioEnMemoria(datos));
        }

        private UsuarioServicio CrearConRecepcionista(DatosEnMemoria datos)
        {
            UsuarioServicio servicio = CrearServicio(datos);
            servicio.RegistrarUsuario(30111222, "Marta", "Gomez", "555", "marta", "Clave123");
            return servicio;
        }

        [TestMethod]
        public void RegistrarUsuario_DatosValidos_CreaElEmpleadoYUnUsuarioActivoConClaveHasheada()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearServicio(datos);

            bool resultado = servicio.RegistrarUsuario(30111222, "Marta", "Gomez", "555", "marta", "Clave123");

            Assert.IsTrue(resultado);
            Empleado empleado = datos.Empleados.Single(e => e.Dni == 30111222);
            Assert.AreEqual("Marta", empleado.Nombre);
            Assert.IsNotNull(empleado.UsuarioId);
            Usuario usuario = datos.Usuarios.Single();
            Assert.AreEqual("marta", usuario.Nombre);
            Assert.AreEqual(EstadosUsuario.Activo, usuario.Estado);
            Assert.IsFalse(usuario.Clave == "Clave123");
            Assert.IsTrue(ClaveSegura.Verificar("Clave123", usuario.Clave));
        }

        [TestMethod]
        public void RegistrarUsuario_TelefonoVacio_EsValido()
        {
            UsuarioServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsTrue(servicio.RegistrarUsuario(30111222, "Marta", "Gomez", "", "marta", "Clave123"));
        }

        [TestMethod]
        public void RegistrarUsuario_NombreDeUsuarioDuplicado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            Assert.IsFalse(servicio.RegistrarUsuario(40111222, "Pedro", "Lopez", "", "marta", "Otra1234"));
            Assert.AreEqual(1, datos.Usuarios.Count);
        }

        [TestMethod]
        public void RegistrarUsuario_DniDeEmpleadoExistente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            Assert.IsFalse(servicio.RegistrarUsuario(30111222, "Pedro", "Lopez", "", "pedro", "Otra1234"));
        }

        [TestMethod]
        public void RegistrarUsuario_CamposObligatoriosVacios_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.RegistrarUsuario(0, "Marta", "Gomez", "", "marta", "Clave123"));
            Assert.IsFalse(servicio.RegistrarUsuario(1, "", "Gomez", "", "marta", "Clave123"));
            Assert.IsFalse(servicio.RegistrarUsuario(1, "Marta", " ", "", "marta", "Clave123"));
            Assert.IsFalse(servicio.RegistrarUsuario(1, "Marta", "Gomez", "", "", "Clave123"));
            Assert.IsFalse(servicio.RegistrarUsuario(1, "Marta", "Gomez", "", "marta", ""));
            Assert.AreEqual(0, datos.Usuarios.Count);
        }

        [TestMethod]
        public void Login_CredencialesCorrectas_DevuelveElEmpleado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            Empleado empleado = servicio.Login("marta", "Clave123");

            Assert.IsNotNull(empleado);
            Assert.AreEqual(30111222, empleado.Dni);
        }

        [TestMethod]
        public void Login_ClaveIncorrecta_RetornaNull()
        {
            UsuarioServicio servicio = CrearConRecepcionista(DatosEnMemoria.CrearBase());

            Assert.IsNull(servicio.Login("marta", "clave123"));
            Assert.IsNull(servicio.Login("marta", "otra"));
        }

        [TestMethod]
        public void Login_UsuarioInexistente_RetornaNull()
        {
            UsuarioServicio servicio = CrearConRecepcionista(DatosEnMemoria.CrearBase());

            Assert.IsNull(servicio.Login("nadie", "Clave123"));
        }

        [TestMethod]
        public void Login_DatosVacios_RetornaNull()
        {
            UsuarioServicio servicio = CrearConRecepcionista(DatosEnMemoria.CrearBase());

            Assert.IsNull(servicio.Login("", "Clave123"));
            Assert.IsNull(servicio.Login("marta", ""));
            Assert.IsNull(servicio.Login(null, null));
        }

        [TestMethod]
        public void Login_UsuarioInactivo_RetornaNull()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            datos.Usuarios[0].Estado = EstadosUsuario.Inactivo;

            Assert.IsNull(servicio.Login("marta", "Clave123"));
        }

        [TestMethod]
        public void Login_UsuarioQueNoPerteneceAUnEmpleado_RetornaNull()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            datos.Usuarios.Add(new Usuario { Id = 7, Nombre = "huerfano", Clave = ClaveSegura.Hashear("Clave123"), Estado = EstadosUsuario.Activo });
            UsuarioServicio servicio = CrearServicio(datos);

            Assert.IsNull(servicio.Login("huerfano", "Clave123"));
        }

        [TestMethod]
        public void CambiarClave_ClaveActualCorrecta_ActualizaYPermiteIngresarConLaNueva()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            int usuarioId = datos.Usuarios[0].Id;

            bool resultado = servicio.CambiarClave(usuarioId, "Clave123", "NuevaClave9");

            Assert.IsTrue(resultado);
            Assert.IsNull(servicio.Login("marta", "Clave123"));
            Assert.IsNotNull(servicio.Login("marta", "NuevaClave9"));
        }

        [TestMethod]
        public void CambiarClave_NoGuardaLaClaveEnTextoPlano()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            servicio.CambiarClave(datos.Usuarios[0].Id, "Clave123", "NuevaClave9");

            Assert.IsFalse(datos.Usuarios[0].Clave.Contains("NuevaClave9"));
        }

        [TestMethod]
        public void CambiarClave_ClaveActualIncorrecta_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            Assert.IsFalse(servicio.CambiarClave(datos.Usuarios[0].Id, "incorrecta", "NuevaClave9"));
            Assert.IsNotNull(servicio.Login("marta", "Clave123"));
        }

        [TestMethod]
        public void CambiarClave_ClaveNuevaVaciaOIgualALaActual_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            int usuarioId = datos.Usuarios[0].Id;

            Assert.IsFalse(servicio.CambiarClave(usuarioId, "Clave123", ""));
            Assert.IsFalse(servicio.CambiarClave(usuarioId, "Clave123", "Clave123"));
        }

        [TestMethod]
        public void CambiarClave_UsuarioInexistente_RetornaFalse()
        {
            UsuarioServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.CambiarClave(99, "a", "b"));
        }

        [TestMethod]
        public void ModificarUsuario_DatosValidos_ActualizaEmpleadoYUsuario()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            int empleadoId = datos.Empleados.Single(e => e.Dni == 30111222).Id;

            bool resultado = servicio.ModificarUsuario(empleadoId, 30111222, "Marta Ines", "Gomez", "777", "marta.g");

            Assert.IsTrue(resultado);
            Assert.AreEqual("Marta Ines", datos.Empleados.Single(e => e.Id == empleadoId).Nombre);
            Assert.AreEqual("marta.g", datos.Usuarios[0].Nombre);
        }

        [TestMethod]
        public void ModificarUsuario_NombreDeUsuarioDeOtro_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            servicio.RegistrarUsuario(40111222, "Pedro", "Lopez", "", "pedro", "Clave123");
            int pedroId = datos.Empleados.Single(e => e.Dni == 40111222).Id;

            Assert.IsFalse(servicio.ModificarUsuario(pedroId, 40111222, "Pedro", "Lopez", "", "marta"));
            Assert.AreEqual("pedro", datos.Usuarios.Single(u => u.Id == datos.Empleados.Single(e => e.Id == pedroId).UsuarioId).Nombre);
        }

        [TestMethod]
        public void ModificarUsuario_DniDeOtroEmpleado_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            servicio.RegistrarUsuario(40111222, "Pedro", "Lopez", "", "pedro", "Clave123");
            int pedroId = datos.Empleados.Single(e => e.Dni == 40111222).Id;

            Assert.IsFalse(servicio.ModificarUsuario(pedroId, 30111222, "Pedro", "Lopez", "", "pedro"));
        }

        [TestMethod]
        public void ModificarUsuario_DatosInvalidosOInexistente_RetornaFalse()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            int empleadoId = datos.Empleados.Single(e => e.Dni == 30111222).Id;

            Assert.IsFalse(servicio.ModificarUsuario(empleadoId, 30111222, "", "Gomez", "", "marta"));
            Assert.IsFalse(servicio.ModificarUsuario(99, 1, "A", "B", "", "c"));
            Assert.AreEqual("Marta", datos.Empleados.Single(e => e.Id == empleadoId).Nombre);
        }

        [TestMethod]
        public void DarDeBajaUsuario_LoDejaInactivoYNoPuedeIngresar()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);
            int empleadoId = datos.Empleados.Single(e => e.Dni == 30111222).Id;

            Assert.IsTrue(servicio.DarDeBajaUsuario(empleadoId));

            Assert.AreEqual(EstadosUsuario.Inactivo, datos.Usuarios[0].Estado);
            Assert.IsNull(servicio.Login("marta", "Clave123"));
        }

        [TestMethod]
        public void DarDeBajaUsuario_EmpleadoSinUsuarioOInexistente_RetornaFalse()
        {
            UsuarioServicio servicio = CrearServicio(DatosEnMemoria.CrearBase());

            Assert.IsFalse(servicio.DarDeBajaUsuario(10));
            Assert.IsFalse(servicio.DarDeBajaUsuario(99));
        }

        [TestMethod]
        public void GetEmpleados_DevuelveLosEmpleadosConSuUsuario()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            UsuarioServicio servicio = CrearConRecepcionista(datos);

            var empleados = servicio.GetEmpleados();

            Assert.AreEqual(2, empleados.Count);
            Assert.AreEqual("marta", empleados.Single(e => e.Dni == 30111222).Usuario.Nombre);
        }
    }
}
