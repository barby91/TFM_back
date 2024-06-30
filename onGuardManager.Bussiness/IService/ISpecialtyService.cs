using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface ISpecialtyService
    {
        /// <summary>
        /// Este método obtiene un listado de las especialidades asociadas a un centro
        /// </summary>
        /// <param name="idCenter">identificador del centro buscado</param>
        /// <returns></returns>
        Task<List<SpecialtyModel>> GetAllSpecialtiesByCenter(int idCenter);

        /// <summary>
        /// Este método obtiene una especialidad por su id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SpecialtyModel?> GetSpecialtyById(int id);

		/// <summary>
		/// Este método retorna una especialidad por su nombre
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<SpecialtyModel?> GetSpecialtyByName(string name);

		/// <summary>
		/// Este método añade una nueva especialidad a la base de datos
		/// </summary>
		/// <param name="newSpecialty">nueva especialidad</param>
		/// <returns></returns>
		Task<bool> AddSpecialty(Specialty newSpecialty);

		/// <summary>
		/// Este método añade una lista de especialidades a la base de datos
		/// </summary>
		/// <param name="newSpecialties"></param>
		/// <returns></returns>
		Task<bool> AddSpecialties(List<SpecialtyModel> newSpecialties);

		/// <summary>
		/// Este método actualiza una especialidad existente
		/// </summary>
		/// <param name="specialty"></param>
		/// <returns></returns>
		Task<bool> UpdateSpecialty(Specialty specialty);

		/// <summary>
		/// Este método elimina una especialidad por su id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteSpecialty(int id);

	}
}
