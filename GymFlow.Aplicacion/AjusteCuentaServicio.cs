using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Data;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class AjusteCuentaServicio
    {
        public const int LongitudMaximaMotivo = 500;

        private readonly ICuentaCorrienteRepositorio _cuentaCorrienteRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;
        private readonly IMovimientoRepositorio _movimientoRepositorio;
        private readonly IAjusteCuentaRepositorio _ajusteCuentaRepositorio;
        private readonly ITipoMovimientoRepositorio _tipoMovimientoRepositorio;
        private readonly ITransaccionBD _transaccion;

        public AjusteCuentaServicio(
            ICuentaCorrienteRepositorio cuentaCorrienteRepositorio,
            IEmpleadoRepositorio empleadoRepositorio,
            IMovimientoRepositorio movimientoRepositorio,
            IAjusteCuentaRepositorio ajusteCuentaRepositorio,
            ITipoMovimientoRepositorio tipoMovimientoRepositorio,
            ITransaccionBD transaccion)
        {
            _cuentaCorrienteRepositorio = cuentaCorrienteRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
            _movimientoRepositorio = movimientoRepositorio;
            _ajusteCuentaRepositorio = ajusteCuentaRepositorio;
            _tipoMovimientoRepositorio = tipoMovimientoRepositorio;
            _transaccion = transaccion;
        }

        public static bool EsTipoAjusteValido(string tipoMovimiento)
        {
            return tipoMovimiento == NombresTipoMovimiento.Debito || tipoMovimiento == NombresTipoMovimiento.Credito;
        }

        public static bool EsImporteValido(decimal importe)
        {
            return importe > 0m && decimal.Round(importe, 2) == importe;
        }

        public bool RegistrarAjuste(int clienteId, int empleadoId, string tipoMovimiento, decimal importe, string motivo)
        {
            if (!EsTipoAjusteValido(tipoMovimiento) || !EsImporteValido(importe) || string.IsNullOrWhiteSpace(motivo))
            {
                return false;
            }

            string motivoNormalizado = motivo.Trim();

            if (motivoNormalizado.Length > LongitudMaximaMotivo)
            {
                return false;
            }

            DateTime hoy = DateTime.Today;

            try
            {
                _transaccion.IniciarTransaccion(IsolationLevel.Serializable);

                Empleado empleado = _empleadoRepositorio.GetEmpleadoById(empleadoId);
                CuentaCorriente cuenta = _cuentaCorrienteRepositorio.GetCuentaCorrienteByCliente(clienteId);
                TipoMovimiento tipo = _tipoMovimientoRepositorio.GetAllTiposMovimiento()
                    .FirstOrDefault(t => t.Nombre == tipoMovimiento);

                if (empleado == null || cuenta == null || tipo == null)
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                AjusteCuenta ajuste = new AjusteCuenta();
                ajuste.Numero = "AJ-" + _ajusteCuentaRepositorio.ObtenerSiguienteNumero().ToString("D8");
                ajuste.FechaEmision = hoy;
                ajuste.Total = importe;
                ajuste.Motivo = motivoNormalizado;
                ajuste.EmpleadoId = empleadoId;

                if (!_ajusteCuentaRepositorio.AddAjusteCuenta(ajuste))
                {
                    _transaccion.RevertirTransaccion();
                    return false;
                }

                Movimiento movimiento = new Movimiento();
                movimiento.Fecha = hoy;
                movimiento.Importe = importe;
                movimiento.TipoMovimientoId = tipo.Id;
                movimiento.ComprobanteId = ajuste.Id;
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
    }
}
