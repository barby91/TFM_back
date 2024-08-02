namespace onGuardManager.Data.IRepository
{
    public interface IUnityRepository<TUnity> where TUnity : class
    {
        Task<List<TUnity>> GetAllCommonUnities(int idCenter);

        Task<TUnity?> GetUnityById(int id);

		Task<TUnity?> GetUnityByName(string name);

		Task<bool> AddUnity(TUnity newUnity);

		Task<bool> AddUnities(List<TUnity> newUnities);

		Task<bool> UpdateUnity(TUnity unity);

		Task<bool> DeleteUnity(int id);

	}
}
