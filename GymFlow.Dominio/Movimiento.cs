using System;

namespace GymFlow.Dominio
{
    public class Movimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public int TipoMovimientoId { get; set; }
        public TipoMovimiento TipoMovimiento { get; set; }
        public int ComprobanteId { get; set; }
        public Comprobante Comprobante { get; set; }
        public int CuentaCorrienteId { get; set; }
    }
}
