using onGuardManager.Models;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.IService
{
    public interface IUserService
    {
        /// <summary>
        /// Este método obtiene los datos de un usuario en base a un email y una contraseña
        /// devuelve un objeto nulo si no encuentra nada en la base de datos
        /// </summary>
        /// <param name="logUser">email del usuario</param>
        /// <param name="password">contraseña del usuario</param>
        /// <returns></returns>
        Task<UserModel?> GetUserByEmailAndPass(string logUser, string password);

		/// <summary>
		/// Este método obtiene un listado de los usuarios asociados a un centro
		/// </summary>
		/// <param name="idCenter">identificador del centro buscado</param>
		/// <returns></returns>
		Task<List<UserModel>> GetAllUsersByCenter(int idCenter, bool addAskedHoliday = false);

		/// <summary>
		/// Este método obtiene un usuario por su id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<RealUserModel?> GetUserById(int id);

		/// <summary>
		/// Este método obtiene un usuario por su id y retorna el modelo
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<UserModel?> GetUserModelById(int id);

		/// <summary>
		/// Este método obtiene un listado de los usuarios asociados a una especialidad
		/// </summary>
		/// <param name="idSpecialty">identificador de la especialidad</param>
		/// <returns></returns>
		Task<List<UserModel>> GetAllUsersBySpecialty(int idSpecialty);

		/// <summary>
		/// Este método añade un nuevo usuario a la base de datos
		/// </summary>
		/// <param name="newUser">nuevo usuario</param>
		/// <returns></returns>
		Task<bool> AddUser(User newUser);

		/// <summary>
		/// Este método añade nuevos usuarios a la base de datos.
		/// </summary>
		/// <param name="newUsers"></param>
		/// <returns></returns>
		Task<bool> AddUsers(StreamReader reader, int idCenter);

		/// <summary>
		/// Este método actualiza un usuario existente
		/// </summary>
		/// <param name="user">usuario modificado</param>
		/// <returns></returns>
		Task<bool> UpdateUser(User user);

		/// <summary>
		/// Este método elimina un usuario por su id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<bool> DeleteUser(int id);

		Task<List<User>> GetAllUsersByCenterRules(int idCenter, bool addAskedHoliday = false);
	}
}
