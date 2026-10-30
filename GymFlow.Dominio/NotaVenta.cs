namespace GymFlow.Dominio
{
    public class NotaVenta : Comprobante
    {
        public int CuotaClienteId { get; set; }
        public CuotaCliente CuotaCliente { get; set; }
    }
}
