using GymFlow.Abstracciones;

namespace GymFlow.Aplicacion
{
    internal static class Autorizacion
    {
        public static bool Tiene(IContextoUsuarioActual contexto, string codigoPermiso)
        {
            return contexto != null
                && contexto.EstaAutenticado
                && contexto.TienePermiso(codigoPermiso);
        }
    }
}