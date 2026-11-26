using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class CuotaServicio
    {
        public const string NombreCuotaMensual = "Mensual";

        private readonly ICuotaRepositorio _cuotaRepositorio;
        private readonly IContextoUsuarioActual _contextoUsuario;

        public CuotaServicio(ICuotaRepositorio cuotaRepositorio, IContextoUsuarioActual contextoUsuario)
        {
            _cuotaRepositorio = cuotaRepositorio;
            _contextoUsuario = contextoUsuario;
        }

        public Cuota ObtenerCuotaMensual()
        {
            return _cuotaRepositorio.GetAllCuotas()
                .FirstOrDefault(c => string.Equals(c.Nombre, NombreCuotaMensual, StringComparison.OrdinalIgnoreCase));
        }

        public bool ActualizarPrecioMensual(decimal nuevoPrecio)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.CuotaEditarPrecio) || nuevoPrecio <= 0)
            {
                return false;
            }

            Cuota cuota = ObtenerCuotaMensual();

            if (cuota == null)
            {
                return false;
            }

            cuota.Precio = nuevoPrecio;

            return _cuotaRepositorio.UpdateCuota(cuota);
        }
    }
}