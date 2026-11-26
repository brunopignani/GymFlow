using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Modulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Formulario> Formularios { get; set; } = new List<Formulario>();
    }
}
