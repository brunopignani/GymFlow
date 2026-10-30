using GymFlow.Aplicacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GymFlow.Tests
{
    [TestClass]
    public class ClaveSeguraTests
    {
        [TestMethod]
        public void Hashear_NoDevuelveLaClaveEnTextoPlano()
        {
            string hash = ClaveSegura.Hashear("Clave123");

            Assert.IsFalse(hash.Contains("Clave123"));
        }

        [TestMethod]
        public void Hashear_LaMismaClaveGeneraHashesDistintosPorLaSal()
        {
            Assert.IsFalse(ClaveSegura.Hashear("Clave123") == ClaveSegura.Hashear("Clave123"));
        }

        [TestMethod]
        public void Verificar_ClaveCorrecta_RetornaTrue()
        {
            Assert.IsTrue(ClaveSegura.Verificar("Clave123", ClaveSegura.Hashear("Clave123")));
        }

        [TestMethod]
        public void Verificar_ClaveIncorrecta_RetornaFalse()
        {
            Assert.IsFalse(ClaveSegura.Verificar("clave123", ClaveSegura.Hashear("Clave123")));
        }

        [TestMethod]
        public void Verificar_HashMalFormadoONulo_RetornaFalseSinLanzarExcepcion()
        {
            Assert.IsFalse(ClaveSegura.Verificar("Clave123", "texto-plano"));
            Assert.IsFalse(ClaveSegura.Verificar("Clave123", null));
            Assert.IsFalse(ClaveSegura.Verificar("Clave123", ""));
        }
    }
}
