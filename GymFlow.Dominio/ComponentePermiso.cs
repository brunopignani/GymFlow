using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public abstract class ComponentePermiso
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public IReadOnlySet<string> ObtenerCodigosPermiso() => ObtenerCodigosPermiso(new HashSet<int>());

        protected internal abstract IReadOnlySet<string> ObtenerCodigosPermiso(HashSet<int> gruposVisitados);

        public bool TienePermiso(string codigo) =>
            !string.IsNullOrWhiteSpace(codigo) && ObtenerCodigosPermiso().Contains(codigo);
    }
}
