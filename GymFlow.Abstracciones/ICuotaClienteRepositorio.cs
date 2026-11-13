using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface ICuotaClienteRepositorio
    {
        bool AddCuotaCliente(CuotaCliente cuotaCliente);
        bool DeleteCuotaCliente(int id);
        List<CuotaCliente> GetCuotasByCliente(int cliId);
        bool UpdateEstadoCuota(int cclId, string nuevoEstado);
        CuotaCliente GetCuotaActivaByCliente(int cliId);
        CuotaCliente GetCuotaClienteById(int id);
        bool UpdateCuotaCliente(CuotaCliente cuotaCliente);
    }
}
