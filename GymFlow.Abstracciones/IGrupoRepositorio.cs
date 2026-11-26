using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface IGrupoRepositorio
    {
        List<Grupo> GetAllGruposConRelaciones();
        Grupo GetGrupoByIdConRelaciones(int id);
        Grupo GetGrupoByNombre(string nombre);
        bool AddGrupo(Grupo grupo);
        bool UpdateGrupo(Grupo grupo);
        bool DeleteGrupo(int id);
        bool TieneUsuariosAsignados(int grupoId);
        bool EsSubgrupoDeOtro(int grupoId);
    }
}
