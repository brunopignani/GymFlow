using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Actividad
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CapacidadMaxima { get; set; }
        public ICollection<Inscripcion> Inscripciones { get; set; }
    }
}
