using System;

namespace GymFlow.Dominio
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int ActividadId { get; set; }
        public DateTime Fecha { get; set; }
        public Cliente Cliente { get; set; }
        public Actividad Actividad { get; set; }
    }
}
