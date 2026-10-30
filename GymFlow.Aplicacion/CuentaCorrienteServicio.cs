using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class CuentaCorrienteServicio
    {
        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;

        public CuentaCorrienteServicio(ICuentaCorrienteRepositorio cuentaCorrienteRepositorio, IMovimientoRepositorio movimientoRepositorio)
        {
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
        }

        public ResumenCuentaCorriente ConsultarCuentaCorriente(int clienteId)
        {
            CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);

            if (cuenta == null)
            {
                return null;
            }

            List<Movimiento> movimientos = _movimientoRepositorio.GetMovimientosByCuenta(cuenta.Id);

            ResumenCuentaCorriente resumen = new ResumenCuentaCorriente();
            resumen.Cuenta = cuenta;
            resumen.Movimientos = movimientos;
            resumen.Saldo = CalcularSaldo(movimientos);

            return resumen;
        }

        private decimal CalcularSaldo(List<Movimiento> movimientos)
        {
            decimal saldo = 0;

            foreach (Movimiento movimiento in movimientos)
            {
                if (movimiento.TipoMovimiento.Nombre == NombresTipoMovimiento.Debito)
                {
                    saldo = saldo + movimiento.Importe;
                }
                else if (movimiento.TipoMovimiento.Nombre == NombresTipoMovimiento.Credito)
                {
                    saldo = saldo - movimiento.Importe;
                }
                else
                {
                    throw new InvalidOperationException("El movimiento " + movimiento.Id + " tiene un tipo de movimiento desconocido.");
                }
            }

            return saldo;
        }
    }
}
