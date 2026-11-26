using GymFlow.Abstracciones;
using GymFlow.ContextoBD;
using GymFlow.Dominio;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Repositorio
{
    public class AccionRepositorio : IAccionRepositorio
    {
        private readonly GymFlowDbContext _contexto;

        public AccionRepositorio(GymFlowDbContext contexto)
        {
            _contexto = contexto;
        }

        public List<Accion> GetAllAccionesConFormularioYModulo()
        {
            return _contexto.Acciones
                .Include(a => a.Formulario).ThenInclude(f => f.Modulo)
                .OrderBy(a => a.Codigo)
                .ToList();
        }

        public Accion GetAccionByCodigo(string codigo)
        {
            return _contexto.Acciones.FirstOrDefault(a => a.Codigo == codigo);
        }

        public bool AddModulo(Modulo modulo)
        {
            _contexto.Modulos.Add(modulo);
            return _contexto.SaveChanges() > 0;
        }

        public bool AddFormulario(Formulario formulario)
        {
            _contexto.Formularios.Add(formulario);
            return _contexto.SaveChanges() > 0;
        }

        public bool AddAccion(Accion accion)
        {
            _contexto.Acciones.Add(accion);
            return _contexto.SaveChanges() > 0;
        }

        public List<Modulo> GetAllModulosConFormulariosYAcciones()
        {
            return _contexto.Modulos
                .Include(m => m.Formularios).ThenInclude(f => f.Acciones)
                .AsSplitQuery()
                .OrderBy(m => m.Nombre)
                .ToList();
        }
    }
}
