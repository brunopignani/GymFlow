using GymFlow.Aplicacion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GymFlow.Tests
{
    [TestClass]
    public class ValidacionesDatosTests
    {
        [TestMethod]
        public void TryParseDni_RechazaEspaciosPuntosYLongitudesFueraDeRango()
        {
            int dni;

            Assert.IsFalse(ValidacionesDatos.TryParseDni(" 30111222", out dni));
            Assert.IsFalse(ValidacionesDatos.TryParseDni("30.111.222", out dni));
            Assert.IsFalse(ValidacionesDatos.TryParseDni("301122", out dni));
            Assert.IsFalse(ValidacionesDatos.TryParseDni("301112222", out dni));
        }

        [TestMethod]
        public void TryParseDni_AceptaSieteYOchoDigitos()
        {
            int dni;

            Assert.IsTrue(ValidacionesDatos.TryParseDni("1234567", out dni));
            Assert.AreEqual(1234567, dni);
            Assert.IsTrue(ValidacionesDatos.TryParseDni("30111222", out dni));
            Assert.AreEqual(30111222, dni);
        }

        [TestMethod]
        public void EsTelefonoValido_AceptaFormatoHabitualYRechazaValoresInvalidos()
        {
            Assert.IsFalse(ValidacionesDatos.EsTelefonoValido(""));
            Assert.IsTrue(ValidacionesDatos.EsTelefonoValido("+54 (11) 5555-5555"));
            Assert.IsFalse(ValidacionesDatos.EsTelefonoValido("123456"));
            Assert.IsFalse(ValidacionesDatos.EsTelefonoValido("12ABC345"));
        }
    }
}
