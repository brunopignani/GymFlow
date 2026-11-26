using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Dominio
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public string Estado { get; set; }
        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
        public ICollection<Accion> AccionesDirectas { get; set; } = new List<Accion>();

        public IEnumerable<ComponentePermiso> ObtenerComponentes() =>
            Grupos.Cast<ComponentePermiso>().Concat(AccionesDirectas);

        public IReadOnlySet<string> ObtenerPermisosEfectivos()
        {
            var codigos = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var componente in ObtenerComponentes())
            {
                codigos.UnionWith(componente.ObtenerCodigosPermiso());
            }
            return codigos;
        }

        public bool TienePermiso(string codigo) =>
            !string.IsNullOrWhiteSpace(codigo) && ObtenerPermisosEfectivos().Contains(codigo);
    }
}
