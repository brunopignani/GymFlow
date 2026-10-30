using System;

namespace GymFlow.Dominio
{
    public class Pago
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public int CuotaClienteId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public Empleado Empleado { get; set; }
        public CuotaCliente CuotaCliente { get; set; }
    }
}
