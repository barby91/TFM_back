using onGuardManager.Models;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface IUnityService
    {
		/// <summary>
		/// Este método obtiene todas las unidades comunes de un determinado centro
		/// </summary>
		/// <param name="ideCenter">Identificador del centro</param>
		/// <returns></returns>
		Task<List<UnityModel>> GetAllCommonUnities(int ideCenter);

		/// <summary>
		/// Este método obtiene una unidad por su id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<UnityModel?> GetUnityById(int id);

		/// <summary>
		/// Este método retorna una unidad por su nombre
		/// </summary>
		/// <param name="name">nombre de la unidad buscada</param>
		/// <returns></returns>
		Task<UnityModel?> GetUnityByName(string name);

		/// <summary>
		/// Este método añade una nueva unidad a la base de datos
		/// </summary>
		/// <param name="newUnity">nueva unidad</param>
		/// <returns></returns>
		Task<bool> AddUnity(Unity newUnity);

		/// <summary>
		/// Este método añade una lista de unidades a la base de datos
		/// </summary>
		/// <param name="reader">datos leidos del fichero</param>
		/// <returns></returns>
		Task<bool> AddUnities(StreamReader reader);

		/// <summary>
		/// Este método actualiza una unidad existente
		/// </summary>
		/// <param name="unity">Unidad a actualizar</param>
		/// <returns></returns>
		Task<bool> UpdateUnity(Unity unity);

		/// <summary>
		/// Este método elimina una unidad por su id
		/// </summary>
		/// <param name="id">Identificador de la unidad a borrar</param>
		/// <returns></returns>
		Task<bool> DeleteUnity(int id);

	}
}
