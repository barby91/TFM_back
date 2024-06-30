using onGuardManager.Models.DTO.Entities;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface IAskedHolidayService
    {
		/// <summary>
		/// Este método añade una nueva petición de vacaciones a la base de datos
		/// </summary>
		/// <param name="newAskedHoliday">nueva petición de vacaciones</param>
		/// <param name="idStatus">estado</param>
		/// <returns></returns>
		Task<bool> AddAskedHoliday(AskedHoliday newAskedHoliday, int idStatus);

		/// <summary>
		/// Este método actualiza una petición de vacaciones existente
		/// </summary>
		/// <param name="askedHoliday">petición de vacaciones modificada</param>
		/// <param name="idStatus">estado</param>
		/// <returns></returns>
		Task<bool> UpdateAskedHoliday(AskedHoliday askedHoliday, int idStatus);

		/// <summary>
		///  Este método obtiene todas las peticiones de vacaciones pendientes de un centro
		/// </summary>
		/// <param name="idCenter"></param>
		/// <param name="idUser"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		Task<List<PendingAskedHolidayModel>> GetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type);

		/// <summary>
		/// Este método calcula si el usuario que ha solicitado las vacaciones tiene días suficientes
		/// </summary>
		/// <param name="askedHolidayModel"></param>
		/// <returns></returns>
		bool CheckPendingHolidaysUser(AskedHolidayModel askedHolidayModel);

		/// <summary>
		/// Este método comprueba si existe una solicitud cancelada con el mismo día inicial y final
		/// </summary>
		/// <param name="askedHolidayModel"></param>
		/// <returns></returns>
		Task<AskedHolidayModel?> UpdateCancelAskedHoliday(AskedHolidayModel askedHolidayModel);
	}
}
