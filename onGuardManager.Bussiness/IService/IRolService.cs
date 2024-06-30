using onGuardManager.Models.DTO.Models;

namespace onGuardManager.Bussiness.IService
{
    public interface IRolService
    {
        /// <summary>
        /// Este método retorna todos los roles
        /// </summary>
        /// <returns></returns>
        Task<List<RolModel>> GetAllRols();

		/// <summary>
		/// Este método retorna un rol por su nombre
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<RolModel?> GetRolByName(string name);
	}
}
