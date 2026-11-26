using System;
using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Accion : ComponentePermiso
    {
        public string Codigo { get; set; } = string.Empty;
        public int FormularioId { get; set; }
        public Formulario Formulario { get; set; }

        protected internal override IReadOnlySet<string> ObtenerCodigosPermiso(HashSet<int> gruposVisitados)
        {
            var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(Codigo))
            {
                codigos.Add(Codigo);
            }
            return codigos;
        }
    }
}
