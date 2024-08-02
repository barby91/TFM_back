using Microsoft.EntityFrameworkCore;
using onGuardManager.Data.DataContext;
using onGuardManager.Data.IRepository;
using onGuardManager.Logger;
using System.Reflection;
using System.Text;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.Repository
{
    public class SpecialtyRepository : ISpecialtyRepository<Specialty>
    {
        #region variables
        private readonly OnGuardManagerContext _context;
        #endregion

        #region constructor
        public SpecialtyRepository(OnGuardManagerContext context)
        {
            _context = context;
        }
        #endregion

        #region interface
        public async Task<List<Specialty>> GetAllSpecialtiesWithAllUnitiesByCenter(int idCenter)
		{
			List<Specialty> specialties = new List<Specialty>();

			try
			{
				specialties = await _context.Specialties
											.Include(s => s.Unities)
											.Include(s => s.Users)
											.Where(s => s.IdCenter == idCenter).ToListAsync();
				List<Unity> commonUnities = await _context.Unities.Where(u => u.IdSpecialty == null).ToListAsync();
				commonUnities.ForEach(c => specialties.ForEach(s => s.Unities.Add(c)));
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado las especialidades en la base de datos");

				return specialties;
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

		public async Task<List<Specialty>> GetAllSpecialtiesWithoutCommonUnitiesByCenter(int idCenter)
		{
			List<Specialty> specialties = new List<Specialty>();

			try
			{
				specialties = await _context.Specialties
											.Include(s => s.Unities)
											.Include(s => s.Users)
											.Where(s => s.IdCenter == idCenter).ToListAsync();
				
				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado las especialidades en la base de datos");

				return specialties;
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

		public async Task<Specialty?> GetSpecialtyById(int id)
		{
			Specialty? specialty = new Specialty();

			try
			{
				specialty = await _context.Specialties
									.Include(s => s.Unities)
									.Where(u => u.Id == id).FirstOrDefaultAsync();

				LogClass.WriteLog(ErrorWrite.Info, "Se ha buscado la especialidad en la base de datos");

				return specialty;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al obtener la especialidad de id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<Specialty?> GetSpecialtyByName(string name)
		{
			Specialty? specialty = new Specialty();

			try
			{
				specialty = await _context.Specialties.Where(l => l.Name.ToLower()
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

				LogClass.WriteLog(ErrorWrite.Info, "Se han buscado las especialidad en la base de datos");

				return specialty;
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

		public async Task<bool> AddSpecialty(Specialty newSpecialty)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				Specialty? specialty = _context.Specialties.FirstOrDefaultAsync(u => u.Name.Equals(newSpecialty.Name) 
																						&& u.IdCenter == newSpecialty.IdCenter).GetAwaiter().GetResult();
				if (specialty == null)
				{
					await _context.Specialties.AddAsync(newSpecialty);
					result = _context.SaveChanges() > 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha añadido una nueva especialidad a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al añadir una especialidad. La traza es: {2}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}

		public async Task<bool> AddSpecialties(List<Specialty> newSpecialties)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				List<Specialty> repeatedSpecialties = new List<Specialty>();
				foreach (Specialty nu in newSpecialties)
				{
					Specialty? rs = _context.Specialties.FirstOrDefaultAsync(u => nu.Name == u.Name && nu.IdCenter == u.IdCenter).GetAwaiter().GetResult();
					if (rs != null)
					{
						repeatedSpecialties.Add(rs);
					}
				}

				List<Specialty> notRepeatedSpecialties = newSpecialties.Where(nu => repeatedSpecialties.TrueForAll(ru => ru.Name != nu.Name || 
																														 ru.IdCenter != nu.IdCenter)).ToList();
				if (notRepeatedSpecialties.Count != 0)
				{
					await _context.Specialties.AddRangeAsync(notRepeatedSpecialties);
					result = _context.SaveChanges() != 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se han añadido nuevas especialidades  a la base de datos");
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

		public async Task<bool> UpdateSpecialty(Specialty specialty)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				Specialty? currentSpecialty = await _context.Specialties
													  .Include(s => s.Unities)
													  .Where(u => u.Id == specialty.Id).FirstOrDefaultAsync();
				if (currentSpecialty != null)
				{
					currentSpecialty.Description = specialty.Description;
					currentSpecialty.Name = specialty.Name;
					currentSpecialty.MaxGuards = specialty.MaxGuards;

					//actualizamos las unidades existentes
					List<Unity> unities = currentSpecialty.Unities.ToList();
					foreach (Unity un in unities)
					{
						Unity? unity = specialty.Unities.FirstOrDefault(u => u.Id == un.Id);
						if (unity == null)
						{
							currentSpecialty.Unities.Remove(un);
							_context.Unities.Remove(un);
						}
						else
						{
							currentSpecialty.Unities.First(u => u.Id == unity.Id).Name = unity.Name;
							currentSpecialty.Unities.First(u => u.Id == unity.Id).Description = unity.Description;
							currentSpecialty.Unities.First(u => u.Id == unity.Id).MaxByDay = unity.MaxByDay;
							currentSpecialty.Unities.First(u => u.Id == unity.Id).MaxByDayWeekend = unity.MaxByDayWeekend;
						}
					}

					//creamos las unidades nuevas
					specialty.Unities.Where(u => u.Id == 0).ToList().ForEach(u => currentSpecialty.Unities.Add(u));
					result = await _context.SaveChangesAsync() != 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha actualizado la especialidad en la base de datos");
				}
				else
				{
					LogClass.WriteLog(ErrorWrite.Info, "No se ha encontrado la especialidad en la base de datos");
					result = false;
				}
				return result;
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat("Se ha producido un error en {0} de {1} al actaulizar la especialidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), specialty.Id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}

		public async Task<bool> DeleteSpecialty(int id)
		{
			bool result = true;
			try
			{
				//primero se comprueba que no exista otra con el mismo nombre
				Specialty? specialty = await _context.Specialties
													 .Include(s => s.Unities)
													 .Where(s => s.Id == id).FirstOrDefaultAsync();
				if (specialty != null)
				{
					_context.Specialties.Remove(specialty);
					result = await _context.SaveChangesAsync() > 0;
					LogClass.WriteLog(ErrorWrite.Info, "Se ha eliminado una especialidad a la base de datos");
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
				sb.AppendFormat("Se ha producido un error en {0} de {1} al eliminar la especialidad con id {2}. La traza es: {3}: ",
								this.GetType().Name, MethodBase.GetCurrentMethod(), id, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}

		}
		#endregion
	}
}
