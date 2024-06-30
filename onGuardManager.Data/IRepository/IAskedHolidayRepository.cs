using onGuardManager.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.IRepository
{
    public interface IAskedHolidayRepository<TAskedHoliday> where TAskedHoliday : class
    {
        Task<bool> AddAskedHoliday(TAskedHoliday newAskedHoliday);

		Task<bool> UpdateAskedHoliday(TAskedHoliday askedHoliday);

		Task<List<TAskedHoliday>> GetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type);

		Task<TAskedHoliday?> GetPendingAskedHolidaysByDates(DateOnly dateFrom, DateOnly dateTo, int idUser);
	}
}
