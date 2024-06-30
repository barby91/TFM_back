using onGuardManager.Bussiness.IService;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.DTO.Enumerados;

namespace onGuardManager.Bussiness.Service
{
    public class AskedHolidayService : IAskedHolidayService
    {
        #region variables
        private readonly IAskedHolidayRepository<AskedHoliday> _askedHolidayRepository;
		private readonly IUserService _userService;
		private readonly IPublicHolidayService _publicHolidayService;
		#endregion

		#region constructor
		public AskedHolidayService(IAskedHolidayRepository<AskedHoliday> askedHolidayRepository,
								   IUserService userService, IPublicHolidayService publicHolidayService)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia AskedHolidayRepository");
            _askedHolidayRepository = askedHolidayRepository;
			_userService = userService;
			_publicHolidayService = publicHolidayService;
		}
        #endregion

        #region interface
        
		public async Task<bool> AddAskedHoliday(AskedHoliday newAskedHoliday, int idStatus)
		{
			try
			{
				newAskedHoliday.IdStatus = idStatus;
				return await _askedHolidayRepository.AddAskedHoliday(newAskedHoliday);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al añadir una petición de vacaciones. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> UpdateAskedHoliday(AskedHoliday askedHoliday, int idStatus)
		{
			try
			{
				askedHoliday.IdStatus = idStatus;
				return await _askedHolidayRepository.UpdateAskedHoliday(askedHoliday);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al actaulizar la petición de vacaciones con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), askedHoliday.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<List<PendingAskedHolidayModel>> GetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type)
		{
			try
			{
				List<AskedHoliday> askedHolidays = await _askedHolidayRepository.GetAllPendingAskedHolidaysByCenter(idCenter, idUser, type);
				List<PendingAskedHolidayModel> askedHolidaysModel = new List<PendingAskedHolidayModel>();
				foreach (AskedHoliday askedHoliday in askedHolidays)
				{
					askedHolidaysModel.Add(new PendingAskedHolidayModel(askedHoliday));
				}
				return await Task.FromResult(askedHolidaysModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener las peticiones pendientes del centro con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), idCenter, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public bool CheckPendingHolidaysUser(AskedHolidayModel askedHolidayModel)
		{
			try
			{
				bool result = false;
				
				UserModel? userModel = _userService.GetUserModelById(askedHolidayModel.IdUser).Result;
				if(userModel != null)
				{ 
					List<PublicHolidayModel> publicHolidays = _publicHolidayService.GetAllPublicHolidaysByCenter(userModel.centerId).Result;
					//cogemos el mismo que el realUser si ese no es nulo, este tampoco
					int currentPeriod = DateTime.Now.Year;
					int previousPeriod = currentPeriod - 1;
					int askedDays = CalculateAskedDays(askedHolidayModel, publicHolidays);
					if ((askedHolidayModel.Period == currentPeriod.ToString() && userModel.CurrentPeriodLeftDay >= askedDays) ||
						(askedHolidayModel.Period == previousPeriod.ToString() && userModel.PreviousPeriodLeftDay >= askedDays) ||
						(askedHolidayModel.Period == "Weekend"))
					{
						result = true;
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al comprobar las vacaciones pendientes del usuario: {2}",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<AskedHolidayModel?> UpdateCancelAskedHoliday(AskedHolidayModel askedHolidayModel)
		{
			AskedHoliday? askedHoliday = await _askedHolidayRepository.GetPendingAskedHolidaysByDates(askedHolidayModel.DateFrom, askedHolidayModel.DateTo, askedHolidayModel.IdUser);
			if (askedHoliday != null)
			{
				askedHolidayModel.Id = askedHoliday.Id;
				askedHolidayModel.StatusDes = EnumHolidayStatus.Solicitado.ToString();
			}
			return askedHoliday != null ? askedHolidayModel : null;
		}

		/// <summary>
		/// Este método calcula los días laborales solicitados
		/// </summary>
		/// <param name="askedHolidayModel">vacaciones solicitadas</param>
		/// <param name="publicHolidays">festivos</param>
		/// <returns></returns>
		private int CalculateAskedDays(AskedHolidayModel askedHolidayModel, List<PublicHolidayModel> publicHolidays)
		{
			int daysDifference = 0;
			int numDaysOfPeriod = askedHolidayModel.DateTo.DayNumber - askedHolidayModel.DateFrom.DayNumber + 1;
			DateOnly currentDate = askedHolidayModel.DateFrom;
			for (int i = 0; i < numDaysOfPeriod; i++)
			{
				if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday &&
					!publicHolidays.Select(ph => ph.Date).Contains(currentDate))
				{
					daysDifference++;
				}

				currentDate = currentDate.AddDays(1);
			}

			return daysDifference;
		}
		#endregion
	}
}
