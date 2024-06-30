using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class AskedHolidayRepository : IAskedHolidayRepository<AskedHoliday>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public AskedHolidayRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
       
		public async Task<bool> AddAskedHoliday(AskedHoliday newAskedHoliday)
		{
			bool result = true;
			try
			{
				//primero comprobamos que no se han solicitado esos días
				List<AskedHoliday> askedHolidays = _context.AskedHolidays
														   .Include(ah => ah.IdStatusNavigation)
														   .Where(ah => ah.IdUser == newAskedHoliday.IdUser &&
																		((ah.DateFrom.CompareTo(newAskedHoliday.DateFrom) <= 0 && 
																		ah.DateTo.CompareTo(newAskedHoliday.DateFrom) >= 0) ||
																		(ah.DateFrom.CompareTo(newAskedHoliday.DateTo) <= 0 &&
																		ah.DateTo.CompareTo(newAskedHoliday.DateTo) >= 0))
																		&& !ah.IdStatusNavigation.Description.Equals("Cancelado")
																		&& ah.Period.Equals(newAskedHoliday.Period)).ToList();
				if (askedHolidays.Count == 0)
				{
					await _context.AskedHolidays.AddAsync(newAskedHoliday);
					result = _context.SaveChanges() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha añadido una nueva solicitud de vacaciones a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir una solicitud de vacaciones. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}

		public async Task<bool> UpdateAskedHoliday(AskedHoliday askedHoliday)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				AskedHoliday? currentAskedHoliday = _context.AskedHolidays.FirstOrDefaultAsync(u => u.Id == askedHoliday.Id).GetAwaiter().GetResult();
				if (currentAskedHoliday != null)
				{
					currentAskedHoliday.IdStatus = askedHoliday.IdStatus;
					result = await _context.SaveChangesAsync() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha actualizado el estado de la solicitud de vacaciones en la base de datos");
				}
				else
				{
					LogClass.WriteLog(ErrorWrite.Info, "No se ha encontrado la solicitud de vacaciones en la base de datos");
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al actualizar a solicitud de vacaciones con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), askedHoliday.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<List<AskedHoliday>> GetAllPendingAskedHolidaysByCenter(int idCenter, int idUser, string type)
		{
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				return await _context.AskedHolidays
								.Include(ah => ah.IdUserNavigation)
								.Include(ah => ah.IdStatusNavigation)
								.Where(ah => ah.IdUserNavigation.IdCenter == idCenter && ah.IdUser != idUser && 
											 ah.IdStatusNavigation.Description == type).ToListAsync();
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las peticiones de vacaciones pendientes del centro con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), idCenter, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<AskedHoliday?> GetPendingAskedHolidaysByDates(DateOnly dateFrom, DateOnly dateTo, int idUser)
		{
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				return await _context.AskedHolidays
								.FirstOrDefaultAsync(ah => ah.DateFrom.CompareTo(dateFrom) == 0 && ah.DateTo.CompareTo(dateTo) == 0 && ah.IdUser == idUser);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener la petición de vacaciones del usuario con id {2} de {3} a {4}. La traza es: {5}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), idUser, dateFrom, dateTo, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}
		#endregion
	}
}
