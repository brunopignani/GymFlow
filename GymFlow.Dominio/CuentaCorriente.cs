using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class CuentaCorriente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public ICollection<Movimiento> Movimientos { get; set; }
    }
}
