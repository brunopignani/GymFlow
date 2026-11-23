using System;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class ResultadoAutoingreso
    {
        public bool SocioEncontrado { get; set; }
        public bool AccesoPermitido { get; set; }
        public int Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => string.IsNullOrWhiteSpace(Nombre) && string.IsNullOrWhiteSpace(Apellido)
            ? string.Empty
            : $"{Nombre} {Apellido}".Trim();
        public string EstadoCuota { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public List<string> Actividades { get; set; } = new List<string>();
    }
}
