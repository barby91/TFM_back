using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Models;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace onGuardManager.Data.Repository
{
 //   public class DayGuardUserRepository : IDayGuardUserRepository<DayGuardUser>
 //   {
 //       #region variables
 //       private readonly OnGuardManagerContext _context;
 //       #endregion

 //       #region constructor
 //       public DayGuardUserRepository(OnGuardManagerContext context)
 //       {
 //           _context = context;
 //       }
 //       #endregion

 //       #region interface

 //       public async Task<bool> SaveGuard(List<DayGuardUser> newGuardDay)
	//	{
	//		try
	//		{
	//			await _context.DayGuadUsers.AddRangeAsync(newGuardDay);
	//			return _context.SaveChanges() > 0;
	//		}
	//		catch (Exception ex)
	//		{
	//			StringBuilder sb = new StringBuilder("");
	//			sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
	//							this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
	//			LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
	//			throw;
	//		}
	//	}

	//	public async Task<List<DayGuardUser>> GetGuards(int year, int month)
	//	{
	//		try
	//		{
	//			if (month != 0)
	//			{
	//				return await _context.DayGuadUsers
	//							  .Include(x => x.IdGuardNavigation)
	//							  .Where(dg => dg.IdGuardNavigation.Day.Month == month &&
	//										   dg.IdGuardNavigation.Day.Year == year).ToListAsync();
	//			}
	//			else
	//			{
	//				return await _context.DayGuadUsers
	//							  .Include(x => x.IdGuardNavigation)
	//							  .Where(dg => dg.IdGuardNavigation.Day.Year == year).ToListAsync();
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			StringBuilder sb = new StringBuilder("");
	//			sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
	//							this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
	//			LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
	//			throw;
	//		}
	//	}

	//	#endregion
	//}
}
