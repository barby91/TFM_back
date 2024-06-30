using onGuardManager.Bussiness.IService;
using onGuardManager.Logger;
using System.Text;
using onGuardManager.Data.IRepository;
using onGuardManager.Models.DTO.Entities;
using onGuardManager.Models.DTO.Models;
using onGuardManager.Models.Entities;
using onGuardManager.Models.DTO.Enumerados;
using Newtonsoft.Json;
using System.Collections.Immutable;

namespace onGuardManager.Bussiness.Service
{
    public class DayGuardService : IDayGuardService
	{
        #region variables
        private readonly IDayGuardRepository<DayGuard> _dayGuardRepository;
		private readonly IUserRepository<User> _userRepository;
		private readonly IPublicHolidayRepository<PublicHoliday> _publicHolidayRepository;
		private readonly ISpecialtyRepository<Specialty> _specialtyRepository;
		private readonly IUnityRepository<Unity> _unityRepository;
		private Dictionary<int, int> totalGuards;
		//private int maxWeekends = 0;
        #endregion

        #region constructor
        public DayGuardService(IDayGuardRepository<DayGuard> dayGuardRepository, IUserRepository<User> userRepository,
							   IPublicHolidayRepository<PublicHoliday> publicHolidayRepository, 
							   ISpecialtyRepository<Specialty> specialtyRepository,
							   IUnityRepository<Unity> unityRepository)
        {
            LogClass.WriteLog(ErrorWrite.Info, "se inicia DayGuadUserService");
			_dayGuardRepository = dayGuardRepository;
			_userRepository = userRepository;
			_publicHolidayRepository = publicHolidayRepository;
			_specialtyRepository = specialtyRepository;
			_unityRepository = unityRepository;

		}
		#endregion

		#region interface

		public async Task<bool> SaveGuard(DayGuard newGuardDay)
		{
			try
			{
				return await _dayGuardRepository.SaveGuard(newGuardDay);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error al guardar las asignaciones de la guardia." +
								"La traza es: {0}: ", ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<bool> DeletePreviousGuard(int month)
		{
			try
			{
				return await _dayGuardRepository.DeletePreviousGuard(month);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error al borrar las asignaciones de la guardia del mes {0}." +
								"La traza es: {1}: ", month, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<string> GetUserStats(GuardRequest guardRequest)
		{
			//obtenemos todos los usuarios de un centro o asociados a una especialidad
			List<User> users;
			List<UserStats> userstats = new List<UserStats>();
			List<Unity> unities = new List<Unity>();
			List<PublicHoliday> publicHolidays = await _publicHolidayRepository.GetAllPublicHolidaysByCenter(guardRequest.idCenter);
			totalGuards = new Dictionary<int, int>();
			if (guardRequest.idSpecialty == 0)
			{
				users = await _userRepository.GetAllUsersByCenter(guardRequest.idCenter, true);
				List<Specialty> specialties = await _specialtyRepository.GetAllSpecialtiesWithoutCommonUnitiesByCenter(guardRequest.idCenter);
				foreach (Specialty specialty in specialties)
				{
					unities.AddRange(specialty.Unities.Where(nu => unities.TrueForAll(u => u.Id != nu.Id)));
					totalGuards.Add((int)specialty.Id, (int)specialty.MaxGuards);
				}
			}
			else
			{
				users = await _userRepository.GetAllUsersBySpecialty(guardRequest.idSpecialty);
				Specialty? specialty = await _specialtyRepository.GetSpecialtyById(guardRequest.idSpecialty);
				if (specialty != null)
				{
					unities.AddRange(specialty.Unities);
					totalGuards.Add((int)specialty.Id, (int)specialty.MaxGuards);
				}
			}

			unities.AddRange(await _unityRepository.GetAllCommonUnities());

			//recorremos cada usuario para inicializar sus estadísticas

			//obtenemos las guardias previas para 
			List<DayGuard> previousGuards;
			if (guardRequest.month == 1)
			{
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year - 1, 12);
			}
			else
			{
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year, 0);
				previousGuards = previousGuards.Where(pg => pg.Day.Month != guardRequest.month).ToList();
			}

			foreach (User user in users)
			{
				//contamos los festivos realizados por cada usuario
				List<DayGuard> userGuard = previousGuards.Where(g => g.assignedUsers.Select(a => a.Id).Contains(user.Id)).ToList();
				int totalWeekends = userGuard.Count(ug => ug.Day.DayOfWeek == DayOfWeek.Saturday ||
														  ug.Day.DayOfWeek == DayOfWeek.Sunday);
				int totalPublicHolidays = userGuard.Count(ug => publicHolidays.Select(ph => ph.Date)
												   .Contains(ug.Day));
				int totalUserGuards = userGuard.Count;
				userstats.Add(new UserStats(user, guardRequest.month == 1 ? 0 : totalUserGuards, guardRequest.month == 1 ? 0 : totalWeekends, guardRequest.month == 1 ? 0 : totalPublicHolidays));
			}
			
			if (guardRequest.month > 0)
			{
				return await AsignMonthGuards(guardRequest.month, guardRequest.idCenter, users, previousGuards,
											  unities, userstats, publicHolidays);
			}
			else
			{
				string result = "";
				bool continueLoop = true;
				int month = DateTime.Now.Month + 1;

				while(month < 12 && continueLoop)
				{
					result = await AsignMonthGuards(month, guardRequest.idCenter, users, previousGuards,
													unities, userstats, publicHolidays);
					if(!result.Contains("OK"))
					{
						continueLoop = false;
					}
					month++;
				}

				return await Task.FromResult(result);
			}
		}

		public async Task<List<DayGuardModel>> GetGuards(int idCenter, int year, int month = 0)
		{
			try
			{
				List<DayGuard> dayGuards = await _dayGuardRepository.GetGuards(idCenter, year, month);
				List<DayGuardModel> dayGuardsModel = new List<DayGuardModel>();
				foreach (DayGuard dayGuard in dayGuards)
				{
					dayGuardsModel.Add(new DayGuardModel(dayGuard));
				}
				return await Task.FromResult(dayGuardsModel.OrderBy(dgm => dgm.Day).ToList());
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error obtener las guardias del mes {0}." +
								"La traza es: {1}: ", month, ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		#endregion

		#region private methods
		private async Task<string> AsignMonthGuards(int month, int idCenter,
													List<User> users, 
													List<DayGuard> previousGuards,
													List<Unity> unities,
													List<UserStats> userstats,
													List<PublicHoliday> publicHolidays)
		{
			List<Day> days = new List<Day>();
			//recorremos los días del mes
			int totalDaysMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);

			for (int i = 0; i < totalDaysMonth; i++)
			{
				DateOnly date = new DateOnly(DateTime.Now.Year, month, i + 1);
				//calculamos los usuarios que no trabajen este día
				Dictionary<User, string> absents = new Dictionary<User, string>();

				foreach (User user in users.Where(u => u.AskedHolidays.Any(ah => ah.DateFrom <= date &&
																				ah.DateTo >= date &&
																				ah.IdStatus == (int)EnumHolidayStatus.Aprobado
																		 )
												).ToList())
				{
					absents.Add(user, "Vacaciones");
				}

				Day newDay = new Day(date, absents);
				if (i == 0 && previousGuards.Exists(pg => pg.Day == newDay.day.AddDays(-1)))
				{
					//los usuarios que estén en el último día del mes anterior son ilegibles para el día actual
					foreach (User user in previousGuards.First(pg => pg.Day == newDay.day.AddDays(-1)).assignedUsers)
					{
						newDay.absents.Add(user, "día previo");
					}
				}
				if (i < 2)
				{
					CalculateAbsentsUsersByPrevMonth(previousGuards, users, newDay);
				}

				days.Add(newDay);
			}

			//calculamos el número de festivos y fines de semana del mes
			/*int totalHoliday = days.Count(d => d.day.DayOfWeek == DayOfWeek.Sunday || d.day.DayOfWeek == DayOfWeek.Saturday) +
								publicHolidays.Count(ph => ph.Date.Month == guardRequest.month &&
														   ph.Date.DayOfWeek != DayOfWeek.Saturday &&
														   ph.Date.DayOfWeek != DayOfWeek.Sunday);
			maxWeekends = (int)Math.Round(((double)totalHoliday / (double)(userstats.Count / 6)), MidpointRounding.ToEven);*/

			if (!BacktrackingGuard(days, userstats, OrderDays(days, publicHolidays), publicHolidays, unities, 1))
			{
				return await Task.FromResult("No se pueden asignar las guardias del mes " + month);
			}
			else
			{
				if (await DeletePreviousGuard(month))
				{
					//guardamos la asignación
					bool result = true;
					foreach (Day day in days)
					{
						DayGuard guard = new DayGuard()
						{
							Day = day.day,
							assignedUsers = day.assigned
						};

						result = result && await SaveGuard(guard);
					}

					List<DayGuardModel> guards = GetGuards(idCenter, DateTime.Now.Year).Result;
					List<GuardStats> stats = new List<GuardStats>();

					foreach (DayGuardModel guard in guards)
					{
						foreach (string name in guard.assignedUsers.Select(au => au.NameSurname))
						{
							if (stats.Exists(s => s.UserName == name))
							{
								stats.First(s => s.UserName == name).GuardByUser++;
								if (guard.Day.DayOfWeek == DayOfWeek.Sunday || guard.Day.DayOfWeek == DayOfWeek.Saturday)
								{
									stats.First(s => s.UserName == name).WeekendsbyUser++;
								}
								else if (publicHolidays.Select(ph => ph.Date).Contains(guard.Day))
								{
									stats.First(s => s.UserName == name).HolidaysByUser++;
								}
							}
							else
							{
								stats.Add(new GuardStats()
								{
									UserName = name,
									GuardByUser = 1,
									HolidaysByUser = (guard.Day.DayOfWeek != DayOfWeek.Sunday &&
													  guard.Day.DayOfWeek != DayOfWeek.Saturday &&
													  publicHolidays.Select(ph => ph.Date).Contains(guard.Day)) ? 1 : 0,
									WeekendsbyUser = (guard.Day.DayOfWeek == DayOfWeek.Sunday ||
													  guard.Day.DayOfWeek == DayOfWeek.Saturday) ? 1 : 0
								});
							}
						}
					}
					if (result)
					{

						dynamic obj = new
						{
							result = "OK",
							stats = stats
						};
						return await Task.FromResult(JsonConvert.SerializeObject(obj));
					}
					else
					{
						return await Task.FromResult("Error al guardar la guardia");
					}
				}
				else
				{
					return await Task.FromResult("Error al borrar la guardia previamente calculada");
				}
			}
		}

		private void CalculateAbsentsUsersByPrevMonth(List<DayGuard> previousGuards,
											 List<User> users, Day newDay)
		{
			//R2 nunca viernes-domingo ni sábado-martes
			if (newDay.day.DayOfWeek == DayOfWeek.Sunday)
			{
				//si el día actual es sábado, los usuarios que han tenido guardia dos días antes (viernes) son ilegibles
				List<ICollection<User>> totalUsers = previousGuards.Where(ug => ug.Day == newDay.day.AddDays(-2))
																   .Select(ug => ug.assignedUsers).ToList();
				List<decimal> idUsers = new List<decimal>();
				foreach (var user in totalUsers)
				{
					idUsers.AddRange(user.Select(u => u.Id).ToList());
				}
				foreach (User user in users.Where(u => idUsers.Contains(u.Id) &&
														 !newDay.absents.Keys.Select(a => a.Id).Contains(u.Id)))
				{
					newDay.absents.Add(user, "nunca viernes-domingo");
				}
			}
			else if (newDay.day.DayOfWeek == DayOfWeek.Tuesday)
			{
				//si el día actual es martes, los usuarios que han tenido guardia tres días antes (sábado) son ilegibles
				List<ICollection<User>> totalUsers = previousGuards.Where(ug => ug.Day == newDay.day.AddDays(-3))
													  .Select(ug => ug.assignedUsers).ToList();
				List<decimal> idUsers = new List<decimal>();
				foreach (var user in totalUsers)
				{
					idUsers.AddRange(user.Select(u => u.Id).ToList());
				}
				foreach (User user in users.Where(u => idUsers.Contains(u.Id) &&
														 !newDay.absents.Keys.Select(a => a.Id).Contains(u.Id)))
				{
					newDay.absents.Add(user, "nunca sábado-martes");
				}
			}
			else
			{
				//R6 Evitar tripletes y cuatrupletes aunque técnicamente se puede
				//para la R6, también hay que añadir los que están 4 días antes y 6 días antes
				List<ICollection<User>> totalUsers = previousGuards.Where(ug => ug.Day == newDay.day.AddDays(-2) ||
																			   ug.Day == newDay.day.AddDays(-4) ||
																			   ug.Day == newDay.day.AddDays(-6))
																  .Select(ug => ug.assignedUsers).ToList();
				List<decimal> idUsers = new List<decimal>();
				foreach (var user in totalUsers)
				{
					idUsers.AddRange(user.Select(u => u.Id).ToList());
				}

				//los usuarios que han tenido guardia dos días antes, se añaden a la lista de posibles
				foreach (User user in users.Where(u => idUsers.Contains(u.Id) &&
													  !newDay.absents.Keys.Select(a => a.Id).Contains(u.Id)))
				{
					newDay.possible.Add(user, "Ha teni guardia dos días antes");
				}
			}
		}

		//private bool SearchTree(List<Day> days, List<UserStats> userStats, List<Unity> unities,
		//						List<Day> assignedDays, List<PublicHoliday> publicHolidays)
		//{
		//	if (days.Count == 0)
		//	{
		//		return true;
		//	}
		//	else
		//	{
		//		//primero ordenamos los días de mayor número de ausentes a menor
		//		Day day = days.Where(d => d.assigned.Count < 6).OrderByDescending(d => d.absents.Count + d.possible.Count).ToList()[0];
		//		LogClass.WriteLog(ErrorWrite.Info, day.day.ToString());

		//		//ordenamos  los residentes por total de fines de semana, festivos y guardias totales que
		//		//no estén en la lista de ausentes ni en la de posibles
		//		bool resultado = false;
		//		while (!resultado && userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//												  !day.assigned.Contains(u.user) &&
		//												  !day.possible.Contains(u.user))
		//									  .OrderBy(us => us.totalWeekends)
		//									  .ThenBy(us => us.totaPublicHolidays)
		//									  .ThenBy(us => us.totalGuardMonth)
		//									  .ToList().Count > 0)
		//		{
		//			resultado = PutResident(userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//														 !day.assigned.Contains(u.user) &&
		//														 !day.possible.Contains(u.user))
		//											 .OrderBy(us => us.totalWeekends)
		//											 .ThenBy(us => us.totaPublicHolidays)
		//											 .ThenBy(us => us.totalGuardMonth)
		//											 .ToList(),
		//									day, days, unities, assignedDays, publicHolidays);
		//		}

		//		//si no se han podido asignar ninguno de los residentes, se comprueban los posibles,
		//		//es decir, los que hay que evitar, salvo que no se pueda
		//		while (!resultado && userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//												  !day.assigned.Contains(u.user))
		//															  .OrderBy(us => us.totalWeekends)
		//															  .ThenBy(us => us.totaPublicHolidays)
		//															  .ThenBy(us => us.totalGuardMonth)
		//															  .ToList().Count > 0)
		//		{
		//			resultado = PutResident(userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//														 !day.assigned.Contains(u.user))
		//											 .OrderBy(us => us.totalWeekends)
		//											 .ThenBy(us => us.totaPublicHolidays)
		//											 .ThenBy(us => us.totalGuardMonth)
		//											 .ToList(),
		//									day, days, unities, assignedDays, publicHolidays);
		//		}

		//		if (!resultado)
		//		{
		//			return false;
		//		}
		//		else
		//		{
		//			assignedDays.Add(day);
		//			return SearchTree(days, userStats, unities, assignedDays, publicHolidays);
		//		}
		//	}
		//}

		//private bool PutResident(List<UserStats> userStats, Day day, List<Day> days, List<Unity> unities,
		//						 List<Day> assignedDays, List<PublicHoliday> publicHolidays)
		//{
		//	bool result;
		//	List<Day> originalDays = CopyList(days);
		//	Day originalDay = (Day)day.Clone();
		//	if (userStats.Count == 0)
		//	{
		//		result = false;
		//	}
		//	else
		//	{
		//		UserStats? r;
		//		//Si aún no hemos asignado los 5 escogemos el primero de los óptimos cuyo level no coincida con uno ya escogido
		//		if (day.assigned.Count < 5)
		//		{
		//			Dictionary<decimal, int> totalUnity = new Dictionary<decimal, int>();
		//			foreach(decimal un in userStats.Select(u => u.user.IdUnity))
		//			{
		//				if(totalUnity.ContainsKey(un))
		//				{
		//					totalUnity[un] += 1;
		//				}
		//				else
		//				{
		//					totalUnity.Add(un, 1);
		//				}
		//			}

		//			r = userStats.Find(us => !day.assigned.Select(a => a.IdLevel).Contains(us.user.IdLevel) && us.user.IdUnity == totalUnity.OrderBy(t => t.Value).First().Key);
		//		}
		//		else
		//		{
		//			r = userStats[0];
		//		}
		//		if (r != null)
		//		{
		//			//Se actualizan las estadísitcas del usuario
		//			r.totalGuardMonth++;
		//			if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday)
		//			{
		//				r.totalWeekends++;
		//			}
		//			//Si el día esta entre la lista de festivos se suma uno a los festivos
		//			if (publicHolidays.Exists(ph => ph.Date == day.day))
		//			{
		//				r.totaPublicHolidays++;
		//			}

		//			//Se asigna al usuario al día y se elimina de la lista de posibles si estuviera
		//			day.assigned.Add(r.user);
		//			day.possible.Remove(r.user);

		//			//se añaden a la lista de ausentes los usuarios que no cumplan alguna regla o a la de posibles
		//			//si hay que evitarlos
		//			CheckRules(r, userStats, day, days, unities, assignedDays);

		//			if (day.assigned.Count < 6)
		//			{
		//				result = PutResident(userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//														  !day.assigned.Contains(u.user) &&
		//														  !day.possible.Contains(u.user))
		//											.OrderBy(us => us.totalWeekends)
		//											.ThenBy(us => us.totaPublicHolidays)
		//											.ThenBy(us => us.totalGuardMonth)
		//											.ToList(), day, days, unities, assignedDays, publicHolidays);
		//			}
		//			else
		//			{
		//				result = true;
		//			}

		//			if (!result)
		//			{
		//				//si no se ha podido colocar el primer residente de forma exitosa, se añade a la lista de ausentes,
		//				//ya que con él en la lista de asignados no se obtiene un resultado y se deshacen los cambios que 
		//				//se hayan hecho
		//				foreach (User user in day.absents.Keys)
		//				{
		//					if (!originalDay.absents.Keys.Select(aa => aa.Id).Contains(user.Id))
		//					{
		//						day.absents.Remove(user);
		//					}
		//				}
		//				day.possible.RemoveAll(p => !originalDay.possible.Select(pp => pp.Id).Contains(p.Id));
		//				day.assigned.RemoveAll(a => !originalDay.assigned.Select(aa => aa.Id).Contains(a.Id));
		//				day.absents.Add(r.user, "No lleva a solución");

		//				//se actualizan las estadísiticas del usuario
		//				//Se actualizan las estadísitcas del usuario
		//				r.totalGuardMonth--;
		//				if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday)
		//				{
		//					r.totalWeekends--;
		//				}
		//				//Si el día esta entre la lista de festivos se suma uno a los festivos
		//				if (publicHolidays.Exists(ph => ph.Date == day.day))
		//				{
		//					r.totaPublicHolidays--;
		//				}

		//				//no se ha podido asignar uno, probamos con la lista de posibles
		//				result = PutResident(userStats.Where(u => !day.absents.Keys.Contains(u.user) &&
		//														  !day.assigned.Contains(u.user))
		//											.OrderBy(us => us.totalWeekends)
		//											.ThenBy(us => us.totaPublicHolidays)
		//											.ThenBy(us => us.totalGuardMonth)
		//											.ToList(), day, days, unities, assignedDays, publicHolidays);
		//			}
		//		}
		//		else
		//		{
		//			foreach (User user in userStats.Select(us => us.user).ToList())
		//			{
		//				day.absents.Add(user, "niveles repetidos");
		//			}
		//			result = false;
		//		}
		//	}

		//	return result;
		//}

		//private void CheckRules(UserStats r, List<UserStats> userStats, Day day,
		//						List<Day> days, List<Unity> unities, List<Day> assignedDays)
		//{
		//	//R1 Nunca dos días seguidos de guardia
		//	if (days.Exists(d => d.day == day.day.AddDays(1)) && !days.First(d => d.day == day.day.AddDays(1)).assigned.Contains(r.user) &&
		//		!days.First(d => d.day == day.day.AddDays(1)).absents.Keys.Contains(r.user))
		//	{
		//		days.First(d => d.day == day.day.AddDays(1)).absents.Add(r.user, "nunca 2 días seguidos (dia siguiente)");
		//		//si estuviera en la lista de posibles, se elimina
		//		days.First(d => d.day == day.day.AddDays(1)).possible.Remove(r.user);
		//	}
		//	if (days.Exists(d => d.day == day.day.AddDays(-1)) && !days.First(d => d.day == day.day.AddDays(-1)).assigned.Contains(r.user) &&
		//		!days.First(d => d.day == day.day.AddDays(-1)).absents.Keys.Contains(r.user))
		//	{
		//		days.First(d => d.day == day.day.AddDays(-1)).absents.Add(r.user, "nunca 2 días seguidos (día anterior)");
		//		//si estuviera en la lista de posibles, se elimina
		//		days.First(d => d.day == day.day.AddDays(-1)).possible.Remove(r.user);
		//	}

		//	//R2 Se puede dobletes (guardia-libre-guardia) pero evitarlos en la medida de lo posible y
		//	//nunca viernes-domingo ni sábado-martes
		//	if (day.day.DayOfWeek == DayOfWeek.Friday)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(2)) && !days.First(d => d.day == day.day.AddDays(2)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(2)).absents.Add(r.user, "nunca viernes-domingo");
		//		}
		//	}
		//	else if (day.day.DayOfWeek == DayOfWeek.Saturday)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(3)) && !days.First(d => d.day == day.day.AddDays(3)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(3)).absents.Add(r.user, "nunca sábado-martes");
		//		}
		//	}
		//	else if (day.day.DayOfWeek == DayOfWeek.Sunday)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(-2)) && !days.First(d => d.day == day.day.AddDays(-2)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(-2)).absents.Add(r.user, "nunca viernes-domingo");
		//		}
		//	}
		//	else if (day.day.DayOfWeek == DayOfWeek.Tuesday)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(-3)) && !days.First(d => d.day == day.day.AddDays(-3)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(-3)).absents.Add(r.user, "nunca sábado-martes");
		//		}
		//	}
		//	else
		//	{
		//		//se añade este usuario a la lista de posibles para +2 días y para -2 días
		//		if (days.Exists(d => d.day == day.day.AddDays(2)) && days.First(d => d.day == day.day.AddDays(2)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(2)).possible.Add(r.user, "Ha tenido guardia dos días antes");
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(-2)) && !days.First(d => d.day == day.day.AddDays(-2)).absents.Keys.Contains(r.user))
		//		{
		//			days.First(d => d.day == day.day.AddDays(-2)).possible.Add(r.user, "Ha tenido guardia dos días después");
		//		}
		//	}

		//	//R3 Nunca mas de 2 de la misma unidad de guardia, salvo de esófago gástrica que solo
		//	//se puede 1, de lunes a jueves. Sábado y domingo no mas de 3 de la misma unidad. 
		//	if (((day.day.DayOfWeek == DayOfWeek.Monday || day.day.DayOfWeek == DayOfWeek.Tuesday ||
		//		  day.day.DayOfWeek == DayOfWeek.Wednesday || day.day.DayOfWeek == DayOfWeek.Thursday)
		//		&& ((unities.Exists(u => u.Name.ToLower()
		//									   .Replace("á", "a")
		//									   .Replace("é", "e")
		//									   .Replace("í", "i")
		//									   .Replace("ó", "o")
		//									   .Replace("ú", "u")
		//									   .Replace(" ", "")
		//									   .Equals("esofagogastrica") &&
		//								 u.IdSpecialty == r.user.IdSpecialty) &&
		//			 r.user.IdUnity == unities.Find(u => u.Name.ToLower()
		//													   .Replace("á", "a")
		//													   .Replace("é", "e")
		//													   .Replace("í", "i")
		//													   .Replace("ó", "o")
		//													   .Replace("ú", "u")
		//													   .Replace(" ", "")
		//													   .Equals("esofagogastrica") &&
		//												 u.IdSpecialty == r.user.IdSpecialty)?.Id)
		//			|| (day.assigned.Count(a => a.IdSpecialty == r.user.IdSpecialty &&
		//										a.IdUnity == r.user.IdUnity) == 2)))
		//		|| ((day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday)
		//			&& day.assigned.Count(a => a.IdSpecialty == r.user.IdSpecialty &&
		//									   a.IdUnity == r.user.IdUnity) == 3))
		//	{
		//		//El resto de residentes de la misma unidad quedan inelegibles para este día
		//		foreach (User user in userStats.Where(u => u.user.IdSpecialty == r.user.IdSpecialty &&
		//												  u.user.IdUnity == r.user.IdUnity &&
		//												  !day.assigned.Contains(u.user) &&
		//												  !day.absents.Keys.Contains(u.user))
		//									  .Select(u => u.user))
		//		{
		//			day.absents.Add(user, "no más de dos de la misma unidad");
		//		}
		//	}

		//	//R4 Máx 6 guardias por cabeza, aunque el máximo legal son 7
		//	if (days.Count(d => d.assigned.Exists(u => u.Id == r.user.Id)) == 6)
		//	{
		//		//queda inelegible para el resto de días
		//		foreach (Day d in days.Where(d => d.assigned.Count < 6))
		//		{
		//			if (!d.absents.Keys.Contains(r.user))
		//			{
		//				d.absents.Add(r.user, "6 guardias asignadas");
		//			}
		//		}
		//	}

		//	//R5 Siempre uno de cada año de guardia. Añadimos todos los residentes del mismo año que el elegido
		//	//a la lista de inelegibles, salvo el que esté rotando.
		//	decimal idRotatorio = unities.First(u => u.Name == "Rotatorio").Id;
		//	foreach (User user in userStats.Where(u => u.user.IdLevel == r.user.IdLevel &&
		//											  ((r.user.IdUnity != idRotatorio && u.user.IdUnity != idRotatorio) ||
		//											   (r.user.IdUnity == idRotatorio && u.user.IdUnity == idRotatorio)) &&
		//											  !day.assigned.Contains(u.user) &&
		//											  !day.absents.Keys.Contains(u.user))
		//								  .Select(u => u.user))
		//	{
		//		day.absents.Add(user, "nivel ya asignado al día");
		//	}

		//	//si hubiera alguno de los inelegibles en la lista de posibles lo quitamos
		//	foreach (decimal uId in day.absents.Keys.Select(u => u.Id))
		//	{
		//		if (day.possible.Find(p => p.Id == uId) != null)
		//		{
		//			day.possible.Remove(day.possible.First(p => p.Id == uId));
		//		}
		//	}

		//	//R6 R5 y R4 de la misma unidad no deberían coincidir a ser posible.
		//	if (r.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
		//	{
		//		//todos los de la misma unidad de R5 deben ir a posibles
		//		day.possible.AddRange(userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R5") &&
		//												   u.user.IdUnity == r.user.IdUnity &&
		//												   !day.absents.Keys.Contains(u.user) &&
		//												   !day.possible.Contains(u.user) &&
		//												   !day.assigned.Contains(u.user))
		//									   .Select(u => u.user));

		//	}
		//	if (r.user.IdLevelNavigation.Name.ToUpper().Equals("R5"))
		//	{
		//		//todos los de la misma unidad de R4 deben ir a posibles
		//		day.possible.AddRange(userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
		//												   u.user.IdUnity == r.user.IdUnity &&
		//												   !day.absents.Keys.Contains(u.user) &&
		//												   !day.possible.Contains(u.user) &&
		//												   !day.assigned.Contains(u.user))
		//									   .Select(u => u.user));

		//	}

		//	//R7 R3 y R4 de pared (unidad de endocrino) no deberían coincidir
		//	if (unities.Exists(u => u.IdSpecialty == r.user.IdSpecialty && u.Name.ToLower().Equals("endocrino")))
		//	{
		//		if (r.user.IdUnity == unities.First(u => u.IdSpecialty == r.user.IdSpecialty &&
		//												 u.Name.ToLower().Equals("endocrino"))?.Id &&
		//			r.user.IdLevelNavigation.Name.ToUpper().Equals("R3"))
		//		{
		//			//todos los de la misma unidad de R5 deben ir a posibles
		//			day.possible.AddRange(userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
		//													   u.user.IdUnity == r.user.IdUnity &&
		//													   !day.absents.Keys.Contains(u.user) &&
		//													   !day.possible.Contains(u.user) &&
		//													   !day.assigned.Contains(u.user))
		//										   .Select(u => u.user));

		//		}
		//		if (r.user.IdUnity == unities.First(u => u.IdSpecialty == r.user.IdSpecialty &&
		//												 u.Name.ToLower().Equals("endocrino"))?.Id &&
		//			r.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
		//		{
		//			//todos los de la misma unidad de R4 deben ir a posibles
		//			day.possible.AddRange(userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R3") &&
		//													   u.user.IdUnity == r.user.IdUnity &&
		//													   !day.absents.Keys.Contains(u.user) &&
		//													   !day.possible.Contains(u.user) &&
		//													   !day.assigned.Contains(u.user))
		//										   .Select(u => u.user));
		//		}
		//	}

		//	//R8 Evitar tripletes y cuatrupletes aunque técnicamente se puede
		//	//tripletes
		//	if (assignedDays.Find(d => d.day == day.day.AddDays(2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id) &&
		//					   !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id) &&
		//						   !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}

		//	if (assignedDays.Find(d => d.day == day.day.AddDays(-2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(-4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						 !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						 && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}

		//	//cuatrupletes
		//	if (assignedDays.Find(d => d.day == day.day.AddDays(2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(6) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(6) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(6) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						 !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						 && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(6) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}

		//	if (assignedDays.Find(d => d.day == day.day.AddDays(2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(-2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(-2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}

		//	if (assignedDays.Find(d => d.day == day.day.AddDays(-2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(-4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null)
		//	{
		//		if (days.Exists(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//		if (days.Exists(d => d.day == day.day.AddDays(-6) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id)))
		//		{
		//			days.Find(d => d.day == day.day.AddDays(-6) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						   && !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//		}
		//	}

		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(-4) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(-6) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						 !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						 && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(-2) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}
		//	if ((assignedDays.Find(d => d.day == day.day.AddDays(-2) && d.assigned.Exists(u => u.Id == r.user.Id)) != null &&
		//		assignedDays.Find(d => d.day == day.day.AddDays(-6) && d.assigned.Exists(u => u.Id == r.user.Id)) != null) &&
		//	   (days.Exists(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//					   !d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//					   && !d.possible.Exists(u => u.Id == r.user.Id))))
		//	{
		//		days.Find(d => d.day == day.day.AddDays(-4) && !d.assigned.Exists(u => u.Id == r.user.Id) &&
		//						!d.absents.Keys.ToList().Exists(u => u.Id == r.user.Id)
		//						&& !d.possible.Exists(u => u.Id == r.user.Id))?.possible.Add(r.user);
		//	}
		//}

		//private List<Day> CopyList(List<Day> list)
		//{
		//	List<Day> copyList = new List<Day>();

		//	foreach (Day original in list)
		//	{
		//		copyList.Add((Day)original.Clone());
		//	}

		//	return copyList;
		//}

		#endregion
		
		#region backtracking

		bool BacktrackingGuard(List<Day> days, List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays, List<Unity> unities, int level)
		{
			if(!days.Exists(d => d.assigned.Count < 6))
			{
				return true;
			}
			else
			{
				if(day.assigned.Count == 6)
				{
					//buscamos el siguiente día 
					Day newDay = OrderDays(days, publicHolidays);
					return BacktrackingGuard(days, userStats, newDay, publicHolidays, unities, 1);
				}
				else
				{
					//cogemos los usuarios posibles, mientras no haya 5 asignados se coge el más optimo
					//cuyo nivel no se haya insertado ya
					List<UserStats> users;

					//si el día es fin de semana o festivo se ordena por el que menos número de fines de semana y festivos tenga
					users = GetUsersOrdered(userStats, day, publicHolidays);

					//while(users.Count > 0)
					foreach(UserStats us in users)
					{
						//UserStats us = users[0];
						/*if (!day.possible.Contains(us.user))
						{ */
						day.assigned.Add(us.user);
						UpdateStatics(us, day, publicHolidays);
						UpdateAbsentsLists(days, day, us, unities, userStats, publicHolidays);
						if (BacktrackingGuard(days, userStats, day, publicHolidays, unities, level == 5 ? 0 : level + 1))
						{
							return true;
						}
						RevertStatics(us, day, publicHolidays);
						RevertLists(days, day, us, unities, userStats, publicHolidays);
						day.assigned.Remove(us.user);
						if (!day.absents.ContainsKey(us.user))
						{
							day.absents.Add(us.user, "No llega a una solución final");
						}
							//users = GetUsersOrdered(level, users, day, publicHolidays);
						//}

						//users.Remove(us);
					}

					//probamos con los posibles
					/*users = GetPossibleUsersOrdered(level, userStats, day, publicHolidays);
					if(users.Count > 0)
					{
						//boramos los usuarios que se han descartado, puede que funcione
						foreach (var pairvalue in day.absents.Where(d => d.Value.Equals("No llega a una solución final")))
						{
							day.absents.Remove(pairvalue.Key);
						}
					}
					//users = GetUsersOrdered(level, userStats, day, publicHolidays);
					//while (users.Count > 0)
					foreach (UserStats us in users)
					{
						//UserStats us = users[0];
						//if (day.possible.Contains(us.user))
						//{
							day.assigned.Add(us.user);
							UpdateStatics(us, day, publicHolidays);
							UpdateAbsentsLists(days, day, us, unities, userStats, publicHolidays);
							UpdatePossiblesLists(days, day, us, unities, userStats);
							if (BacktrackingGuard(days, userStats, day, publicHolidays, unities, level == 5? 0 : level + 1))
							{
								return true;
							}
							else
							{
								RevertStatics(us, day, publicHolidays);
								RevertLists(days, day, us, unities, userStats, publicHolidays);
								day.assigned.Remove(us.user);
								if (!day.absents.ContainsKey(us.user))
								{
									day.absents.Add(us.user, "No llega a una solución final");
								}
							}
							//users = GetUsersOrdered(level, users, day, publicHolidays);
						//}

						//users.Remove(us);
					}*/

					return false;
				}
			}
		}

		/*private List<UserStats> GetUsersOrdered(int level, List<UserStats>userStats, Day day, List<PublicHoliday> publicHolidays)
		{
			List<UserStats> users;
			if(day.assigned.Count < 5)
			{
				//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
				users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
			}
			else
			{
				//cogemos el usaurio más óptimo de los restantes
				users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
			}
			/*if (level != 0)
			{
				users = userStats.Where(u => u.user.IdLevelNavigation.Name == "R" + level &&
											 !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
			}
			else
			{
				users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
			}*/

		/*if(users.Count == 0)
		{
			if (day.assigned.Count < 5)
			{
				//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
				users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
			}
			else
			{
				//cogemos el usaurio más óptimo de los restantes
				users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
			}
		}

		users = (day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday
				|| publicHolidays.Select(ph => ph.Date).Contains(day.day)) ?
				users.OrderBy(us => us.totalWeekends + us.totaPublicHolidays).ToList() :
				users.OrderBy(us => us.totalGuardMonth).ToList();

		return users;
	}*/

		private List<UserStats> GetUsersOrdered(List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays)
		{
			List<UserStats> users;
			if (day.assigned.Count < 5)
			{
				//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
				users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
			}
			else
			{
				//cogemos el usaurio más óptimo de los restantes
				users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											 !day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											 !day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
			}

			//si no queda ninguno de los usuarios que cumplen todas las reglas, cogemos los posibles
			if (users.Count == 0)
			{
				if (day.assigned.Count < 5)
				{
					//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
					users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
												 !day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
				}
				else
				{
					//cogemos el usaurio más óptimo de los restantes
					users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
				}
			}

			users = (day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday
					|| publicHolidays.Select(ph => ph.Date).Contains(day.day)) ?
					users.OrderBy(us => us.totalWeekends + us.totaPublicHolidays).ToList() :
					users.OrderBy(us => us.totalGuardMonth).ToList();

			return users;
		}

		//private List<UserStats> GetPossibleUsersOrdered(int level, List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays)
		//{
		//	List<UserStats> users;
		//	if (day.assigned.Count < 5)
		//	{
		//		//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
		//		users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
		//									 !day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
		//	}
		//	else
		//	{
		//		//cogemos el usaurio más óptimo de los restantes
		//		users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
		//	}
		//	/*if (level != 0)
		//	{
		//		users = userStats.Where(u => u.user.IdLevelNavigation.Name == "R" + level &&
		//									 day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
		//	}
		//	else
		//	{
		//		users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
		//	}*/
		//	users = (day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday
		//			|| publicHolidays.Select(ph => ph.Date).Contains(day.day)) ?
		//			users.OrderBy(us => us.totalWeekends + us.totaPublicHolidays).ToList() :
		//			users.OrderBy(us => us.totalGuardMonth).ToList();

		//	return users;
		//}

		private void UpdateStatics(UserStats us, Day day, List<PublicHoliday> publicHolidays)
		{
			//Se actualizan las estadísitcas del usuario
			us.totalGuardMonth++;
			if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday)
			{
				us.totalWeekends++;
			}
			//Si el día esta entre la lista de festivos se suma uno a los festivos
			if (publicHolidays.Exists(ph => ph.Date == day.day) && day.day.DayOfWeek != DayOfWeek.Sunday && day.day.DayOfWeek != DayOfWeek.Saturday)
			{
				us.totaPublicHolidays++;
			}
		}

		private void UpdateAbsentsLists(List<Day> days, Day day, UserStats us, List<Unity> unities, List<UserStats> userStats, 
										List<PublicHoliday> publicHolidays)
		{
			//R1 Nunca dos días seguidos de guardia
			if (days.Exists(d => d.day == day.day.AddDays(1)) && 
				!days.First(d => d.day == day.day.AddDays(1)).absents.Keys.Contains(us.user))
			{
				days.First(d => d.day == day.day.AddDays(1)).absents.Add(us.user, "nunca 2 días seguidos (dia siguiente)");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-1)) && 
				!days.First(d => d.day == day.day.AddDays(-1)).assigned.Contains(us.user) &&
				!days.First(d => d.day == day.day.AddDays(-1)).absents.Keys.Contains(us.user))
			{
				days.First(d => d.day == day.day.AddDays(-1)).absents.Add(us.user, "nunca 2 días seguidos (día anterior)");
			}

			//R2 Se puede dobletes (guardia-libre-guardia) pero evitarlos en la medida de lo posible y
			//nunca viernes-domingo ni sábado-martes
			if (day.day.DayOfWeek == DayOfWeek.Friday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(2)) && 
					!days.First(d => d.day == day.day.AddDays(2)).absents.Keys.Contains(us.user))
				{
					days.First(d => d.day == day.day.AddDays(2)).absents.Add(us.user, "nunca viernes-domingo");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Saturday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(3)) && 
					!days.First(d => d.day == day.day.AddDays(3)).absents.Keys.Contains(us.user))
				{
					days.First(d => d.day == day.day.AddDays(3)).absents.Add(us.user, "nunca sábado-martes");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Sunday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(-2)) && 
					!days.First(d => d.day == day.day.AddDays(-2)).absents.Keys.Contains(us.user))
				{
					days.First(d => d.day == day.day.AddDays(-2)).absents.Add(us.user, "nunca viernes-domingo");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Tuesday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(-3)) && 
					!days.First(d => d.day == day.day.AddDays(-3)).absents.Keys.Contains(us.user))
				{
					days.First(d => d.day == day.day.AddDays(-3)).absents.Add(us.user, "nunca sábado-martes");
				}
			}

			//R3 Nunca mas de 2 de la misma unidad de guardia, salvo de esófago gástrica que solo
			//se puede 1, de lunes a jueves. Sábado y domingo no mas de 3 de la misma unidad. 
			if (((day.day.DayOfWeek == DayOfWeek.Monday || day.day.DayOfWeek == DayOfWeek.Tuesday ||
				  day.day.DayOfWeek == DayOfWeek.Wednesday || day.day.DayOfWeek == DayOfWeek.Thursday)
				&& day.assigned.Count < 6
				&& ((unities.Exists(u => u.Name.ToLower()
											   .Replace("á", "a")
											   .Replace("é", "e")
											   .Replace("í", "i")
											   .Replace("ó", "o")
											   .Replace("ú", "u")
											   .Replace(" ", "")
											   .Equals("esofagogastrica") &&
										 u.IdSpecialty == us.user.IdSpecialty) &&
					 us.user.IdUnity == unities.Find(u => u.Name.ToLower()
															   .Replace("á", "a")
															   .Replace("é", "e")
															   .Replace("í", "i")
															   .Replace("ó", "o")
															   .Replace("ú", "u")
															   .Replace(" ", "")
															   .Equals("esofagogastrica") &&
														 u.IdSpecialty == us.user.IdSpecialty)?.Id)
					|| (day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
												a.IdUnity == us.user.IdUnity) == 2)))
				|| ((day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday)
					&& day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
											   a.IdUnity == us.user.IdUnity) == 3))
			{
				//El resto de residentes de la misma unidad quedan inelegibles para este día
				foreach (User user in userStats.Where(u => u.user.IdSpecialty == us.user.IdSpecialty &&
														  u.user.IdUnity == us.user.IdUnity &&
														  !day.assigned.Contains(u.user) &&
														  !day.absents.Keys.Contains(u.user))
											  .Select(u => u.user))
				{
					day.absents.Add(user, "no más de dos de la misma unidad");
				}
			}

			//R4 Máx 6 guardias por cabeza, aunque el máximo legal son 7
			if (days.Count(d => d.assigned.Exists(u => u.Id == us.user.Id)) == totalGuards[(int)us.user.IdSpecialty])
			{
				//queda inelegible para el resto de días pero no para el propio
				foreach (Day d in days.Where(d => d.day != day.day && !d.assigned.Contains(us.user)))
				{
					if (!d.absents.Keys.Contains(us.user))
					{
						d.absents.Add(us.user, totalGuards+" guardias asignadas");
					}
				}
			}

			//si ha alcanzado al méximo de fines de semana/festivos queda ilegible para otros fines de semana y festivos
			/*if(us.totalWeekends + us.totaPublicHolidays == maxWeekends)
			{
				foreach(Day dd in days.Where(d => !d.assigned.Contains(us.user) &&
												  !d.absents.Keys.Contains(us.user) && 
												  (d.day.DayOfWeek == DayOfWeek.Saturday || d.day.DayOfWeek == DayOfWeek.Sunday ||
												  publicHolidays.Select(ph => ph.Date).Contains(d.day))))
				{
					dd.absents.Add(us.user, "fines de semana máximos");
				}
			}*/
			//R5 Siempre uno de cada año de guardia. Añadimos todos los residentes del mismo año que el elegido
			//a la lista de inelegibles, salvo el que esté rotando.
			/*decimal idRotatorio = unities.First(u => u.Name == "Rotatorio").Id;
			foreach (User user in userStats.Where(u => u.user.IdLevel == us.user.IdLevel &&
													  ((us.user.IdUnity != idRotatorio && u.user.IdUnity != idRotatorio) ||
													   (us.user.IdUnity == idRotatorio && u.user.IdUnity == idRotatorio)) &&
													  !day.assigned.Contains(u.user) &&
													  !day.absents.Keys.Contains(u.user))
										  .Select(u => u.user))
			{
				day.absents.Add(user, "nivel ya asignado al día");
			}
			*/
			//si hubiera alguno de los inelegibles en la lista de posibles lo quitamos
			foreach (User key in day.absents.Keys.Where(k => day.possible.ContainsKey(k)))
			{
				day.possible.Remove(key);
			}

			UpdatePossiblesLists(days, day, us, unities, userStats);
		}

		private Day OrderDays(List<Day> days, List<PublicHoliday> publicHolidays)
		{
			//primero cogemos los festivos, sábados y domingos
			/*Day? day = days.Where(d => d.assigned.Count < 6 &&
							(d.day.DayOfWeek == DayOfWeek.Saturday ||
							 d.day.DayOfWeek == DayOfWeek.Sunday ||
							 publicHolidays.Select(ph => ph.Date).Contains(d.day)))
				.OrderBy(d => d.absents.Count).FirstOrDefault();

			if (day == null)
			{
				day = days.Where(d => d.assigned.Count < 6 &&
									  d.day.DayOfWeek != DayOfWeek.Saturday &&
									  d.day.DayOfWeek != DayOfWeek.Sunday &&
									  !publicHolidays.Select(ph => ph.Date).Contains(d.day))
						  .OrderBy(d => d.absents.Count).First();
			}

			return day;*/
			return days.Where(d => d.assigned.Count < 6).OrderByDescending(d => d.absents.Count).First();
		}

		private void UpdatePossiblesLists(List<Day> days, Day day, UserStats us, List<Unity> unities, List<UserStats> userStats)
		{
			//R6 R5 y R4 de la misma unidad no deberían coincidir a ser posible.
			if (us.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
			{
				//todos los de la misma unidad de R5 deben ir a posibles
				foreach (User key in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R5") &&
														   u.user.IdUnity == us.user.IdUnity &&
														   !day.absents.Keys.Contains(u.user) &&
														   !day.possible.Keys.Contains(u.user) &&
														   !day.assigned.Contains(u.user))
											   .Select(u => u.user))
				{
					day.possible.Add(key, "hay un R4 de la misma unidad");
				}

			}
			if (us.user.IdLevelNavigation.Name.ToUpper().Equals("R5"))
			{
				//todos los de la misma unidad de R4 deben ir a posibles
				foreach (User key in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
														   u.user.IdUnity == us.user.IdUnity &&
														   !day.absents.Keys.Contains(u.user) &&
														   !day.possible.Keys.Contains(u.user) &&
														   !day.assigned.Contains(u.user))
											   .Select(u => u.user))
				{
					day.possible.Add(key, "hay un R5 de la misma unidad");
				}

			}

			//R7 R3 y R4 de pared (unidad de endocrino) no deberían coincidir
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Equals("endocrino")))
			{
				if (us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														 u.Name.ToLower().Equals("endocrino"))?.Id &&
					us.user.IdLevelNavigation.Name.ToUpper().Equals("R3"))
				{
					//todos los de la misma unidad de R5 deben ir a posibles
					foreach (User key in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
															   u.user.IdUnity == us.user.IdUnity &&
															   !day.absents.Keys.Contains(u.user) &&
															   !day.possible.Keys.Contains(u.user) &&
															   !day.assigned.Contains(u.user))
												   .Select(u => u.user))
					{
						day.possible.Add(key, "Hay un R3 de endocrino");
					}

				}
				if (us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														 u.Name.ToLower().Equals("endocrino"))?.Id &&
					us.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
				{
					//todos los de la misma unidad de R4 deben ir a posibles
					foreach (User key in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R3") &&
															   u.user.IdUnity == us.user.IdUnity &&
															   !day.absents.Keys.Contains(u.user) &&
															   !day.possible.Keys.Contains(u.user) &&
															   !day.assigned.Contains(u.user))
												   .Select(u => u.user))
					{
						day.possible.Add(key, "Hay un R4 de endocrino");
					}
				}
			}

			//R2 evitar dobletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)))
			{
				days.Find(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "posible doblete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)))
			{
				days.Find(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "posible doblete");
			}

			//R8 Evitar tripletes y cuatrupletes aunque técnicamente se puede
			//tripletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
							    d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								!d.assigned.Contains(us.user) &&
								!d.possible.ContainsKey(us.user) &&
								!d.absents.ContainsKey(us.user))
				)
			{
				days.Find(d => d.day == day.day.AddDays(4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible triplete");
			}

			//cuatripletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(6) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-6) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(4) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
			if (days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))?.possible.Add(us.user, "Posible cuadriplete");
			}
		}

		private void RevertStatics(UserStats us, Day day, List<PublicHoliday> publicHolidays)
		{
			//Se actualizan las estadísitcas del usuario
			us.totalGuardMonth--;
			if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday)
			{
				us.totalWeekends--;
			}
			//Si el día esta entre la lista de festivos se suma uno a los festivos
			if (publicHolidays.Exists(ph => ph.Date == day.day) && day.day.DayOfWeek != DayOfWeek.Sunday && day.day.DayOfWeek != DayOfWeek.Saturday)
			{
				us.totaPublicHolidays--;
			}
		}

		private void RevertLists(List<Day> days, Day day, UserStats us, List<Unity> unities, List<UserStats> userStats,
								 List<PublicHoliday> publicHolidays)
		{
			//R1 Nunca dos días seguidos de guardia
			if (days.Exists(d => d.day == day.day.AddDays(1)) &&
				days.First(d => d.day == day.day.AddDays(1)).absents.Keys.Contains(us.user) &&
				days.First(d => d.day == day.day.AddDays(1)).absents[us.user] == "nunca 2 días seguidos (dia siguiente)")
			{
				days.First(d => d.day == day.day.AddDays(1)).absents.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-1)) &&
				days.First(d => d.day == day.day.AddDays(-1)).absents.Keys.Contains(us.user) &&
				days.First(d => d.day == day.day.AddDays(-1)).absents[us.user] == "nunca 2 días seguidos (dia siguiente)")
			{
				days.First(d => d.day == day.day.AddDays(-1)).absents.Remove(us.user);
			}

			//R2 Se puede dobletes (guardia-libre-guardia) pero evitarlos en la medida de lo posible y
			//nunca viernes-domingo ni sábado-martes
			if (day.day.DayOfWeek == DayOfWeek.Friday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(2)) &&
					days.First(d => d.day == day.day.AddDays(2)).absents.Keys.Contains(us.user) &&
					days.First(d => d.day == day.day.AddDays(2)).absents[us.user] == "nunca viernes-domingo")
				{
					days.First(d => d.day == day.day.AddDays(2)).absents.Remove(us.user);
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Saturday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(3)) &&
					days.First(d => d.day == day.day.AddDays(3)).absents.Keys.Contains(us.user) &&
					days.First(d => d.day == day.day.AddDays(3)).absents[us.user] == "nunca sábado-martes")
				{
					days.First(d => d.day == day.day.AddDays(3)).absents.Remove(us.user);
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Sunday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(-2)) &&
					days.First(d => d.day == day.day.AddDays(-2)).absents.Keys.Contains(us.user) &&
					days.First(d => d.day == day.day.AddDays(-2)).absents[us.user] == "nunca viernes-domingo")
				{
					days.First(d => d.day == day.day.AddDays(-2)).absents.Remove(us.user);
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Tuesday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(-3)) &&
					days.First(d => d.day == day.day.AddDays(-3)).absents.Keys.Contains(us.user) &&
					days.First(d => d.day == day.day.AddDays(-3)).absents[us.user] == "nunca sábado-martes")
				{
					days.First(d => d.day == day.day.AddDays(-3)).absents.Remove(us.user);
				}
			}
			else
			{
				//se añade este usuario a la lista de posibles para +2 días y para -2 días
				if (days.Exists(d => d.day == day.day.AddDays(2)) &&
					days.First(d => d.day == day.day.AddDays(2)).possible.ContainsKey(us.user) &&
					days.First(d => d.day == day.day.AddDays(2)).possible[us.user] == "posible doblete")
				{
					days.First(d => d.day == day.day.AddDays(2)).possible.Remove(us.user);
				}
				if (days.Exists(d => d.day == day.day.AddDays(-2)) &&
					days.First(d => d.day == day.day.AddDays(-2)).possible.ContainsKey(us.user) &&
					days.First(d => d.day == day.day.AddDays(-2)).possible[us.user] == "posible doblete")
				{
					days.First(d => d.day == day.day.AddDays(-2)).possible.Remove(us.user);
				}
			}

			//R3 Nunca mas de 2 de la misma unidad de guardia, salvo de esófago gástrica que solo
			//se puede 1, de lunes a jueves. Sábado y domingo no mas de 3 de la misma unidad. 
			if (((day.day.DayOfWeek == DayOfWeek.Monday || day.day.DayOfWeek == DayOfWeek.Tuesday ||
				  day.day.DayOfWeek == DayOfWeek.Wednesday || day.day.DayOfWeek == DayOfWeek.Thursday)
				&& ((unities.Exists(u => u.Name.ToLower()
											   .Replace("á", "a")
											   .Replace("é", "e")
											   .Replace("í", "i")
											   .Replace("ó", "o")
											   .Replace("ú", "u")
											   .Replace(" ", "")
											   .Equals("esofagogastrica") &&
										 u.IdSpecialty == us.user.IdSpecialty) &&
					 us.user.IdUnity == unities.Find(u => u.Name.ToLower()
															   .Replace("á", "a")
															   .Replace("é", "e")
															   .Replace("í", "i")
															   .Replace("ó", "o")
															   .Replace("ú", "u")
															   .Replace(" ", "")
															   .Equals("esofagogastrica") &&
														 u.IdSpecialty == us.user.IdSpecialty)?.Id)
					|| (day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
												a.IdUnity == us.user.IdUnity) == 2)))
				|| ((day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday)
					&& day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
											   a.IdUnity == us.user.IdUnity) == 3))
			{
				//El resto de residentes de la misma unidad quedan inelegibles para este día
				foreach (User user in userStats.Where(u => u.user.IdSpecialty == us.user.IdSpecialty &&
														  u.user.IdUnity == us.user.IdUnity &&
														  !day.assigned.Contains(u.user) &&
														  day.absents.Keys.Contains(u.user))
											  .Select(u => u.user))
				{
					if (day.absents.ContainsKey(user) && day.absents[user] == "no más de dos de la misma unidad")
					{
						day.absents.Remove(user);
					}
				}
			}

			//R4 Máx 6 guardias por cabeza, aunque el máximo legal son 7
			if (days.Count(d => d.assigned.Exists(u => u.Id == us.user.Id)) == totalGuards[(int)us.user.IdSpecialty])
			{
				//queda inelegible para el resto de días
				foreach (Day d in days.Where(d => d.day != day.day && !d.assigned.Contains(us.user) &&
												  d.absents.Keys.Contains(us.user) && d.absents[us.user] == totalGuards+" guardias asignadas"))
				{
					d.absents.Remove(us.user);
				}
			}

			//si ha alcanzado al méximo de fines de semana/festivos queda ilegible para otros fines de semana y festivos
			/*if (us.totalWeekends + us.totaPublicHolidays == maxWeekends)
			{
				foreach (Dictionary<User, string> dd in days.Where(d => d.absents.ContainsKey(us.user) &&
												   d.absents[us.user] == "fines de semana máximos" &&
												  (d.day.DayOfWeek == DayOfWeek.Saturday || d.day.DayOfWeek == DayOfWeek.Sunday ||
												  publicHolidays.Select(ph => ph.Date).Contains(d.day))).Select(d => d.absents))
				{
					dd.Remove(us.user);
				}
			}
			*/
			//R5 Siempre uno de cada año de guardia. Añadimos todos los residentes del mismo año que el elegido
			//a la lista de inelegibles, salvo el que esté rotando.
			/*decimal idRotatorio = unities.First(u => u.Name == "Rotatorio").Id;
			foreach (User user in userStats.Where(u => u.user.IdLevel == us.user.IdLevel &&
													  ((us.user.IdUnity != idRotatorio && u.user.IdUnity != idRotatorio) ||
													   (us.user.IdUnity == idRotatorio && u.user.IdUnity == idRotatorio)) &&
													  day.assigned.Contains(u.user) &&
													  !day.absents.Keys.Contains(u.user))
										  .Select(u => u.user))
			{
				if (day.absents.Contains(user) && day.absents[user] == "nivel ya asignado al día")
				{
					day.absents.Remove(user);
				}
			}*/

			//R6 R5 y R4 de la misma unidad no deberían coincidir a ser posible.
			if (us.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
			{
				//todos los de la misma unidad de R5 deben ir a posibles
				foreach (User u in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R5") &&
														   u.user.IdUnity == us.user.IdUnity &&
														   !day.absents.ContainsKey(u.user) &&
														   day.possible.ContainsKey(u.user) &&
														   day.possible[u.user] == "Hay un R4 de la misma unidad" &&
														   day.assigned.Contains(u.user))
											   .Select(u => u.user))
				{
					day.possible.Remove(u);
				}

			}
			if (us.user.IdLevelNavigation.Name.ToUpper().Equals("R5"))
			{
				//todos los de la misma unidad de R4 deben ir a posibles
				foreach (User u in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
														   u.user.IdUnity == us.user.IdUnity &&
														   !day.absents.ContainsKey(u.user) &&
														   day.possible.ContainsKey(u.user) &&
														   day.possible[u.user] == "Hay un R5 de la misma unidad" &&
														   day.assigned.Contains(u.user))
											   .Select(u => u.user))
				{
					day.possible.Remove(u);
				}

			}

			//R7 R3 y R4 de pared (unidad de endocrino) no deberían coincidir
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Equals("endocrino")))
			{
				if (us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														 u.Name.ToLower().Equals("endocrino"))?.Id &&
					us.user.IdLevelNavigation.Name.ToUpper().Equals("R3"))
				{
					//todos los de la misma unidad de R5 deben ir a posibles
					foreach (User u in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R4") &&
															   u.user.IdUnity == us.user.IdUnity &&
															   !day.absents.ContainsKey(u.user) &&
															   day.possible.ContainsKey(u.user) &&
															   day.possible[u.user] == "Hay un R3 de endocrino" &&
															   day.assigned.Contains(u.user))
												   .Select(u => u.user))
					{
						day.possible.Remove(u);
					}

				}
				if (us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														  u.Name.ToLower().Equals("endocrino"))?.Id &&
					us.user.IdLevelNavigation.Name.ToUpper().Equals("R4"))
				{
						//todos los de la misma unidad de R4 deben ir a posibles
						foreach (User u in userStats.Where(u => u.user.IdLevelNavigation.Name.ToUpper().Equals("R3") &&
																   u.user.IdUnity == us.user.IdUnity &&
																   !day.absents.ContainsKey(u.user) &&
																   day.possible.ContainsKey(u.user) &&
																   day.possible[u.user] == "Hay un R4 de endocrino" &&
																   day.assigned.Contains(u.user))
												   .Select(u => u.user))
						{
							day.possible.Remove(u);
						}
				}
			}

			//R8 Evitar tripletes y cuatrupletes aunque técnicamente se puede
			//tripletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2))?.possible.Remove(us.user);
			}

			//cuatripletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(6))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-6))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(4))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(-2))?.possible.Remove(us.user);
			}
			if (days.Exists(d => d.day == day.day.AddDays(4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(6) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.Find(d => d.day == day.day.AddDays(2))?.possible.Remove(us.user);
			}
			
			//Se comprueba si hay que añadir algún user a posibles
			UpdatePossiblesLists(days, day, us, unities, userStats);
		}

		
		/*bool Backtracking(Dia[] calendario, List<Usuario> usuarios, int dia)
		{
			if (dia == DiasMes)
			{
				return true; // Se ha asignado usuarios para todos los días
			}

			Dia diaActual = calendario[dia];

			if (diaActual.UsuariosAsignados.Count == 6)
			{
				return Backtracking(calendario, usuarios, dia + 1);
			}

			// Lista de usuarios elegibles
			var elegibles = usuarios
				.Where(u => !diaActual.UsuariosProhibidos.Contains(u) && !diaActual.UsuariosAsignados.Contains(u))
				.OrderBy(u => u.GuardiasFinesDeSemana)
				.ThenBy(u => u.GuardiasAsignadas)
				.ToList();

			foreach (var usuario in elegibles)
			{
				if (VerificarRestriccionesUsuario(usuario, dia, calendario))
				{
					diaActual.UsuariosAsignados.Add(usuario);
					usuario.GuardiasAsignadas++;
					if (EsFinDeSemana(dia) || DiasFestivos.Contains(dia))
					{
						usuario.GuardiasFinesDeSemana++;
					}
					ActualizarRestricciones(calendario, dia, usuario);

					if (Backtracking(calendario, usuarios, dia))
					{
						return true;
					}

					diaActual.UsuariosAsignados.Remove(usuario);
					usuario.GuardiasAsignadas--;
					if (EsFinDeSemana(dia) || DiasFestivos.Contains(dia))
					{
						usuario.GuardiasFinesDeSemana--;
					}
					RevertirRestricciones(calendario, dia, usuario);
				}
			}

			// Si no se pudo asignar con los elegibles, probar con los posibles
			var posibles = diaActual.UsuariosPosibles
				.OrderBy(u => u.GuardiasFinesDeSemana)
				.ThenBy(u => u.GuardiasAsignadas)
				.ToList();

			foreach (var usuario in posibles)
			{
				if (VerificarRestriccionesUsuario(usuario, dia, calendario))
				{
					diaActual.UsuariosAsignados.Add(usuario);
					usuario.GuardiasAsignadas++;
					if (EsFinDeSemana(dia) || DiasFestivos.Contains(dia))
					{
						usuario.GuardiasFinesDeSemana++;
					}
					ActualizarRestricciones(calendario, dia, usuario);

					if (Backtracking(calendario, usuarios, dia))
					{
						return true;
					}

					diaActual.UsuariosAsignados.Remove(usuario);
					usuario.GuardiasAsignadas--;
					if (EsFinDeSemana(dia) || DiasFestivos.Contains(dia))
					{
						usuario.GuardiasFinesDeSemana--;
					}
					RevertirRestricciones(calendario, dia, usuario);
				}
			}

			return false;
		}

		static void ActualizarRestricciones(Dia[] calendario, int dia, Usuario usuario)
		{
			// Añadir a la lista de prohibidos los usuarios que incumplen restricciones
			if (dia + 1 < DiasMes)
			{
				calendario[dia + 1].UsuariosProhibidos.Add(usuario);
			}

			if (dia + 2 < DiasMes)
			{
				if ((dia % 7 == 4) || (dia % 7 == 5))
				{
					calendario[dia + 2].UsuariosProhibidos.Add(usuario);
				}
			}

			if (dia + 3 < DiasMes && dia % 7 == 5)
			{
				calendario[dia + 3].UsuariosProhibidos.Add(usuario);
			}

			for (int i = dia + 1; i < DiasMes; i++)
			{
				if (calendario[i].UsuariosAsignados.Count < 6)
				{
					calendario[i].UsuariosProhibidos.Add(usuario);
				}
			}
		}

		static void RevertirRestricciones(Dia[] calendario, int dia, Usuario usuario)
		{
			// Revertir las restricciones añadidas
			if (dia + 1 < DiasMes)
			{
				calendario[dia + 1].UsuariosProhibidos.Remove(usuario);
			}

			if (dia + 2 < DiasMes)
			{
				if ((dia % 7 == 4) || (dia % 7 == 5))
				{
					calendario[dia + 2].UsuariosProhibidos.Remove(usuario);
				}
			}

			if (dia + 3 < DiasMes && dia % 7 == 5)
			{
				calendario[dia + 3].UsuariosProhibidos.Remove(usuario);
			}

			for (int i = dia + 1; i < DiasMes; i++)
			{
				if (calendario[i].UsuariosAsignados.Count < 6)
				{
					calendario[i].UsuariosProhibidos.Remove(usuario);
				}
			}
		}

		static bool VerificarRestriccionesUsuario(Usuario usuario, int dia, Dia[] calendario)
		{
			if (usuario.DiasVacaciones.Contains(dia))
			{
				return false;
			}

			if (usuario.GuardiasAsignadas >= 6)
			{
				return false;
			}

			if (EsFinDeSemana(dia) || DiasFestivos.Contains(dia))
			{
				if (usuario.GuardiasFinesDeSemana >= 2)
				{
					return false;
				}
			}

			if (dia > 0 && calendario[dia - 1].UsuariosAsignados.Contains(usuario))
			{
				return false;
			}

			if (dia > 1)
			{
				if ((dia % 7 == 4 && dia + 2 < DiasMes && calendario[dia + 2].UsuariosAsignados.Contains(usuario)) ||
					(dia % 7 == 5 && dia + 3 < DiasMes && calendario[dia + 3].UsuariosAsignados.Contains(usuario)))
				{
					return false;
				}
			}

			return true;
		}

		static bool EsFinDeSemana(int dia)
		{
			return (dia % 7 == 5 || dia % 7 == 6); // 5: Sábado, 6: Domingo
		}*/

		#endregion
	}
}
