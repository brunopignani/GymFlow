using GymFlow.Dominio;

namespace GymFlow.Aplicacion
{
    public class EstadoCliente
    {
        public Cliente Cliente { get; set; }
        public CuotaCliente CuotaCliente { get; set; }
        public bool TieneCuota { get; set; }
        public bool Vigente
        {
            get { return Estado == EstadosCliente.Activo; }
        }

        public string Estado => ClienteServicio.DeterminarEstadoCuota(CuotaCliente);
    }
}
