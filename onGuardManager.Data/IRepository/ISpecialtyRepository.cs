namespace onGuardManager.Data.IRepository
{
    public interface ISpecialtyRepository<TSpecialty> where TSpecialty : class
    {
        Task<List<TSpecialty>> GetAllSpecialtiesWithAllUnitiesByCenter(int idCenter);

		Task<List<TSpecialty>> GetAllSpecialtiesWithoutCommonUnitiesByCenter(int idCenter);

		Task<TSpecialty?> GetSpecialtyById(int id);

		Task<TSpecialty?> GetSpecialtyByName(string name);

		Task<bool> AddSpecialty(TSpecialty newSpecialty);

		Task<bool> AddSpecialties(List<TSpecialty> newSpecialtiess);

		Task<bool> UpdateSpecialty(TSpecialty specialty);

		Task<bool> DeleteSpecialty(int id);

	}
}
