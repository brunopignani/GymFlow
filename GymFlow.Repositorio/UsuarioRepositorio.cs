using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
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

        public bool UpdateUsuario(Usuario usuario)
        {
            _contexto.Usuarios.Update(usuario);
            return _contexto.SaveChanges() > 0;
        }
    }
}
