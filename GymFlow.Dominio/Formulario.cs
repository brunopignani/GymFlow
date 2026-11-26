using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Formulario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; }
        public ICollection<Accion> Acciones { get; set; } = new List<Accion>();
    }
}
