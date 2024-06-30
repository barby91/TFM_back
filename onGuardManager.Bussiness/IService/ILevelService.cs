using onGuardManager.Models.DTO.Models;

namespace onGuardManager.Bussiness.IService
{
    public interface ILevelService
    {
        /// <summary>
        /// Este método retorna todos los roles
        /// </summary>
        /// <returns></returns>
        Task<List<LevelModel>> GetAllLevels();

        /// <summary>
        /// Este método retorna un nivel por su nombre
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<LevelModel?> GetLevelByName(string name);

	}
}
