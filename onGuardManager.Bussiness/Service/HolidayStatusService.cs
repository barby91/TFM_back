using onGuardManager.Bussiness.IService;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.Service
{
    public class HolidayStatusService : IHolidayStatusService
    {
        #region variables
        private readonly IHolidayStatusRepository<HolidayStatus> _holidayStatusRepository;
        #endregion

        #region constructor
        public HolidayStatusService(IHolidayStatusRepository<HolidayStatus> holidayStatusRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia LevelService");
			_holidayStatusRepository = holidayStatusRepository;
        }
		#endregion

		#region interface

		public async Task<int> GetIdHolidayStatusByDescription(string description)
		{
			try
			{
				return await _holidayStatusRepository.GetIdHolidayStatusByDescription(description);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los estados de las vacaciones. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion
	}
}
