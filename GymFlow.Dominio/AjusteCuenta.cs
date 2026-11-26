namespace GymFlow.Dominio
{
    public class AjusteCuenta : Comprobante
    {
        public string Motivo { get; set; }
        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }
    }
}
