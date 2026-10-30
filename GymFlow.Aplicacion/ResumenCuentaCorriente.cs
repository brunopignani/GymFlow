using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class ResumenCuentaCorriente
    {
        public CuentaCorriente Cuenta { get; set; }
        public List<Movimiento> Movimientos { get; set; }
        public decimal Saldo { get; set; }
    }
}
