using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class AuditoriaRepositorio : IAuditoriaRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public AuditoriaRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public bool RegistrarSesion(AuditoriaSesion registro)
        {
            _contexto.AuditoriasSesion.Add(registro);
            return _contexto.SaveChanges() > 0;
        }

        public bool RegistrarCliente(AuditoriaCliente registro)
        {
            _contexto.AuditoriasCliente.Add(registro);
            return _contexto.SaveChanges() > 0;
        }

        public List<AuditoriaSesion> ConsultarSesiones(DateTime? desde, DateTime? hasta, string nombreUsuario, string tipoEvento)
        {
            IQueryable<AuditoriaSesion> consulta = _contexto.AuditoriasSesion.AsNoTracking();

            if (desde.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora >= desde.Value);
            }

            if (hasta.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora <= hasta.Value);
            }

            if (!string.IsNullOrWhiteSpace(nombreUsuario))
            {
                string filtroUsuario = nombreUsuario.Trim();
                consulta = consulta.Where(a => a.NombreUsuario.Contains(filtroUsuario));
            }

            if (!string.IsNullOrWhiteSpace(tipoEvento))
            {
                string filtroEvento = tipoEvento.Trim();
                consulta = consulta.Where(a => a.TipoEvento == filtroEvento);
            }

            return consulta
                .OrderByDescending(a => a.FechaHora)
                .ThenByDescending(a => a.Id)
                .ToList();
        }

        public List<AuditoriaCliente> ConsultarClientes(DateTime? desde, DateTime? hasta, string nombreUsuario, string operacion, int? dniCliente)
        {
            IQueryable<AuditoriaCliente> consulta = _contexto.AuditoriasCliente.AsNoTracking();

            if (desde.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora >= desde.Value);
            }

            if (hasta.HasValue)
            {
                consulta = consulta.Where(a => a.FechaHora <= hasta.Value);
            }

            if (!string.IsNullOrWhiteSpace(nombreUsuario))
            {
                string filtroUsuario = nombreUsuario.Trim();
                consulta = consulta.Where(a => a.NombreUsuario.Contains(filtroUsuario));
            }

            if (!string.IsNullOrWhiteSpace(operacion))
            {
                string filtroOperacion = operacion.Trim();
                consulta = consulta.Where(a => a.Operacion == filtroOperacion);
            }

            if (dniCliente.HasValue)
            {
                consulta = consulta.Where(a => a.DniCliente == dniCliente.Value);
            }

            return consulta
                .OrderByDescending(a => a.FechaHora)
                .ThenByDescending(a => a.Id)
                .ToList();
        }
    }
}
