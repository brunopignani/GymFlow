using Microsoft.Data.SqlClient;
using System;

namespace GymFlow.InterfazUsuario
{
    internal static class ConcurrenciaUI
    {
        public static bool EsConflictoDeConcurrencia(Exception ex)
        {
            Exception actual = ex;
            while (actual != null)
            {
                if (actual.Message.IndexOf("deadlock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    actual.Message.IndexOf("1205", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
                actual = actual.InnerException;
            }
            return false;
        }

        public static bool EsConflictoDeUnicidad(Exception ex)
        {
            Exception actual = ex;
            while (actual != null)
            {
                if (actual is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                {
                    return true;
                }

                if (actual.Message.IndexOf("IX_Personas_Dni", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    actual.Message.IndexOf("duplicate key", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    actual.Message.IndexOf("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                actual = actual.InnerException;
            }
            return false;
        }
    }
}
