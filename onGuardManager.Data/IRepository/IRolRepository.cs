using onGuardManager.Models;

namespace onGuardManager.Data.IRepository
{
    public interface IRolRepository<TRol> where TRol : class
    {
        Task<List<TRol>> GetAllRols();
		Task<TRol?> GetRolByName(string name);
	}
}
