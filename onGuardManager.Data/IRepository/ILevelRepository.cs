using onGuardManager.Models;

namespace onGuardManager.Data.IRepository
{
    public interface ILevelRepository<TLevel> where TLevel : class
    {
        Task<List<TLevel>> GetAllLevels();

        Task<TLevel?> GetLevelByName(string name);
	}
}
