using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;

namespace GymFlow.Aplicacion
{
    public class AuditoriaServicio
    {
        private readonly IAuditoriaRepositorio _auditoriaRepositorio;
        private readonly IContextoUsuarioActual _contextoUsuario;

        public AuditoriaServicio(
            IAuditoriaRepositorio auditoriaRepositorio,
            IContextoUsuarioActual contextoUsuario)
        {
            _auditoriaRepositorio = auditoriaRepositorio;
            _contextoUsuario = contextoUsuario;
        }

        public List<AuditoriaSesion> ConsultarSesiones(DateTime? desde, DateTime? hasta, string nombreUsuario, string tipoEvento)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.AuditoriaVer))
            {
                return new List<AuditoriaSesion>();
            }

            return _auditoriaRepositorio.ConsultarSesiones(desde, hasta, nombreUsuario, tipoEvento);
        }

        public List<AuditoriaCliente> ConsultarClientes(DateTime? desde, DateTime? hasta, string nombreUsuario, string operacion, int? dniCliente)
        {
            if (!Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.AuditoriaVer))
            {
                return new List<AuditoriaCliente>();
            }

            return _auditoriaRepositorio.ConsultarClientes(desde, hasta, nombreUsuario, operacion, dniCliente);
        }
    }
}
