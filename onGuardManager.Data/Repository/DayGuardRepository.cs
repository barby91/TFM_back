using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class DayGuardRepository : IDayGuardRepository<DayGuard>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public DayGuardRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<bool> SaveGuard(DayGuard newGuardDay)
		{
			try
			{
				await _context.DayGuards.AddAsync(newGuardDay);
				return _context.SaveChanges() > 0;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeletePreviousGuard(int month)
		{
			try
			{
				if (month > 0)
				{
					_context.DayGuards.RemoveRange(await _context.DayGuards
																	.Include(dg => dg.assignedUsers)
																	.ThenInclude(au => au.IdUnityNavigation)
																	 .Where(dg => dg.Day.Month == month &&
																				  dg.Day.Year == DateTime.Now.Year)
																	 .ToListAsync());
				}
				else
				{
					month = DateTime.Now.Month + 1;
					_context.DayGuards.RemoveRange(await _context.DayGuards
																	 .Where(dg => dg.Day.Month >= month &&
																				  dg.Day.Year == DateTime.Now.Year)
																	 .ToListAsync());
				}
				return await _context.SaveChangesAsync() >= 0;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<List<DayGuard>> GetGuards(int idCenter, int year, int month)
		{
			try
			{
				if (month != 0)
				{
					return await _context.DayGuards
								  //.Include(x => x.dayGuardsUser)
								  .Include(x => x.assignedUsers.OrderBy(au => au.IdLevel))
								  .ThenInclude(au => au.IdLevelNavigation)
								  .Include(x => x.assignedUsers.OrderBy(au => au.IdLevel))
								  .ThenInclude(au => au.IdUnityNavigation)
								  .Where(dg => dg.Day.Month == month &&
											   dg.Day.Year == year &&
											   dg.assignedUsers.Select(x => x.IdCenter).Contains(idCenter))
								  .ToListAsync();
				}
				else
				{
					return await _context.DayGuards
								  .Include(x => x.assignedUsers.OrderBy(au => au.IdLevel))
								  .ThenInclude(au => au.IdLevelNavigation)
								  .Include(x => x.assignedUsers.OrderBy(au => au.IdLevel))
								  .ThenInclude(au => au.IdUnityNavigation)
								  .Where(dg => dg.Day.Year == year).ToListAsync();
				}
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las especialidades. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion
	}
}
