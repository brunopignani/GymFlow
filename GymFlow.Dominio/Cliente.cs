using System.Collections.Generic;

namespace GymFlow.Dominio
{
    public class Cliente : Persona
    {
        public ICollection<Pago> Pagos { get; set; }
        public ICollection<Inscripcion> Inscripciones { get; set; }
        public ICollection<CuotaCliente> CuotasCliente { get; set; }
        public CuentaCorriente Cuenta { get; set; }
    }
}
