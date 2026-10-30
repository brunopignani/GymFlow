namespace GymFlow.Dominio
{
    public class ReciboPago : Comprobante
    {
        public string MetodoPago { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }
    }
}
