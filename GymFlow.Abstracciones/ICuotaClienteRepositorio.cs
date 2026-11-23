using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface ICuotaClienteRepositorio
    {
        bool AddCuotaCliente(CuotaCliente cuotaCliente);
        bool DeleteCuotaCliente(int id);
        List<CuotaCliente> GetCuotasByCliente(int cliId);
        CuotaCliente GetCuotaActivaByCliente(int cliId);
        Dictionary<int, CuotaCliente> GetCuotasActivasPorCliente();
        CuotaCliente GetCuotaClienteById(int id);
        bool UpdateCuotaCliente(CuotaCliente cuotaCliente);
    }
}
