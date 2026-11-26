using GymFlow.Abstracciones;
using GymFlow.Dominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymFlow.Aplicacion
{
    public class GrupoServicio
    {
        private const int LongitudMaximaNombre = 100;
        private const int LongitudMaximaDescripcion = 250;

        private readonly IGrupoRepositorio _grupoRepositorio;
        private readonly IAccionRepositorio _accionRepositorio;
        private readonly IContextoUsuarioActual _contextoUsuario;

        public GrupoServicio(
            IGrupoRepositorio grupoRepositorio,
            IAccionRepositorio accionRepositorio,
            IContextoUsuarioActual contextoUsuario)
        {
            _grupoRepositorio = grupoRepositorio;
            _accionRepositorio = accionRepositorio;
            _contextoUsuario = contextoUsuario;
        }

        private bool Autorizado()
        {
            return Autorizacion.Tiene(_contextoUsuario, CodigosPermiso.GruposGestionar);
        }

        public List<Grupo> GetAllGrupos()
        {
            if (!Autorizado())
            {
                return new List<Grupo>();
            }

            return _grupoRepositorio.GetAllGruposConRelaciones();
        }

        public Grupo GetGrupoById(int id)
        {
            if (!Autorizado())
            {
                return null;
            }

            return _grupoRepositorio.GetGrupoByIdConRelaciones(id);
        }

        public List<Modulo> GetAllModulosConAcciones()
        {
            if (!Autorizado())
            {
                return new List<Modulo>();
            }

            return _accionRepositorio.GetAllModulosConFormulariosYAcciones();
        }

        public bool AddGrupo(string nombre, string descripcion)
        {
            if (!Autorizado() || !EsNombreValido(nombre) || !EsDescripcionValida(descripcion))
            {
                return false;
            }

            if (_grupoRepositorio.GetGrupoByNombre(nombre.Trim()) != null)
            {
                return false;
            }

            Grupo grupo = new Grupo();
            grupo.Nombre = nombre.Trim();
            grupo.Descripcion = (descripcion ?? string.Empty).Trim();
            grupo.Estado = EstadosGrupo.Activo;

            return _grupoRepositorio.AddGrupo(grupo);
        }

        public bool UpdateGrupo(int id, string nombre, string descripcion)
        {
            if (!Autorizado() || !EsNombreValido(nombre) || !EsDescripcionValida(descripcion))
            {
                return false;
            }

            Grupo grupo = _grupoRepositorio.GetGrupoByIdConRelaciones(id);

            if (grupo == null)
            {
                return false;
            }

            Grupo conMismoNombre = _grupoRepositorio.GetGrupoByNombre(nombre.Trim());

            if (conMismoNombre != null && conMismoNombre.Id != id)
            {
                return false;
            }

            grupo.Nombre = nombre.Trim();
            grupo.Descripcion = (descripcion ?? string.Empty).Trim();

            return _grupoRepositorio.UpdateGrupo(grupo);
        }

        public bool ActivarGrupo(int id)
        {
            return CambiarEstado(id, EstadosGrupo.Activo);
        }

        public bool DesactivarGrupo(int id)
        {
            return CambiarEstado(id, EstadosGrupo.Inactivo);
        }

        public bool EliminarGrupo(int id)
        {
            if (!Autorizado())
            {
                return false;
            }

            Grupo grupo = _grupoRepositorio.GetGrupoByIdConRelaciones(id);

            if (grupo == null
                || _grupoRepositorio.TieneUsuariosAsignados(id)
                || _grupoRepositorio.EsSubgrupoDeOtro(id)
                || grupo.Subgrupos.Count > 0)
            {
                return false;
            }

            // El repositorio solo elimina las relaciones del grupo (T_GRUPO_ACCION), nunca las acciones.
            return _grupoRepositorio.DeleteGrupo(id);
        }

        public bool AsignarAcciones(int grupoId, List<int> accionIds)
        {
            if (!Autorizado())
            {
                return false;
            }

            accionIds = accionIds ?? new List<int>();

            if (accionIds.Distinct().Count() != accionIds.Count)
            {
                return false;
            }

            Grupo grupo = _grupoRepositorio.GetGrupoByIdConRelaciones(grupoId);

            if (grupo == null)
            {
                return false;
            }

            List<Accion> todas = _accionRepositorio.GetAllAccionesConFormularioYModulo();
            List<Accion> seleccionadas = accionIds
                .Select(id => todas.FirstOrDefault(a => a.Id == id))
                .ToList();

            if (seleccionadas.Any(a => a == null))
            {
                return false;
            }

            grupo.Acciones.Clear();
            foreach (Accion accion in seleccionadas)
            {
                grupo.Acciones.Add(accion);
            }

            return _grupoRepositorio.UpdateGrupo(grupo);
        }

        public bool AsignarSubgrupos(int grupoId, List<int> subgrupoIds)
        {
            if (!Autorizado())
            {
                return false;
            }

            subgrupoIds = subgrupoIds ?? new List<int>();

            if (subgrupoIds.Distinct().Count() != subgrupoIds.Count || subgrupoIds.Contains(grupoId))
            {
                return false;
            }

            List<Grupo> todos = _grupoRepositorio.GetAllGruposConRelaciones();
            Grupo grupo = todos.FirstOrDefault(g => g.Id == grupoId);

            if (grupo == null)
            {
                return false;
            }

            List<Grupo> seleccionados = subgrupoIds
                .Select(id => todos.FirstOrDefault(g => g.Id == id))
                .ToList();

            if (seleccionados.Any(g => g == null))
            {
                return false;
            }

            // Prevención de ciclos: ningún subgrupo nuevo puede alcanzar al grupo padre.
            foreach (Grupo subgrupo in seleccionados)
            {
                if (AlcanzaGrupo(subgrupo.Id, grupoId, todos))
                {
                    return false;
                }
            }

            grupo.Subgrupos.Clear();
            foreach (Grupo subgrupo in seleccionados)
            {
                grupo.Subgrupos.Add(subgrupo);
            }

            return _grupoRepositorio.UpdateGrupo(grupo);
        }

        private static bool AlcanzaGrupo(int origenId, int destinoId, List<Grupo> todos)
        {
            Dictionary<int, Grupo> porId = todos.ToDictionary(g => g.Id);
            HashSet<int> visitados = new HashSet<int>();
            Stack<int> pendientes = new Stack<int>();
            pendientes.Push(origenId);

            while (pendientes.Count > 0)
            {
                int actual = pendientes.Pop();

                if (actual == destinoId)
                {
                    return true;
                }

                if (!visitados.Add(actual) || !porId.TryGetValue(actual, out Grupo grupoActual))
                {
                    continue;
                }

                foreach (Grupo hijo in grupoActual.Subgrupos)
                {
                    pendientes.Push(hijo.Id);
                }
            }

            return false;
        }

        private bool CambiarEstado(int id, string estado)
        {
            if (!Autorizado())
            {
                return false;
            }

            Grupo grupo = _grupoRepositorio.GetGrupoByIdConRelaciones(id);

            if (grupo == null)
            {
                return false;
            }

            grupo.Estado = estado;

            return _grupoRepositorio.UpdateGrupo(grupo);
        }

        private static bool EsNombreValido(string nombre)
        {
            return !string.IsNullOrWhiteSpace(nombre) && nombre.Trim().Length <= LongitudMaximaNombre;
        }

        private static bool EsDescripcionValida(string descripcion)
        {
            return descripcion == null || descripcion.Trim().Length <= LongitudMaximaDescripcion;
        }
    }
}