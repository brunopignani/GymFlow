using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IPagoRepositorio
    {
        bool AddPago(Pago pago);
        List<Pago> GetAllPagos();
        List<Pago> GetPagosByCliente(int cliId);
    }
}
