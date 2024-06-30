using onGuardManager.Models.DTO.Models;

namespace onGuardManager.Bussiness.IService
{
    public interface IPublicHolidayService
    {
        /// <summary>
        /// Este método retorna todos los roles
        /// </summary>
        /// <returns></returns>
        Task<List<PublicHolidayModel>> GetAllPublicHolidaysByCenter(int centerId);

	}
}
