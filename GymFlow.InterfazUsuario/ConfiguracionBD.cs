using System;

namespace GymFlow.InterfazUsuario
{
    public static class ConfiguracionBD
    {
        public const string VariableDeEntorno = "GYMFLOW_CADENA_CONEXION";
        public const string CadenaPredeterminada = "Server=.\\SQLEXPRESS;Database=GymFlowDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static string CadenaConexion
        {
            get
            {
                string valor = Environment.GetEnvironmentVariable(VariableDeEntorno);

                string cadena = string.IsNullOrWhiteSpace(valor) ? CadenaPredeterminada : valor;

                if (!cadena.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase))
                {
                    cadena = cadena.TrimEnd(';') + ";TrustServerCertificate=True;";
                }

                return cadena;
            }
        }
    }
}
