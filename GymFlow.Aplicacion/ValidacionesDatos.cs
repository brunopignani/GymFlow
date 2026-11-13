using System.Linq;
using System.Text.RegularExpressions;

namespace GymFlow.Aplicacion
{
    public static class ValidacionesDatos
    {
        public const int LongitudMaximaNombre = 100;
        public const int LongitudMaximaTelefono = 30;
        public const int LongitudMaximaUsuario = 50;

        public static bool EsDniValido(int dni)
        {
            return dni >= 1000000 && dni <= 99999999;
        }

        public static bool TryParseDni(string texto, out int dni)
        {
            dni = 0;

            if (string.IsNullOrEmpty(texto) || !Regex.IsMatch(texto, "^[0-9]{7,8}$"))
            {
                return false;
            }

            return int.TryParse(texto, out dni) && EsDniValido(dni);
        }

        public static bool EsNombreValido(string texto)
        {
            return !string.IsNullOrWhiteSpace(texto)
                && texto.Trim().Length <= LongitudMaximaNombre;
        }

        public static bool EsTelefonoValido(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return false;
            }

            string valor = telefono.Trim();

            return valor.Length <= LongitudMaximaTelefono
                && Regex.IsMatch(valor, "^[0-9+() -]+$")
                && valor.Count(char.IsDigit) >= 7;
        }

        public static bool EsNombreUsuarioValido(string nombreUsuario)
        {
            return !string.IsNullOrWhiteSpace(nombreUsuario)
                && nombreUsuario.Trim().Length <= LongitudMaximaUsuario;
        }
    }
}
