namespace GymFlow.Abstracciones
{
    public interface IPersonaRepositorio
    {
        bool ExisteDni(int dni, int? excluirPersonaId = null);
    }
}
