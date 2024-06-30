using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class HolidayStatusRepository : IHolidayStatusRepository<HolidayStatus>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public HolidayStatusRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<int> GetIdHolidayStatusByDescription(string description)
		{
			try
			{
				HolidayStatus? holidayStatus = await _context.HolidayStatuses.FirstOrDefaultAsync(hs => hs.Description == description);
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se busca el estado {0} en la base de datos", description);
				LogClass.WriteLog(ErrorWrite.Info, sb.ToString());
				
				return holidayStatus != null ? (int)holidayStatus.Id : 0;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los estados de las vacaciones. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion
	}
}
