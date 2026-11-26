using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.IO;

namespace GymFlow.Aplicacion
{
    public class RespaldoServicio
    {
        private readonly IRespaldoRepositorio _respaldoRepositorio;
        private readonly IContextoUsuarioActual _contextoUsuario;

        public RespaldoServicio(
            IRespaldoRepositorio respaldoRepositorio,
            IContextoUsuarioActual contextoUsuario)
        {
            _respaldoRepositorio = respaldoRepositorio ?? throw new ArgumentNullException(nameof(respaldoRepositorio));
            _contextoUsuario = contextoUsuario;
        }

        public string ObtenerNombreBaseActual()
        {
            ValidarAutorizacion();
            return _respaldoRepositorio.ObtenerNombreBaseActual();
        }

        public string ObtenerRutaPorDefecto()
        {
            ValidarAutorizacion();
            return _respaldoRepositorio.ObtenerRutaPorDefecto();
        }

        public void GenerarRespaldo(string rutaArchivo)
        {
            ValidarAutorizacion();
            ValidarRutaBak(rutaArchivo);

            _respaldoRepositorio.GenerarRespaldo(rutaArchivo);
        }

        public void RestaurarRespaldo(string rutaArchivo)
        {
            ValidarAutorizacion();
            ValidarRutaBak(rutaArchivo);

            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException("El archivo de resguardo especificado no existe.", rutaArchivo);
            }

            _respaldoRepositorio.RestaurarRespaldo(rutaArchivo);
        }

        private void ValidarAutorizacion()
        {
            if (_contextoUsuario == null
                || !_contextoUsuario.EstaAutenticado
                || !_contextoUsuario.TienePermiso(CodigosPermiso.RespaldoGestionar))
            {
                throw new UnauthorizedAccessException("No posee permisos para gestionar resguardos y restauraciones de la base de datos.");
            }
        }

        private static void ValidarRutaBak(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                throw new ArgumentException("Debe especificar una ruta de archivo válida.", nameof(rutaArchivo));
            }

            string extension = Path.GetExtension(rutaArchivo);
            if (!string.Equals(extension, ".bak", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("El archivo de resguardo debe tener extensión .bak.", nameof(rutaArchivo));
            }
        }
    }
}
