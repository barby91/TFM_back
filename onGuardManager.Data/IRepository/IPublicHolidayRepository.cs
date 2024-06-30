namespace onGuardManager.Data.IRepository
{
    public interface IPublicHolidayRepository<TPublicHoliday> where TPublicHoliday : class
    {
        Task<List<TPublicHoliday>> GetAllPublicHolidaysByCenter(int centerId);
	}
}
