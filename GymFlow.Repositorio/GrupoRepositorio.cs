using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class GrupoRepositorio : IGrupoRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public GrupoRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        private IQueryable<Grupo> ConRelaciones()
        {
            return _contexto.Grupos
                .Include(g => g.Acciones).ThenInclude(a => a.Formulario).ThenInclude(f => f.Modulo)
                .Include(g => g.Subgrupos).ThenInclude(s => s.Acciones)
                .Include(g => g.Subgrupos).ThenInclude(s => s.Subgrupos).ThenInclude(s => s.Acciones)
                .AsSplitQuery();
        }

        public List<Grupo> GetAllGruposConRelaciones()
        {
            return ConRelaciones().OrderBy(g => g.Nombre).ToList();
        }

        public Grupo GetGrupoByIdConRelaciones(int id)
        {
            return ConRelaciones().FirstOrDefault(g => g.Id == id);
        }

        public Grupo GetGrupoByNombre(string nombre)
        {
            return _contexto.Grupos.FirstOrDefault(g => g.Nombre == nombre);
        }

        public bool AddGrupo(Grupo grupo)
        {
            _contexto.Grupos.Add(grupo);
            return _contexto.SaveChanges() > 0;
        }

        public bool UpdateGrupo(Grupo grupo)
        {
            if (grupo == null)
            {
                return false;
            }

            if (_contexto.Entry(grupo).State == EntityState.Detached)
            {
                _contexto.Grupos.Update(grupo);
            }

            _contexto.SaveChanges();
            return true;
        }

        public bool DeleteGrupo(int id)
        {
            var grupo = _contexto.Grupos
                .Include(g => g.Acciones)
                .Include(g => g.Subgrupos)
                .FirstOrDefault(g => g.Id == id);
            if (grupo == null)
            {
                return false;
            }
            grupo.Acciones.Clear();
            grupo.Subgrupos.Clear();
            _contexto.Grupos.Remove(grupo);
            return _contexto.SaveChanges() > 0;
        }

        public bool TieneUsuariosAsignados(int grupoId)
        {
            return _contexto.Usuarios.Any(u => u.Grupos.Any(g => g.Id == grupoId));
        }

        public bool EsSubgrupoDeOtro(int grupoId)
        {
            return _contexto.Grupos.Any(g => g.Subgrupos.Any(s => s.Id == grupoId));
        }
    }
}
