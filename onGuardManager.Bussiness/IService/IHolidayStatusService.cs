using onGuardManager.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface IHolidayStatusService
	{
        /// <summary>
        /// Este método retorna todos los roles
        /// </summary>
        /// <returns></returns>
        Task<int> GetIdHolidayStatusByDescription(string description);

	}
}
