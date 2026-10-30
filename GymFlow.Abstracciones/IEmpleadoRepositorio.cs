using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IEmpleadoRepositorio
    {
        Empleado GetEmpleadoById(int id);
        List<Empleado> GetAllEmpleados();
        Empleado GetEmpleadoByDni(int dni);
        Empleado GetEmpleadoByUsuarioId(int usuarioId);
        bool AddEmpleado(Empleado empleado);
        bool UpdateEmpleado(Empleado empleado);
    }
}
