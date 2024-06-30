using onGuardManager.Models;

namespace onGuardManager.Data.IRepository
{
    public interface IUserRepository<TUser> where TUser : class
    {
        Task<TUser?> GetUserByEmailAndPass(string logUser, string password);
        
		Task<List<TUser>> GetAllUsersByCenter(int idCenter, bool addAskedHoliday = false);

		Task<TUser?> GetUserById(int id);

		Task<List<TUser>> GetAllUsersBySpecialty(int idSpecialty);

		Task<bool> AddUser(TUser newUser);

		Task<bool> AddUsers(List<TUser> newUsers);

		Task<bool> UpdateUser(TUser user);

		Task<bool> DeleteUser(int id);

	}
}
