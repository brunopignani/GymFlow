using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Dominio
{
    public class Grupo : ComponentePermiso
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = EstadosGrupo.Activo;
        public ICollection<Accion> Acciones { get; set; } = new List<Accion>();
        public ICollection<Grupo> Subgrupos { get; set; } = new List<Grupo>();

        public IEnumerable<ComponentePermiso> ObtenerHijos() =>
            Acciones.Cast<ComponentePermiso>().Concat(Subgrupos);

        protected internal override IReadOnlySet<string> ObtenerCodigosPermiso(HashSet<int> gruposVisitados)
        {
            var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Estado != EstadosGrupo.Activo)
            {
                return codigos;
            }

            // El conjunto representa el camino actual (ancestros): detecta ciclos
            // sin bloquear un subgrupo compartido por dos ramas distintas (DAG).
            HashSet<int> camino = gruposVisitados;
            if (Id > 0)
            {
                if (gruposVisitados.Contains(Id))
                {
                    return codigos;
                }
                camino = new HashSet<int>(gruposVisitados) { Id };
            }

            foreach (var hijo in ObtenerHijos())
            {
                codigos.UnionWith(hijo.ObtenerCodigosPermiso(camino));
            }
            return codigos;
        }
    }
}
