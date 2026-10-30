using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class PagoServicio
    {
        private readonly IPagoRepositorio _pagoRepositorio;
        private readonly ICuotaClienteRepositorio _cuotaClienteRepositorio;
        private readonly ITransaccionBD _transaccion;

        public PagoServicio(IPagoRepositorio pagoRepositorio, ICuotaClienteRepositorio cuotaClienteRepositorio, ITransaccionBD transaccion)
        {
            _pagoRepositorio = pagoRepositorio;
            _cuotaClienteRepositorio = cuotaClienteRepositorio;
            _transaccion = transaccion;
        }

        public bool RegistrarPago(Pago pago)
        {
            CuotaCliente cuotaCliente = _cuotaClienteRepositorio.GetCuotaClienteById(pago.CuotaClienteId);

            if (cuotaCliente == null)
            {
                return false;
            }

            try
            {
                _transaccion.IniciarTransaccion();

                cuotaCliente.FechaVencimiento = pago.Fecha.AddDays(30);

                if (!_cuotaClienteRepositorio.UpdateCuotaCliente(cuotaCliente))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                if (!_pagoRepositorio.AddPago(pago))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                _transaccion.ConfirmarTransaccion();
                return true;
            }
            catch (Exception)
            {
                _transaccion.RevertirTransaccion();
                throw;
            }
        }

        public List<Pago> GetAllPagos()
        {
            return _pagoRepositorio.GetAllPagos();
        }

        public List<Pago> GetPagosByCliente(int cliId)
        {
            return _pagoRepositorio.GetPagosByCliente(cliId);
        }
    }
}
