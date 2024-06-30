using onGuardManager.Bussiness.IService;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;

namespace onGuardManager.Bussiness.Service
{
    public class PublicHolidayService : IPublicHolidayService
    {
        #region variables
        private readonly IPublicHolidayRepository<PublicHoliday> _publicHolidayRepository;
        #endregion

        #region constructor
        public PublicHolidayService(IPublicHolidayRepository<PublicHoliday> publicHolidayRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia LevelService");
			_publicHolidayRepository = publicHolidayRepository;
        }
		#endregion

		#region interface

		public async Task<List<PublicHolidayModel>> GetAllPublicHolidaysByCenter(int centerId)
		{
			try
			{
				List<PublicHoliday> publicHolidays = await _publicHolidayRepository.GetAllPublicHolidaysByCenter(centerId);
				List<PublicHolidayModel> publicHolidaysModel = new List<PublicHolidayModel>();
				foreach(PublicHoliday publicHoliday in publicHolidays)
				{
					publicHolidaysModel.Add(new PublicHolidayModel(publicHoliday));
				}
				return await Task.FromResult(publicHolidaysModel);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error en {0} de {1} al obtener los festivos. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion
	}
}
