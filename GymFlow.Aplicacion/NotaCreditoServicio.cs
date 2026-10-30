using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class NotaCreditoServicio
    {
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly IComprobanteRepositorio _comprobanteRepositorio;
        private readonly ITipoMovimientoRepositorio _tipoMovimientoRepositorio;
        private readonly ITransaccionBD _transaccion;

        public NotaCreditoServicio(
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            IComprobanteRepositorio comprobanteRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion)
        {
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _comprobanteRepositorio = comprobanteRepositorio;
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _transaccion = transaccion;
        }

        public bool EmitirNotaCredito(int clienteId, int comprobanteId, decimal importe, string motivo)
        {
            if (importe <= 0 || string.IsNullOrWhiteSpace(motivo))
            {
                return false;
            }

            CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);
            TipoMovimiento tipoCredito = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                .FirstOrDefault(t => t.Nombre == NombresTipoMovimiento.Credito);

            if (cuenta == null || tipoCredito == null)
            {
                return false;
            }

            NotaVenta notaVenta = BuscarNotaVentaDeLaCuenta(cuenta.Id, comprobanteId);

            if (notaVenta == null || importe > notaVenta.Total)
            {
                return false;
            }

            DateTime hoy = DateTime.Today;

            try
            {
                _transaccion.IniciarTransaccion();

                NotaCredito notaCredito = new NotaCredito();
                notaCredito.Numero = GenerarNumeroNotaCredito();
                notaCredito.FechaEmision = hoy;
                notaCredito.Total = importe;
                notaCredito.Motivo = motivo;

                if (!_comprobanteRepositorio.AddComprobante(notaCredito))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Movimiento movimiento = new Movimiento();
                movimiento.Fecha = hoy;
                movimiento.Importe = importe;
                movimiento.TipoMovimientoId = tipoCredito.Id;
                movimiento.ComprobanteId = notaCredito.Id;
                movimiento.CuentaCorrienteId = cuenta.Id;

                if (!_movimientoRepositorio.AddMovimiento(movimiento))
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

        private NotaVenta BuscarNotaVentaDeLaCuenta(int cuentaId, int comprobanteId)
        {
            List<Movimiento> movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuentaId);

            foreach (Movimiento movimiento in movimientos)
            {
                if (movimiento.ComprobanteId == comprobanteId)
                {
                    return movimiento.Comprobante as NotaVenta;
                }
            }

            return null;
        }

        private string GenerarNumeroNotaCredito()
        {
            int cantidad = _comprobanteRepositorio.GetAllComprobantes().Count;
            return "NC-" + (cantidad + 1).ToString("D8");
        }
    }
}
