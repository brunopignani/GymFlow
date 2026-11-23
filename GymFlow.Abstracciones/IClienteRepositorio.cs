using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IClienteRepositorio
    {
        List<Cliente> GetAllClientes();
        Cliente GetClienteById(int id);
        Cliente GetClienteByDNI(int dni);
        bool AddCliente(Cliente cliente);
        bool UpdateCliente(Cliente cliente);
        bool DeleteCliente(int id);
    }
}
