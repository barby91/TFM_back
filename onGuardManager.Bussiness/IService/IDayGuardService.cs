using onGuardManager.Models;
using onGuardManager.Models.DTO.Entities;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface IDayGuardService
	{
		/// <summary>
		/// Este método añade las asignaciones realizadas para la guardia de un mes
		/// </summary>
		/// <param name="newGuardDay">asignaciones</param>
		/// <returns></returns>
		Task<bool> SaveGuard(DayGuard newGuardDay);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="month"></param>
		/// <returns></returns>
		Task<List<DayGuardModel>> GetGuards(int idCenter, int year, int month = 0);

		/// <summary>
		/// Borra las asignaciones de un mes
		/// </summary>
		/// <param name="month"></param>
		/// <returns></returns>
		Task<bool> DeletePreviousGuard(int month);

		Task<string> GetUserStats(GuardRequest guardRequest);

	}
}
