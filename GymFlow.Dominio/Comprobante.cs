using System;

namespace GymFlow.Dominio
{
    public abstract class Comprobante
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
    }
}
