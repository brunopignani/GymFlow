using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IAccionRepositorio
    {
        List<Accion> GetAllAccionesConFormularioYModulo();
        Accion GetAccionByCodigo(string codigo);
        bool AddModulo(Modulo modulo);
        bool AddFormulario(Formulario formulario);
        bool AddAccion(Accion accion);
        List<Modulo> GetAllModulosConFormulariosYAcciones();
    }
}
