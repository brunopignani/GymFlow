using GymFlow.Dominio;
using System.Collections.Generic;

namespace GymFlow.Abstracciones
{
    public interface ICuotaRepositorio
    {
        List<Cuota> GetAllCuotas();
        Cuota GetCuotaById(int id);
    }
}
