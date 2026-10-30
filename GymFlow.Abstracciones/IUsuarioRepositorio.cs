using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IUsuarioRepositorio
    {
        List<Usuario> GetAllUsuarios();
        Usuario GetUsuarioById(int id);
        bool AddUsuario(Usuario usuario);
        Usuario GetUsuarioByNombre(string nombre);
        bool UpdateUsuario(Usuario usuario);
    }
}
