using System;

namespace GymFlow.Dominio
{
    public class CuotaCliente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int CuotaId { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; }
        public Cliente Cliente { get; set; }
        public Cuota Cuota { get; set; }
    }
}
