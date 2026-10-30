using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class ResultadoDebitoMensual
    {
        public int Emitidos { get; set; }
        public int YaEmitidos { get; set; }
        public List<string> NoProcesados { get; set; }

        public ResultadoDebitoMensual()
        {
            NoProcesados = new List<string>();
        }
    }
}
