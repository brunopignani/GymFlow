using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public ClienteRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<Cliente> GetAllClientes()
        {
            return _contexto.Clientes.ToList();
        }

        public Cliente GetClienteByDNI(int dni)
        {
            return _contexto.Clientes.FirstOrDefault(c => c.Dni == dni);
        }

        public bool AddCliente(Cliente cliente)
        {
            _contexto.Clientes.Add(cliente);
            return _contexto.SaveChanges() > 0;
        }

        public bool UpdateCliente(Cliente cliente)
        {
            Cliente actual = _contexto.Clientes.FirstOrDefault(c => c.Id == cliente.Id);

            if (actual == null)
            {
                return false;
            }

            _contexto.Entry(actual).CurrentValues.SetValues(cliente);
            _contexto.SaveChanges();
            return true;
        }

        public bool DeleteCliente(int id)
        {
            Cliente cliente = _contexto.Clientes.FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return false;
            }

            _contexto.Clientes.Remove(cliente);
            return _contexto.SaveChanges() > 0;
        }
    }
}
