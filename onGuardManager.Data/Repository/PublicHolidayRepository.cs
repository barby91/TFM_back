using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class PublicHolidayRepository : IPublicHolidayRepository<PublicHoliday>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public PublicHolidayRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<List<PublicHoliday>> GetAllPublicHolidaysByCenter(int centerId)
		{
			List<PublicHoliday> publicHolidays = new List<PublicHoliday>();

			try
			{
				publicHolidays = await _context.PublicHolidayCenters
												.Include(phc => phc.IdPublicHolidayNavigation)
												.Include(phc => phc.IdPublicHolidayNavigation.IdTypeNavigation)
												.Where(phc => phc.IdCenter == centerId)
												.Select(phc => phc.IdPublicHolidayNavigation)
												.ToListAsync();
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado los festivos en la base de datos");

				return publicHolidays;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener los festivos. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
