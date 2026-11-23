using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Empleado : Persona
    {
        public ICollection<ReciboPago> Recibos { get; set; }
    }
}
