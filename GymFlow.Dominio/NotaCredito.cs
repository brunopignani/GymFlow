namespace GymFlow.Dominio
{
    public class NotaCredito : Comprobante
    {
        public string Motivo { get; set; }
        public int NotaVentaId { get; set; }
        public NotaVenta NotaVenta { get; set; }
    }
}
