using System;

namespace GymFlow.Dominio
{
    public class AuditoriaCliente
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Operacion { get; set; }
        public int ClienteId { get; set; }
        public int DniCliente { get; set; }
        public int UsuarioId { get; set; }
        public int? EmpleadoId { get; set; }
        public string NombreUsuario { get; set; }
        public string ValoresAnteriores { get; set; }
        public string ValoresNuevos { get; set; }
    }
}
