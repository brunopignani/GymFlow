using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public UsuarioRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<Usuario> GetAllUsuarios()
        {
            return _contexto.Usuarios.ToList();
        }

        public Usuario GetUsuarioById(int id)
        {
            return _contexto.Usuarios.FirstOrDefault(u => u.Id == id);
        }

        public bool AddUsuario(Usuario usuario)
        {
            _contexto.Usuarios.Add(usuario);
            return _contexto.SaveChanges() > 0;
        }

        public Usuario GetUsuarioByNombre(string nombre)
        {
            return _contexto.Usuarios.FirstOrDefault(u => u.Nombre == nombre);
        }

        public Usuario GetUsuarioConPermisosById(int id)
        {
            return ConPermisos().FirstOrDefault(u => u.Id == id);
        }

        public Usuario GetUsuarioConPermisosByNombre(string nombre)
        {
            return ConPermisos().FirstOrDefault(u => u.Nombre == nombre);
        }

        private IQueryable<Usuario> ConPermisos()
        {
            return _contexto.Usuarios
                .Include(u => u.AccionesDirectas)
                .Include(u => u.Grupos).ThenInclude(g => g.Acciones)
                .Include(u => u.Grupos).ThenInclude(g => g.Subgrupos).ThenInclude(s => s.Acciones)
                .Include(u => u.Grupos).ThenInclude(g => g.Subgrupos).ThenInclude(s => s.Subgrupos).ThenInclude(s => s.Acciones)
                .AsSplitQuery();
        }

        public bool UpdateUsuario(Usuario usuario)
        {
            _contexto.Usuarios.Update(usuario);
            return _contexto.SaveChanges() > 0;
        }
    }
}


