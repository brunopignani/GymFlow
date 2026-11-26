using GymFlow.Dominio;

namespace GymFlow.Aplicacion
{
    public class ResultadoRegistroCobro
    {
        public bool Registrado { get; set; }
        public ReciboPago Recibo { get; set; }
        public ResumenCobro ResumenActualizado { get; set; }
    }
}
