using GymFlow.Aplicacion;
using GymFlow.Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class GrupoServicioTests
    {
        private GrupoServicio CrearServicio(DatosEnMemoria datos, ContextoUsuarioEnMemoria contexto = null)
        {
            return new GrupoServicio(
                new GrupoRepositorioEnMemoria(datos),
                new AccionRepositorioEnMemoria(datos),
                contexto ?? ContextoUsuarioEnMemoria.ConTodosLosPermisos());
        }

        private static Grupo AgregarGrupo(DatosEnMemoria datos, int id, string nombre)
        {
            Grupo grupo = new Grupo { Id = id, Nombre = nombre, Estado = EstadosGrupo.Activo };
            datos.Grupos.Add(grupo);
            return grupo;
        }

        private static Accion AgregarAccion(DatosEnMemoria datos, int id, string codigo)
        {
            Accion accion = new Accion { Id = id, Nombre = codigo, Codigo = codigo };
            datos.Acciones.Add(accion);
            return accion;
        }

        [TestMethod]
        public void AddGrupo_DatosValidos_CreaGrupoActivo()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Assert.IsTrue(CrearServicio(datos).AddGrupo("  Recepcion  ", "Atiende socios"));
            Grupo grupo = datos.Grupos.Single();
            Assert.AreEqual("Recepcion", grupo.Nombre);
            Assert.AreEqual(EstadosGrupo.Activo, grupo.Estado);
        }

        [TestMethod]
        public void AddGrupo_NombreVacioODuplicado_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            GrupoServicio servicio = CrearServicio(datos);
            Assert.IsFalse(servicio.AddGrupo("  ", "x"));
            Assert.IsTrue(servicio.AddGrupo("Caja", "x"));
            Assert.IsFalse(servicio.AddGrupo("caja", "otra"));
            Assert.AreEqual(1, datos.Grupos.Count);
        }

        [TestMethod]
        public void UpdateGrupo_ValidaExistenciaNombreYUnicidad()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarGrupo(datos, 1, "A");
            AgregarGrupo(datos, 2, "B");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.UpdateGrupo(99, "X", ""));
            Assert.IsFalse(servicio.UpdateGrupo(1, "", ""));
            Assert.IsFalse(servicio.UpdateGrupo(1, "B", ""));
            Assert.IsTrue(servicio.UpdateGrupo(1, "A", "nueva descripcion"));
            Assert.IsTrue(servicio.UpdateGrupo(1, "C", ""));
            Assert.AreEqual("C", datos.Grupos.Single(g => g.Id == 1).Nombre);
        }

        [TestMethod]
        public void ActivarYDesactivarGrupo_CambianElEstado()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo grupo = AgregarGrupo(datos, 1, "A");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.DesactivarGrupo(1));
            Assert.AreEqual(EstadosGrupo.Inactivo, grupo.Estado);
            Assert.IsTrue(servicio.ActivarGrupo(1));
            Assert.AreEqual(EstadosGrupo.Activo, grupo.Estado);
            Assert.IsFalse(servicio.ActivarGrupo(99));
        }

        [TestMethod]
        public void AsignarAcciones_Valido_ReemplazaLasAcciones()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo grupo = AgregarGrupo(datos, 1, "A");
            AgregarAccion(datos, 1, "X");
            AgregarAccion(datos, 2, "Y");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AsignarAcciones(1, new List<int> { 1, 2 }));
            Assert.AreEqual(2, grupo.Acciones.Count);
            Assert.IsTrue(servicio.AsignarAcciones(1, new List<int> { 2 }));
            CollectionAssert.AreEqual(new[] { "Y" }, grupo.Acciones.Select(a => a.Codigo).ToList());
        }

        [TestMethod]
        public void AsignarAcciones_DuplicadasOInexistentes_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo grupo = AgregarGrupo(datos, 1, "A");
            AgregarAccion(datos, 1, "X");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AsignarAcciones(1, new List<int> { 1, 1 }));
            Assert.IsFalse(servicio.AsignarAcciones(1, new List<int> { 1, 99 }));
            Assert.IsFalse(servicio.AsignarAcciones(99, new List<int> { 1 }));
            Assert.AreEqual(0, grupo.Acciones.Count);
        }

        [TestMethod]
        public void AsignarSubgrupos_Autorreferencia_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarGrupo(datos, 1, "A");
            Assert.IsFalse(CrearServicio(datos).AsignarSubgrupos(1, new List<int> { 1 }));
        }

        [TestMethod]
        public void AsignarSubgrupos_CicloDirecto_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo a = AgregarGrupo(datos, 1, "A");
            Grupo b = AgregarGrupo(datos, 2, "B");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AsignarSubgrupos(1, new List<int> { 2 }));
            Assert.IsFalse(servicio.AsignarSubgrupos(2, new List<int> { 1 }));
            Assert.AreEqual(0, b.Subgrupos.Count);
            Assert.AreEqual(1, a.Subgrupos.Count);
        }

        [TestMethod]
        public void AsignarSubgrupos_CicloIndirecto_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarGrupo(datos, 1, "A");
            AgregarGrupo(datos, 2, "B");
            Grupo c = AgregarGrupo(datos, 3, "C");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AsignarSubgrupos(1, new List<int> { 2 }));
            Assert.IsTrue(servicio.AsignarSubgrupos(2, new List<int> { 3 }));
            Assert.IsFalse(servicio.AsignarSubgrupos(3, new List<int> { 1 }));
            Assert.AreEqual(0, c.Subgrupos.Count);
        }

        [TestMethod]
        public void AsignarSubgrupos_DuplicadosOInexistentes_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarGrupo(datos, 1, "A");
            AgregarGrupo(datos, 2, "B");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.AsignarSubgrupos(1, new List<int> { 2, 2 }));
            Assert.IsFalse(servicio.AsignarSubgrupos(1, new List<int> { 99 }));
            Assert.IsFalse(servicio.AsignarSubgrupos(99, new List<int> { 2 }));
        }

        [TestMethod]
        public void AsignarSubgrupos_DagValido_SeAcepta()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            AgregarGrupo(datos, 1, "A");
            AgregarGrupo(datos, 2, "B");
            AgregarGrupo(datos, 3, "C");
            AgregarGrupo(datos, 4, "D");
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsTrue(servicio.AsignarSubgrupos(1, new List<int> { 2, 3 }));
            Assert.IsTrue(servicio.AsignarSubgrupos(2, new List<int> { 4 }));
            Assert.IsTrue(servicio.AsignarSubgrupos(3, new List<int> { 4 }));
        }

        [TestMethod]
        public void EliminarGrupo_ConUsuariosAsignados_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo grupo = AgregarGrupo(datos, 1, "A");
            Usuario usuario = new Usuario { Id = 1, Nombre = "u" };
            usuario.Grupos.Add(grupo);
            datos.Usuarios.Add(usuario);

            Assert.IsFalse(CrearServicio(datos).EliminarGrupo(1));
            Assert.AreEqual(1, datos.Grupos.Count);
        }

        [TestMethod]
        public void EliminarGrupo_ComoSubgrupoOConSubgrupos_Rechaza()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo padre = AgregarGrupo(datos, 1, "Padre");
            Grupo hijo = AgregarGrupo(datos, 2, "Hijo");
            padre.Subgrupos.Add(hijo);
            GrupoServicio servicio = CrearServicio(datos);

            Assert.IsFalse(servicio.EliminarGrupo(2));
            Assert.IsFalse(servicio.EliminarGrupo(1));
            Assert.AreEqual(2, datos.Grupos.Count);
        }

        [TestMethod]
        public void EliminarGrupo_Valido_EliminaSoloElGrupoYConservaLasAcciones()
        {
            DatosEnMemoria datos = DatosEnMemoria.CrearBase();
            Grupo grupo = AgregarGrupo(datos, 1, "A");
            Grupo otro = AgregarGrupo(datos, 2, "Otro");
            Accion accion = AgregarAccion(datos, 1, "X");
            grupo.Acciones.Add(accion);
            otro.Acciones.Add(accion);

            Assert.IsTrue(CrearServicio(datos).EliminarGrupo(1));
            Assert.AreEqual(1, datos.Grupos.Count);
            Assert.AreEqual(1, datos.Acciones.Count);
            Assert.AreEqual(1, otro.Acciones.Count);
        }

        [TestMethod]
        public void GrupoServicio_SinPermisoONoAutenticado_DeniegaTodo()
        {
            foreach (ContextoUsuarioEnMemoria contexto in new[]
            {
                ContextoUsuarioEnMemoria.SinPermisos(),
                ContextoUsuarioEnMemoria.NoAutenticado(),
                ContextoUsuarioEnMemoria.ConPermisos(CodigosPermiso.UsuariosGestionar)
            })
            {
                DatosEnMemoria datos = DatosEnMemoria.CrearBase();
                AgregarGrupo(datos, 1, "A");
                AgregarGrupo(datos, 2, "B");
                AgregarAccion(datos, 1, "X");
                GrupoServicio servicio = CrearServicio(datos, contexto);

                Assert.AreEqual(0, servicio.GetAllGrupos().Count);
                Assert.IsNull(servicio.GetGrupoById(1));
                Assert.AreEqual(0, servicio.GetAllModulosConAcciones().Count);
                Assert.IsFalse(servicio.AddGrupo("Nuevo", ""));
                Assert.IsFalse(servicio.UpdateGrupo(1, "Z", ""));
                Assert.IsFalse(servicio.ActivarGrupo(1));
                Assert.IsFalse(servicio.DesactivarGrupo(1));
                Assert.IsFalse(servicio.EliminarGrupo(1));
                Assert.IsFalse(servicio.AsignarAcciones(1, new List<int> { 1 }));
                Assert.IsFalse(servicio.AsignarSubgrupos(1, new List<int> { 2 }));
                Assert.AreEqual(2, datos.Grupos.Count);
            }
        }
    }
}
