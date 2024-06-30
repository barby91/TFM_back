using onGuardManager.Models;

namespace onGuardManager.Data.IRepository
{
    public interface IHolidayStatusRepository<TStatusHoliday> where TStatusHoliday : class
    {
        Task<int> GetIdHolidayStatusByDescription(string description);

	}
}
