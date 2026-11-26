using System;

namespace GymFlow.Aplicacion
{
    public class AuditoriaLoginException : Exception
    {
        public AuditoriaLoginException()
            : base("No se pudo iniciar sesión por un error interno al registrar la auditoría. Intente nuevamente o contacte a un administrador.")
        {
        }
    }
}
