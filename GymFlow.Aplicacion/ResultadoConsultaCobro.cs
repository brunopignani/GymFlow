using GymFlow.Dominio;

namespace GymFlow.Aplicacion
{
    public class ResultadoConsultaCobro
    {
        public bool ClienteEncontrado { get; set; }
        public Cliente Cliente { get; set; }
        public ResumenCobro Resumen { get; set; }
    }
}
