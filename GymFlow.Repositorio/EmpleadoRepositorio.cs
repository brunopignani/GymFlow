using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class EmpleadoRepositorio : IEmpleadoRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public EmpleadoRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public Empleado GetEmpleadoById(int id)
        {
            return _contexto.Empleados.Include(e => e.Usuario).FirstOrDefault(e => e.Id == id);
        }

        public List<Empleado> GetAllEmpleados()
        {
            return _contexto.Empleados.Include(e => e.Usuario).ToList();
        }

        public Empleado GetEmpleadoByDni(int dni)
        {
            return _contexto.Empleados.Include(e => e.Usuario).FirstOrDefault(e => e.Dni == dni);
        }

        public Empleado GetEmpleadoByUsuarioId(int usuarioId)
        {
            return _contexto.Empleados.Include(e => e.Usuario).FirstOrDefault(e => e.UsuarioId == usuarioId);
        }

        public bool AddEmpleado(Empleado empleado)
        {
            _contexto.Empleados.Add(empleado);
            return _contexto.SaveChanges() > 0;
        }

        public bool UpdateEmpleado(Empleado empleado)
        {
            _contexto.Empleados.Update(empleado);
            return _contexto.SaveChanges() > 0;
        }
    }
}
