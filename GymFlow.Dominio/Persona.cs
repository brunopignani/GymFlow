namespace GymFlow.Dominio
{
    public class Persona
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public int Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public Usuario Usuario { get; set; }
    }
}
