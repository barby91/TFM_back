using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class UserRepository : IUserRepository<User>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public UserRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<User?> GetUserByEmailAndPass(string logUser, string password)
        {
            User? user = new User();

			try
            {
				user = await _context.Users.Include(u => u.IdLevelNavigation)
	                                 .Include(u => u.IdCenterNavigation)
                                     .Include(u => u.IdRolNavigation)
									 .Include(u => u.IdSpecialtyNavigation)
									 .Include(u => u.AskedHolidays)
									 .ThenInclude(ah => ah.IdStatusNavigation)
									 .Where(u => u.Password.Equals(password) &&
														  u.Email.StartsWith(logUser)).FirstOrDefaultAsync();
                if(user != null)
                {
                    user.Password = "";
                }

				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los datos en la base de datos");
			    
                return user;
			}
			catch (Exception ex)
			{
                StringBuilder sb = new StringBuilder("");
                sb.AppendFormat("Se ha producido un error en {0} de {1} con usuario {2} y pass {3}. La traza es: {4}: ", 
                                this.GetType().Name, MethodBase.GetCurrentMethod(), logUser, password, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

        public async Task<List<User>> GetAllUsersByCenter(int idCenter, bool addAskedHoliday = false)
        {
			List<User> user = new List<User>();

			try
			{
				if (addAskedHoliday)
				{
					user = await _context.Users.Include(u => u.IdLevelNavigation) // Joining is performed here
										 .Include(u => u.IdCenterNavigation)
										 .Include(u => u.IdRolNavigation)
										 .Include(u => u.IdSpecialtyNavigation)
										 .Include(u => u.IdUnityNavigation)
										 .Include(u => u.AskedHolidays)
										 .ThenInclude(ah => ah.IdStatusNavigation)
										 .Where(u => u.IdCenter == idCenter).ToListAsync();
				}
				else
				{
					user = await _context.Users.Include(u => u.IdLevelNavigation) // Joining is performed here
										 .Include(u => u.IdCenterNavigation)
										 .Include(u => u.IdRolNavigation)
										 .Include(u => u.IdSpecialtyNavigation)
										 .Include(u => u.IdUnityNavigation)
										 .Where(u => u.IdCenter == idCenter).ToListAsync();
				}
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los usuarios en la base de datos");

				return user;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener lo usuarios. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<User?> GetUserById(int id)
		{
			User? user;

			try
			{
				user = await _context.Users.Include(u => u.IdLevelNavigation) 
									 .Include(u => u.IdCenterNavigation)
									 .Include(u => u.IdRolNavigation)
									 .Include(u => u.IdSpecialtyNavigation)
									 .ThenInclude(s => s.Unities)
									 .Include(u => u.AskedHolidays)
									 .ThenInclude(ah => ah.IdStatusNavigation)
									 .Where(u => u.Id == id).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se ha buscado el usuario en la base de datos");

				return await Task.FromResult(user);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener la especialidad de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<List<User>> GetAllUsersBySpecialty(int idSpecialty)
		{
			List<User> user = new List<User>();

			try
			{
				user = await _context.Users.Include(u => u.IdLevelNavigation)
									 .Include(u => u.IdCenterNavigation)
									 .Include(u => u.IdRolNavigation)
									 .Include(u => u.IdSpecialtyNavigation)
									 .ThenInclude(s => s.Unities)
									 .Include(u => u.AskedHolidays)
									 .ThenInclude(ah => ah.IdStatusNavigation)
									 .Where(u => u.IdSpecialty == idSpecialty).ToListAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se ha buscado el usuario en la base de datos");

				return user;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los usuarios con especialidad {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), idSpecialty, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUser(User newUser)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				User? user = _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(newUser.Email)
																					&& u.IdCenter == newUser.IdCenter).GetAwaiter().GetResult();
				if (user == null)
				{
					await _context.Users.AddAsync(newUser);
					result = _context.SaveChanges() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha añadido un nuevo usuario a la base de datos");
				}
				else
				{
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir un usuario. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUsers(List<User> newUsers)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				List<User> repeatedUsers = new List<User>();
				foreach (User nu in newUsers)
				{
					User? ru = _context.Users.FirstOrDefaultAsync(u => nu.Email == u.Email && nu.IdCenter == u.IdCenter).GetAwaiter().GetResult();
					if (ru != null)
					{
						repeatedUsers.Add(ru);
					}
				}

				List<User> notRepeatedUser = newUsers.Where(nu => repeatedUsers.TrueForAll(ru => ru.Email != nu.Email || ru.IdCenter != nu.IdCenter)).ToList();
				if (notRepeatedUser.Count != 0)
				{
					await _context.Users.AddRangeAsync(notRepeatedUser);
					result = _context.SaveChanges() != 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se han añadido nuevos usuarios a la base de datos");
				}
				else
				{
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir los usuarios La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		
		public async Task<bool> UpdateUser(User user)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				User? currentUser = _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id).GetAwaiter().GetResult();
				if (currentUser != null)
				{
					currentUser.Name = user.Name;
					currentUser.Surname = user.Surname;
					currentUser.Email = user.Email;
					currentUser.Password = user.Password;
					currentUser.IdLevel = user.IdLevel;
					currentUser.IdRol = user.IdRol;
					currentUser.IdSpecialty = user.IdSpecialty;
					currentUser.IdUnity = user.IdUnity;
					result = await _context.SaveChangesAsync() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha actualizado el usuario en la base de datos");
				}
				else
				{
					LogClass.WriteLog(ErrorWrite.Info, "No se ha encontrado el usuario en la base de datos");
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al actualizar el usuario con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), user.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}

		public async Task<bool> DeleteUser(int id)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				User? user = _context.Users
									 .Include(u => u.AskedHolidays)	
									 //.Include(u => u.dayGuardsUser)
									 .FirstOrDefaultAsync(u => u.Id == id).GetAwaiter().GetResult();
				if (user != null)
				{
					_context.Users.Remove(user);
					result = await _context.SaveChangesAsync() > 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha eliminado un usuario a la base de datos");
				}
				else
				{
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al eliminar el usuario con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}
		#endregion
	}
}
