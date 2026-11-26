using GymFlow.Dominio;
using System;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IAuditoriaRepositorio
    {
        bool RegistrarSesion(AuditoriaSesion registro);
        bool RegistrarCliente(AuditoriaCliente registro);
        List<AuditoriaSesion> ConsultarSesiones(DateTime? desde, DateTime? hasta, string nombreUsuario, string tipoEvento);
        List<AuditoriaCliente> ConsultarClientes(DateTime? desde, DateTime? hasta, string nombreUsuario, string operacion, int? dniCliente);
    }
}
