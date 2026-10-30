using System;
using System.Security.Cryptography;

namespace GymFlow.Aplicacion
{
    public static class ClaveSegura
    {
        private const string Prefijo = "PBKDF2";
        private const int Iteraciones = 100000;
        private const int BytesSal = 16;
        private const int BytesHash = 32;

        public static string Hashear(string clave)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(BytesSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);

            return Prefijo + "$" + Iteraciones + "$" + Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string clave, string claveGuardada)
        {
            if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(claveGuardada))
            {
                return false;
            }

            string[] partes = claveGuardada.Split('$');

            if (partes.Length != 4 || partes[0] != Prefijo)
            {
                return false;
            }

            try
            {
                int iteraciones = int.Parse(partes[1]);
                byte[] sal = Convert.FromBase64String(partes[2]);
                byte[] hashEsperado = Convert.FromBase64String(partes[3]);
                byte[] hashActual = Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

                return CryptographicOperations.FixedTimeEquals(hashActual, hashEsperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
