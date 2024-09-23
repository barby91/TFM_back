using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class UnityRepository : IUnityRepository<Unity>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public UnityRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<List<Unity>> GetAllCommonUnities(int idCenter)
		{
			List<Unity> unities = new List<Unity>();

			try
			{
				unities = await _context.Unities.Where(u => u.IdSpecialty == null && u.IdCenter == idCenter).ToListAsync();
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado las unidades comunes en la base de datos");

				return unities;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener las unidades comunes. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<Unity?> GetUnityById(int id)
		{
			Unity? unity = new Unity();

			try
			{
				unity = await _context.Unities.Where(u => u.Id == id).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se ha buscado la unidad en la base de datos");

				return await Task.FromResult(unity);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener la unidad de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<Unity?> GetUnityByName(string name)
		{
			Unity? unity = new Unity();

			try
			{
				unity = await _context.Unities.Where(u => u.Name.ToLower()
																.Replace("á", "a")
																.Replace("é", "e")
																.Replace("í", "i")
																.Replace("ó", "o")
																.Replace("ú", "u")
																.Replace(" ", "") ==
																name.ToLower()
																.Replace("á", "a")
																.Replace("é", "e")
																.Replace("í", "i")
																.Replace("ó", "o")
																.Replace("ú", "u")
																.Replace(" ", "")).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se ha buscado la unidad en la base de datos");

				return unity;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener la especialidad de nombre {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), name, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUnity(Unity newUnity)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				Unity? unity = _context.Unities.FirstOrDefaultAsync(u => u.Name.Equals(newUnity.Name)).GetAwaiter().GetResult();
				if (unity == null)
				{
					await _context.Unities.AddAsync(newUnity);
					result = _context.SaveChanges() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha añadido una nueva unidad a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir una unidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> AddUnities(List<Unity> newUnities)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				List<Unity> repeatedUnities = new List<Unity>();
				foreach (Unity nu in newUnities)
				{
					Unity? rs = _context.Unities.FirstOrDefaultAsync(u => nu.Name.ToUpper() == u.Name.ToUpper() && nu.Description.ToUpper() == u.Description.ToUpper()).GetAwaiter().GetResult();
					if (rs != null)
					{
						repeatedUnities.Add(rs);
					}
				}

				List<Unity> notRepeatedUnities = newUnities.Where(nu => repeatedUnities.TrueForAll(ru => ru.Name.ToUpper() != nu.Name.ToUpper() || ru.Description.ToUpper() != nu.Description.ToUpper())).ToList();
				if (notRepeatedUnities.Count != 0)
				{
					await _context.Unities.AddRangeAsync(notRepeatedUnities);
					result = _context.SaveChanges() != 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se han añadido nuevas unidades  a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir las especialidades La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> UpdateUnity(Unity unity)
		{
			bool result = true;
			try
			{
				Unity? currentUnity = _context.Unities.FirstOrDefaultAsync(u => u.Id == unity.Id).GetAwaiter().GetResult();
				if (currentUnity != null)
				{
					currentUnity.Description = unity.Description;
					currentUnity.Name = unity.Name;
					currentUnity.MaxByDay = unity.MaxByDay;
					currentUnity.MaxByDayWeekend = unity.MaxByDayWeekend;
					result = await _context.SaveChangesAsync() != 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha actualizado la unidad en la base de datos");
				}
				else
				{
					LogClass.WriteLog(ErrorWrite.Info, "No se ha encontrado la unidad en la base de datos");
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al actaulizar la unidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), unity.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeleteUnity(int id)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				Unity? unity = _context.Unities.FirstOrDefaultAsync(u => u.Id == id).GetAwaiter().GetResult();
				if (unity != null)
				{
					_context.Unities.Remove(unity);
					result = await _context.SaveChangesAsync() == 1;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha eliminado una unidad a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al eliminar la unidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}
		#endregion
	}
}
