using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Tests
{
    [TestClass]
    public class CompositePermisosTests
    {
        private static Accion A(int id, string codigo) => new Accion { Id = id, Nombre = codigo, Codigo = codigo };

        private static Grupo G(int id, string nombre, string estado = EstadosGrupo.Activo) =>
            new Grupo { Id = id, Nombre = nombre, Estado = estado };

        [TestMethod]
        public void Accion_DevuelveSuCodigo()
        {
            var codigos = A(1, CodigosPermiso.CobroRegistrar).ObtenerCodigosPermiso();
            Assert.AreEqual(1, codigos.Count);
            Assert.IsTrue(codigos.Contains("COBRO_REGISTRAR"));
        }

        [TestMethod]
        public void Accion_SinCodigo_NoOtorgaNada()
        {
            Assert.AreEqual(0, new Accion { Id = 1, Codigo = " " }.ObtenerCodigosPermiso().Count);
        }

        [TestMethod]
        public void TienePermiso_CodigoVacio_Deniega()
        {
            Assert.IsFalse(A(1, "X").TienePermiso(""));
            Assert.IsFalse(A(1, "X").TienePermiso(null));
        }

        [TestMethod]
        public void Grupo_TresNiveles_DevuelveTodosSinDuplicados()
        {
            var nieto = G(3, "Nieto");
            nieto.Acciones.Add(A(3, "C"));
            nieto.Acciones.Add(A(1, "A"));
            var hijo = G(2, "Hijo");
            hijo.Acciones.Add(A(2, "B"));
            hijo.Acciones.Add(A(1, "A"));
            hijo.Subgrupos.Add(nieto);
            var raiz = G(1, "Raiz");
            raiz.Acciones.Add(A(4, "D"));
            raiz.Subgrupos.Add(hijo);

            var codigos = raiz.ObtenerCodigosPermiso();

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C", "D" }, codigos.ToList());
        }

        [TestMethod]
        public void Grupo_Inactivo_CortaSuRama()
        {
            var inactivo = G(2, "Inactivo", EstadosGrupo.Inactivo);
            inactivo.Acciones.Add(A(1, "A"));
            var raiz = G(1, "Raiz");
            raiz.Subgrupos.Add(inactivo);

            Assert.AreEqual(0, raiz.ObtenerCodigosPermiso().Count);
            Assert.AreEqual(0, inactivo.ObtenerCodigosPermiso().Count);
        }

        [TestMethod]
        public void Grupo_Dag_SubgrupoCompartidoOtorgaPermisosPorOtraRamaActiva()
        {
            var compartido = G(4, "Compartido");
            compartido.Acciones.Add(A(1, "COMPARTIDO"));

            var ramaInactiva = G(2, "RamaInactiva", EstadosGrupo.Inactivo);
            ramaInactiva.Subgrupos.Add(compartido);
            var ramaActiva = G(3, "RamaActiva");
            ramaActiva.Subgrupos.Add(compartido);

            var raiz = G(1, "Raiz");
            raiz.Subgrupos.Add(ramaInactiva);
            raiz.Subgrupos.Add(ramaActiva);

            Assert.IsTrue(raiz.ObtenerCodigosPermiso().Contains("COMPARTIDO"));
        }

        [TestMethod]
        public void Grupo_Dag_SubgrupoCompartidoPorDosRamasActivasNoSeBloquea()
        {
            var compartido = G(4, "Compartido");
            compartido.Acciones.Add(A(1, "X"));
            var r1 = G(2, "R1");
            r1.Subgrupos.Add(compartido);
            var r2 = G(3, "R2");
            r2.Subgrupos.Add(compartido);
            r2.Acciones.Add(A(2, "Y"));
            var raiz = G(1, "Raiz");
            raiz.Subgrupos.Add(r1);
            raiz.Subgrupos.Add(r2);

            CollectionAssert.AreEquivalent(new[] { "X", "Y" }, raiz.ObtenerCodigosPermiso().ToList());
            Assert.IsTrue(r2.TienePermiso("X"));
        }

        [TestMethod]
        public void Grupo_Ciclo_NoProduceStackOverflow()
        {
            var a = G(1, "A");
            var b = G(2, "B");
            a.Acciones.Add(A(1, "DE_A"));
            b.Acciones.Add(A(2, "DE_B"));
            a.Subgrupos.Add(b);
            b.Subgrupos.Add(a);

            CollectionAssert.AreEquivalent(new[] { "DE_A", "DE_B" }, a.ObtenerCodigosPermiso().ToList());
            CollectionAssert.AreEquivalent(new[] { "DE_A", "DE_B" }, b.ObtenerCodigosPermiso().ToList());
        }

        [TestMethod]
        public void Usuario_CombinaGruposYAccionesDirectas()
        {
            var g1 = G(1, "G1");
            g1.Acciones.Add(A(1, "A"));
            var g2 = G(2, "G2");
            g2.Acciones.Add(A(2, "B"));
            var usuario = new Usuario { Id = 1, Nombre = "u" };
            usuario.Grupos.Add(g1);
            usuario.Grupos.Add(g2);
            usuario.AccionesDirectas.Add(A(3, "C"));
            usuario.AccionesDirectas.Add(A(1, "A"));

            CollectionAssert.AreEquivalent(new[] { "A", "B", "C" }, usuario.ObtenerPermisosEfectivos().ToList());
            Assert.AreEqual(4, usuario.ObtenerComponentes().Count());
            Assert.IsTrue(usuario.TienePermiso("c"));
            Assert.IsFalse(usuario.TienePermiso("Z"));
        }

        [TestMethod]
        public void Usuario_SinPermisos_DenegaPorDefecto()
        {
            var usuario = new Usuario { Id = 1, Nombre = "u" };
            Assert.AreEqual(0, usuario.ObtenerPermisosEfectivos().Count);
            Assert.IsFalse(usuario.TienePermiso(CodigosPermiso.UsuariosGestionar));
            Assert.IsFalse(usuario.TienePermiso(null));
        }

        [TestMethod]
        public void Usuario_GrupoInactivo_NoOtorgaPermisos()
        {
            var g = G(1, "G", EstadosGrupo.Inactivo);
            g.Acciones.Add(A(1, "A"));
            var usuario = new Usuario { Id = 1 };
            usuario.Grupos.Add(g);
            Assert.IsFalse(usuario.TienePermiso("A"));
        }

        [TestMethod]
        public void Modelo_ConfiguraTablasDeSeguridad()
        {
            var opciones = new DbContextOptionsBuilder<GymFlowDbContext>()
                .UseSqlServer("Server=(local);Database=SoloModelo;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;
            using var ctx = new GymFlowDbContext(opciones);
            var modelo = ctx.GetService<IDesignTimeModel>().Model;

            Assert.IsNull(modelo.FindEntityType(typeof(ComponentePermiso)));
            Assert.AreEqual("T_MODULO", modelo.FindEntityType(typeof(Modulo)).GetTableName());
            Assert.AreEqual("T_FORMULARIO", modelo.FindEntityType(typeof(Formulario)).GetTableName());
            Assert.AreEqual("T_ACCION", modelo.FindEntityType(typeof(Accion)).GetTableName());
            Assert.AreEqual("T_GRUPO", modelo.FindEntityType(typeof(Grupo)).GetTableName());

            var tablas = modelo.GetEntityTypes().Select(e => e.GetTableName()).ToList();
            foreach (var t in new[] { "T_USUARIO_GRUPO", "T_GRUPO_ACCION", "T_USUARIO_ACCION", "T_GRUPO_GRUPO" })
            {
                CollectionAssert.Contains(tablas, t);
            }

            var accion = modelo.FindEntityType(typeof(Accion));
            Assert.IsTrue(accion.GetIndexes().Any(i => i.IsUnique && i.Properties.Single().Name == "Codigo"));
            Assert.AreEqual(80, accion.FindProperty("Codigo").GetMaxLength());

            var gg = modelo.GetEntityTypes().Single(e => e.GetTableName() == "T_GRUPO_GRUPO");
            Assert.AreEqual(2, gg.FindPrimaryKey().Properties.Count);
            Assert.IsTrue(gg.GetCheckConstraints().Any(c => c.Sql == "[GrupoPadreId] <> [GrupoHijoId]"));
            Assert.IsTrue(gg.GetForeignKeys().All(f => f.DeleteBehavior == DeleteBehavior.NoAction));

            var ga = modelo.GetEntityTypes().Single(e => e.GetTableName() == "T_GRUPO_ACCION");
            Assert.AreEqual(DeleteBehavior.Cascade, ga.GetForeignKeys().Single(f => f.Properties.Single().Name == "GrupoId").DeleteBehavior);
            Assert.AreEqual(DeleteBehavior.Restrict, ga.GetForeignKeys().Single(f => f.Properties.Single().Name == "AccionId").DeleteBehavior);
        }
    }
}


