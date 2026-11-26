using System;

namespace GymFlow.Dominio
{
    public class AuditoriaSesion
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string TipoEvento { get; set; }
        public int UsuarioId { get; set; }
        public int? EmpleadoId { get; set; }
        public string NombreUsuario { get; set; }
    }
}
