using onGuardManager.Models;

namespace onGuardManager.Data.IRepository
{
    public interface IDayGuardRepository<TDayGuard> where TDayGuard : class
    {
        Task<bool> SaveGuard(TDayGuard newGuardDay);
		Task<List<TDayGuard>> GetGuards(int idCenter, int year, int month);

		//Task<bool> DeletePreviousGuard(DateOnly initialDate, DateOnly finalDate);

		Task<bool> DeletePreviousGuard(int month);

	}
}
