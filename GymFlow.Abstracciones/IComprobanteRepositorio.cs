using GymFlow.Dominio;
using System;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IComprobanteRepositorio
    {
        bool AddComprobante(Comprobante comprobante);
        List<Comprobante> GetAllComprobantes();
        Comprobante GetComprobanteById(int id);
        NotaVenta ObtenerNotaVentaPorCuotaYPeriodo(int cuotaClienteId, DateTime periodo);
        int ObtenerSiguienteNumeroRecibo();
        int ObtenerSiguienteNumeroNotaVenta();
        int ObtenerSiguienteNumeroNotaCredito();
        void BloquearNotaVentaParaAcreditar(int notaVentaId);
        decimal GetTotalNotasCreditoByNotaVenta(int notaVentaId);
        bool ExisteComprobanteParaCuotas(List<int> cuotaClienteIds);
    }
}
