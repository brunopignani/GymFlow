using GymFlow.ContextoBD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymFlow.InterfazUsuario
{
    public class GymFlowDbContextFactory : IDesignTimeDbContextFactory<GymFlowDbContext>
    {
        public GymFlowDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<GymFlowDbContext> opciones = new DbContextOptionsBuilder<GymFlowDbContext>();
            opciones.UseSqlServer(ConfiguracionBD.CadenaConexion);

            return new GymFlowDbContext(opciones.Options);
        }
    }
}
