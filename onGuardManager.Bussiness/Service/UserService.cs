using onGuardManager.Bussiness.IService;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.Service
{
    public class UserService : IUserService
    {
        #region variables
        private readonly IUserRepository<User> _userRepository;
		private readonly IPublicHolidayService _publicHolidayService;
		private readonly IRolService _rolService;
		private readonly ILevelService _levelService;
		private readonly ISpecialtyService _specialtyService;
		private readonly IUnityService _unityService;

		#endregion

		#region constructor
		public UserService(IUserRepository<User> userRepository, IPublicHolidayService publicHolidayService, IRolService rolService, 
			ILevelService levelService, ISpecialtyService specialtyService, IUnityService unityService)

		{																									 
            LogClass.WriteLog(ErrorWrite.Info, "se inicia UserService");									 
            _userRepository = userRepository;																 
			_publicHolidayService = publicHolidayService;
			_rolService = rolService;
			_levelService = levelService;
			_specialtyService = specialtyService;
			_unityService = unityService;

		}
        #endregion

        #region interface
        public async Task<UserModel?> GetUserByEmailAndPass(string logUser, string password)
        {
            try
            {
				User? userLogged = await _userRepository.GetUserByEmailAndPass(logUser, password);
				UserModel? userLoggedModel = null;
				if (userLogged != null)
				{
					List<PublicHolidayModel> publicHolidayModel = await _publicHolidayService.GetAllPublicHolidaysByCenter((int)userLogged.IdCenter);
					userLoggedModel = userLogged.Id == 0 ? null : new UserModel(userLogged, publicHolidayModel);
				}
				return await Task.FromResult(userLoggedModel);
            }catch(Exception ex)
            {
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} con usuario {2} y pass {3}. La traza es: {4}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), logUser, password, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
        }

		public async Task<List<UserModel>> GetAllUsersByCenter(int idCenter, bool addAskedHoliday = false)
		{
			try
			{
				List<User> users = await _userRepository.GetAllUsersByCenter(idCenter, addAskedHoliday);
				List<UserModel> usersModel = new List<UserModel>();
				foreach (User user in users)
				{
					usersModel.Add(new UserModel(user, _publicHolidayService.GetAllPublicHolidaysByCenter((int)user.IdCenter).Result));
				}
				return await Task.FromResult(usersModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los usuarios. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<RealUserModel?> GetUserById(int id)
		{
			try
			{
				User? user = await _userRepository.GetUserById(id);
				return await Task.FromResult((user == null || user.Id == 0) ? null : new RealUserModel(user));
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener el usuario de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<UserModel?> GetUserModelById(int id)
		{
			try
			{
				User? user = await _userRepository.GetUserById(id);
				return await Task.FromResult((user == null || user.Id == 0) ? null : new UserModel(user, await _publicHolidayService.GetAllPublicHolidaysByCenter((int)user.IdCenter)));
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener el usuario de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<List<UserModel>> GetAllUsersBySpecialty(int idSpecialty)
		{
			try
			{
				List<User> users = await _userRepository.GetAllUsersBySpecialty(idSpecialty);
				List<UserModel> usersModel = new List<UserModel>();
				foreach(User user in users)
				{
					usersModel.Add(new UserModel(user, await _publicHolidayService.GetAllPublicHolidaysByCenter((int)user.IdCenter)));
				}
				return await Task.FromResult(usersModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener el usuario por la especialidad {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), idSpecialty, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUser(User newUser)
		{
			try
			{
				newUser.Password = "1234";
				newUser.HolidayCurrentPeridod = 22;
				newUser.HolidayPreviousPeriod = 0;
				return await _userRepository.AddUser(newUser);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir un usuario. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUsers(StreamReader reader, int idCenter)
		{
			try
			{
				bool result = false;
				List<User> newUsers = new List<User>();
				string? userStr = reader.ReadLine();
				while (userStr != null)
				{
					string[] userArray = userStr == String.Empty ? [] : userStr.Split(';');
					if (userArray.Length == 7)
					{
						LevelModel? level = _levelService.GetLevelByName(userArray[3]).Result;
						RolModel? rol = _rolService.GetRolByName(userArray[4]).Result;
						SpecialtyModel? specialty = _specialtyService.GetSpecialtyByName(userArray[5]).Result;
						UnityModel? unity = _unityService.GetUnityByName(userArray[6]).Result;
						if (level != null && rol != null && specialty != null && unity != null)
						{
							newUsers.Add(new User()
							{
								Name = userArray[0],
								Surname = userArray[1],
								IdCenter = idCenter,
								Email = userArray[2],
								IdLevel = level.Id,
								Password = "1234",
								IdRol = rol.Id,
								IdSpecialty = specialty.Id,
								IdUnity = unity.Id,
								HolidayCurrentPeridod = 22,
								HolidayPreviousPeriod = 0
							});
						}
						else
						{
							if (level == null)
							{
								LogClass.WriteLog(ErrorWrite.Error, "No se ha encontrado el nivel: " + userArray[3]);
							}
							if (rol == null)
							{
								LogClass.WriteLog(ErrorWrite.Error, "No se ha encontrado el rol: " + userArray[4]);
							}
							if (specialty == null)
							{
								LogClass.WriteLog(ErrorWrite.Error, "No se ha encontrado la especialidad: " + userArray[5]);
							}
							if (unity == null)
							{
								LogClass.WriteLog(ErrorWrite.Error, "No se ha encontrado la unidad: " + userArray[6]);
							}
						}
					}
					else
					{
						LogClass.WriteLog(ErrorWrite.Error, "Faltan o sobran datos ");
					}
					userStr = reader.ReadLine();
				}

				result = await _userRepository.AddUsers(newUsers);

				return await Task.FromResult(result);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir los usuarios. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> UpdateUser(User user)
		{
			try
			{
				return await _userRepository.UpdateUser(user);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al actaulizar el usuario con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), user.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeleteUser(int id)
		{
			try
			{
				return await _userRepository.DeleteUser(id);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al eliminar un usuario. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		public async Task<List<User>> GetAllUsersByCenterRules(int idCenter, bool addAskedHoliday = false)
		{
			try
			{
				return await _userRepository.GetAllUsersByCenter(idCenter, addAskedHoliday);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los usuarios. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}


		#endregion
	}
}
