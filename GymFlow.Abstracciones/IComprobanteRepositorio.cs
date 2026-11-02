using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IComprobanteRepositorio
    {
        bool AddComprobante(Comprobante comprobante);
        List<Comprobante> GetAllComprobantes();
        Comprobante GetComprobanteById(int id);
        void BloquearNotaVentaParaAcreditar(int notaVentaId);
        decimal GetTotalNotasCreditoByNotaVenta(int notaVentaId);
    }
}
