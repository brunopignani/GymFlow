using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GymFlow.Aplicacion
{
    public class CobroFacade
    {
        private readonly ClienteServicio _clienteServicio;
        private readonly CobroServicio _cobroServicio;
        private readonly CuentaCorrienteServicio _cuentaCorrienteServicio;

        public CobroFacade(
            ClienteServicio clienteServicio,
            CobroServicio cobroServicio,
            CuentaCorrienteServicio cuentaCorrienteServicio)
        {
            _clienteServicio = clienteServicio ?? throw new ArgumentNullException(nameof(clienteServicio));
            _cobroServicio = cobroServicio ?? throw new ArgumentNullException(nameof(cobroServicio));
            _cuentaCorrienteServicio = cuentaCorrienteServicio ?? throw new ArgumentNullException(nameof(cuentaCorrienteServicio));
        }

        public IReadOnlyList<string> ObtenerMediosPagoValidos()
        {
            return CobroServicio.MediosPagoValidos;
        }

        public ResultadoConsultaCobro BuscarClienteParaCobro(int dni)
        {
            Cliente cliente = _clienteServicio.GetClienteByDNI(dni);

            if (cliente == null)
            {
                return new ResultadoConsultaCobro
                {
                    ClienteEncontrado = false
                };
            }

            ResumenCobro resumen = _cobroServicio.ObtenerResumenCobro(cliente.Id);

            return new ResultadoConsultaCobro
            {
                ClienteEncontrado = true,
                Cliente = cliente,
                Resumen = resumen
            };
        }

        public ResumenCobro ObtenerResumenCobro(int clienteId)
        {
            return _cobroServicio.ObtenerResumenCobro(clienteId);
        }

        public ResultadoRegistroCobro RegistrarCobro(int clienteId, int empleadoId, decimal importe, string metodoPago)
        {
            bool registrado = _cobroServicio.RegistrarCobro(clienteId, empleadoId, importe, metodoPago);

            if (!registrado)
            {
                return new ResultadoRegistroCobro
                {
                    Registrado = false,
                    ResumenActualizado = _cobroServicio.ObtenerResumenCobro(clienteId)
                };
            }

            ReciboPago recibo = null;

            try
            {
                recibo = ObtenerUltimoRecibo(clienteId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            ResumenCobro resumenActualizado = _cobroServicio.ObtenerResumenCobro(clienteId);

            return new ResultadoRegistroCobro
            {
                Registrado = true,
                Recibo = recibo,
                ResumenActualizado = resumenActualizado
            };
        }

        public ReciboPago ObtenerUltimoRecibo(int clienteId)
        {
            ResumenCuentaCorriente resumen = _cuentaCorrienteServicio.ConsultarCuentaCorriente(clienteId);

            if (resumen == null || resumen.Movimientos == null)
            {
                return null;
            }

            ReciboPago ultimo = null;

            foreach (Movimiento movimiento in resumen.Movimientos)
            {
                ReciboPago recibo = movimiento.Comprobante as ReciboPago;

                if (recibo != null && (ultimo == null || recibo.Id > ultimo.Id))
                {
                    ultimo = recibo;
                }
            }

            return ultimo;
        }
    }
}
