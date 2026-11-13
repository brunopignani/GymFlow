using GymFlow.Dominio;

namespace GymFlow.Aplicacion
{
    public class ResumenCobro
    {
        public CuotaCliente CuotaCliente { get; set; }
        public decimal PrecioCuota { get; set; }
        public decimal Saldo { get; set; }
        public decimal MinimoExigible { get; set; }
        public bool TieneNotaVentaDelPeriodo { get; set; }
    }
}
