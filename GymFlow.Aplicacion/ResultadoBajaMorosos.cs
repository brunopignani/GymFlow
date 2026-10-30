using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class ResultadoBajaMorosos
    {
        public int ClientesAfectados { get; set; }
        public int InscripcionesEliminadas { get; set; }
        public List<string> NoProcesados { get; set; }

        public ResultadoBajaMorosos()
        {
            NoProcesados = new List<string>();
        }
    }
}
