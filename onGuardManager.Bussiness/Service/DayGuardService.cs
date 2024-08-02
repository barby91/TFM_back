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
		private static List<GuardInterval> guardIntervals = new List<GuardInterval>();
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

			int totalDaysYear = DateTime.IsLeapYear(DateTime.Now.Year) ? 366 : 365;
			DateOnly firstDay = new DateOnly(DateTime.Now.Year, 1, 1);

			if(firstDay.DayOfWeek != DayOfWeek.Monday)
			{
				int daysToAdd = ((int)DayOfWeek.Monday - (int)firstDay.DayOfWeek + 7) % 7;
				firstDay = firstDay.AddDays(daysToAdd);
			}

			while (guardIntervals.Count <= (totalDaysYear/30))
			{
				guardIntervals.Add(new GuardInterval()
				{
					firstDayInterval = firstDay,
					lastDayInterval = firstDay.AddDays(29)
				});

				firstDay = firstDay.AddDays(30);
			}
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

		public async Task<bool> DeletePreviousGuard(GuardInterval guardInterval)
		{
			try
			{
				return await _dayGuardRepository.DeletePreviousGuard(guardInterval.firstDayInterval, guardInterval.lastDayInterval);
			}
			catch (Exception ex)
			{
				StringBuilder sb = new StringBuilder("");
				sb.AppendFormat(" Se ha producido un error al borrar las asignaciones de la guardia del mes {0}." +
								"La traza es: {1}: ", guardInterval.firstDayInterval.ToString() + " - " + guardInterval.lastDayInterval.ToString(), ex.ToString());
				LogClass.WriteLog(ErrorWrite.Error, sb.ToString());
				throw;
			}
		}

		public async Task<string> GetUserStats(GuardRequest guardRequest)
		{
			//obtenemos todos los usuarios de un centro o asociados a una especialidad
			List<User> users;
			List<UserStats> userStats = new List<UserStats>();
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

			unities.AddRange(await _unityRepository.GetAllCommonUnities(guardRequest.idCenter));

			//recorremos cada usuario para inicializar sus estadísticas

			//obtenemos las guardias previas para 
			List<DayGuard> previousGuards;
			int indexFirst = guardRequest.groupOfWeeks - 1;
			if (guardRequest.groupOfWeeks == 1)
			{
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year - 1, 12);
			}
			else
			{
				DateTime now = DateTime.Now;
				indexFirst = guardRequest.groupOfWeeks > 0 ? guardRequest.groupOfWeeks - 1 :
																 guardIntervals.IndexOf(guardIntervals.First(gi => now.CompareTo(gi.firstDayInterval.ToDateTime(new TimeOnly())) >= 0 &&
																												   now.CompareTo(gi.lastDayInterval.ToDateTime(new TimeOnly())) <= 0)) + 1;
				DateOnly startDateInterval = guardIntervals[indexFirst].firstDayInterval;
				DateOnly endDateInterval = guardRequest.groupOfWeeks > 0 ? guardIntervals[guardRequest.groupOfWeeks - 1].firstDayInterval :
												guardIntervals.Last().lastDayInterval;
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year, 0);
				previousGuards = previousGuards.Where(pg => pg.Day.CompareTo(startDateInterval) < 0 ||
															pg.Day.CompareTo(endDateInterval) > 0).ToList();
			}

			foreach (User user in users)
			{
				//contamos los festivos realizados por cada usuario
				List<DayGuard> userGuard = previousGuards.Where(g => g.assignedUsers.Select(a => a.Id).Contains(user.Id)).ToList();
				int totalWeekends = userGuard.Count(ug => ug.Day.DayOfWeek == DayOfWeek.Saturday ||
														  ug.Day.DayOfWeek == DayOfWeek.Sunday ||
														  ug.Day.DayOfWeek == DayOfWeek.Friday);
				int totalPublicHolidays = userGuard.Count(ug => publicHolidays.Select(ph => ph.Date)
												   .Contains(ug.Day));
				int totalUserGuards = userGuard.Count;
				userStats.Add(new UserStats(user, guardIntervals[indexFirst].firstDayInterval.Month == 1 ? 0 : totalUserGuards, 
												  guardIntervals[indexFirst].firstDayInterval.Month == 1 ? 0 : totalWeekends, 
												  guardIntervals[indexFirst].firstDayInterval.Month == 1 ? 0 : totalPublicHolidays));
			}
			
			if (guardRequest.groupOfWeeks > 0)
			{
				return await AsignMonthGuards(guardRequest.groupOfWeeks-1, guardRequest.idCenter, users, previousGuards,
											  unities, userStats, publicHolidays);
			}
			else
			{
				string result = "";
				bool continueLoop = true;
				int i = indexFirst;

				while (i < guardIntervals.Count && continueLoop)
				{
					result = await AsignMonthGuards(i, guardRequest.idCenter, users, previousGuards,
													unities, userStats, publicHolidays);
					if(!result.Contains("OK"))
					{
						continueLoop = false;
					}

					
					previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year, 0);
					previousGuards = previousGuards.Where(pg => pg.Day.CompareTo(guardIntervals[i].firstDayInterval) < 0 ||
																pg.Day.CompareTo(guardIntervals.Last().lastDayInterval) > 0).ToList();

					//reseteamos el total de fines de semana y festivos por mes
					userStats.ForEach(u => u.totalGuardMonth = u.totalWeekendsMonth = u.totaPublicHolidaysMonth = 0);
					i++;
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
		private async Task<string> AsignMonthGuards(int groupOfWeeks, int idCenter,
													List<User> users, 
													List<DayGuard> previousGuards,
													List<Unity> unities,
													List<UserStats> userstats,
													List<PublicHoliday> publicHolidays)
		{
			List<Day> days = new List<Day>();
			//recorremos los días del mes
			DateOnly date = guardIntervals[groupOfWeeks].firstDayInterval;
			int i = 0;
			while (date.CompareTo(guardIntervals[groupOfWeeks].lastDayInterval) <= 0)
			{
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
				i++;
				date = date.AddDays(1);
			}

			if (!BacktrackingGuard(days, userstats, OrderDays(days), publicHolidays, unities))
			{
				return await Task.FromResult("No se pueden asignar las guardias del grupo de semanas " + guardIntervals[groupOfWeeks].firstDayInterval.ToString() + " - " + guardIntervals[groupOfWeeks].lastDayInterval.ToString());
			}
			else
			{
				//si se ha asignado, se reorganizan aquellos usuarios que tengnan menos guardias del mínimo
				if (!ReorderUsers(days, userstats, publicHolidays, unities))
				{
					return await Task.FromResult("No se pueden asignar las guardias mínimas por usuario del grupo de semanas " + guardIntervals[groupOfWeeks].firstDayInterval.ToString() + " - " + guardIntervals[groupOfWeeks].lastDayInterval.ToString());
				}
				else
				{
					if (await DeletePreviousGuard(guardIntervals[groupOfWeeks]))
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
						GuardStats stat = new GuardStats()
						{
							TotalDobletes = 0,
							TotalTripletes = 0,
							TotalCuatripletes = 0,
							users = new List<GuardUserStats>()
						};

						foreach (DayGuardModel guard in guards)
						{

							foreach (string name in guard.assignedUsers.Select(au => au.NameSurname))
							{
								if (stat.users.Exists(s => s.UserName == name))
								{
									stat.users.First(s => s.UserName == name).GuardByUser++;
									if (guard.Day.DayOfWeek == DayOfWeek.Sunday || guard.Day.DayOfWeek == DayOfWeek.Saturday || guard.Day.DayOfWeek == DayOfWeek.Friday)
									{
										stat.users.First(s => s.UserName == name).WeekendsbyUser++;
									}
									else if (publicHolidays.Select(ph => ph.Date).Contains(guard.Day))
									{
										stat.users.First(s => s.UserName == name).HolidaysByUser++;
									}
								}
								else
								{
									stat.users.Add(new GuardUserStats()
									{
										UserName = name,
										GuardByUser = 1,
										HolidaysByUser = (guard.Day.DayOfWeek != DayOfWeek.Sunday &&
														  guard.Day.DayOfWeek != DayOfWeek.Saturday &&
														  guard.Day.DayOfWeek != DayOfWeek.Friday &&
														  publicHolidays.Select(ph => ph.Date).Contains(guard.Day)) ? 1 : 0,
										WeekendsbyUser = (guard.Day.DayOfWeek == DayOfWeek.Sunday ||
														  guard.Day.DayOfWeek == DayOfWeek.Saturday ||
														  guard.Day.DayOfWeek == DayOfWeek.Friday) ? 1 : 0
									});
								}
							}

							foreach (UserModel us in guard.assignedUsers)
							{
								stat.TotalDobletes += guards.Count(g => g.Day == guard.Day.AddDays(2) && g.assignedUsers.Any(au => au.NameSurname.Equals(us.NameSurname)));

								//tripletes
								if ((guards.Exists(d => d.Day == guard.Day.AddDays(2) &&
													d.assignedUsers.Any(a => a.NameSurname.Equals(us.NameSurname))) &&
									guards.Exists(d => d.Day == guard.Day.AddDays(4) &&
													d.assignedUsers.Any(a => a.NameSurname.Equals(us.NameSurname)))
									))
								{
									stat.TotalTripletes++;
								}

								//cuadrupletes
								if ((guards.Exists(d => d.Day == guard.Day.AddDays(2) &&
														d.assignedUsers.Any(a => a.NameSurname.Equals(us.NameSurname))) &&
									 guards.Exists(d => d.Day == guard.Day.AddDays(4) &&
														d.assignedUsers.Any(a => a.NameSurname.Equals(us.NameSurname))) &&
									 guards.Exists(d => d.Day == guard.Day.AddDays(6) &&
														d.assignedUsers.Any(a => a.NameSurname.Equals(us.NameSurname)))
									))
								{
									stat.TotalCuatripletes++;
								}
							}

						}
						if (result)
						{

							dynamic obj = new
							{
								result = "OK",
								stat = stat
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

		#endregion
		
		#region backtracking

		private bool BacktrackingGuard(List<Day> days, List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays, List<Unity> unities)
		{
			if(!days.Exists(d => d.assigned.Count < 6))
			{
				return true;
			}
			else
			{
				if(day.assigned.Count == 6)
				{
					return BacktrackingGuard(days, userStats, OrderDays(days), publicHolidays, unities);
				}
				else
				{
					//cogemos los usuarios posibles, mientras no haya 5 asignados se coge el más optimo
					//cuyo nivel no se haya insertado ya
					List<UserStats> users;

					//si el día es fin de semana o festivo se ordena por el que menos número de fines de semana y festivos tenga
					users = GetUsersOrdered(userStats, day, publicHolidays);

					foreach(UserStats us in users)
					{
						day.assigned.Add(us.user);
						UpdateStatics(us, day, publicHolidays);
						UpdateAbsentsLists(days, day, us, unities, userStats);
						if (BacktrackingGuard(days, userStats, day, publicHolidays, unities))
						{
							return true;
						}
						RevertStatics(us, day, publicHolidays);
						RevertLists(days, day, us, unities, userStats);
						day.assigned.Remove(us.user);
						if (!day.absents.ContainsKey(us.user))
						{
							day.absents.Add(us.user, "No llega a una solución final");
						}
					}

					return false;
				}
			}
		}

		private bool ReorderUsers(List<Day> days, List<UserStats> userStats, List<PublicHoliday> publicHolidays, List<Unity> unities)
		{
			List<UserStats> userLessMin = userStats.Where(u => u.totalGuardMonth < 5).OrderBy(u => u.totalGuard).ToList();
			List<UserStats> userMax = userStats.Where(u => u.totalGuardMonth == 6).ToList();

			if (userMax.Any() && userLessMin.Any())
			{
				Dictionary<UserStats, List<Day>> dayUsers = new Dictionary<UserStats, List<Day>>();
				foreach (UserStats us in userLessMin)
				{
					//cogemos los días que no contengan entre ausentes y asignados al usuario que hay que asignarle más días y que entre los asignados
					//tenga alguno de los usuarios que tienen el máximo de guardias asignados
					dayUsers.Add(us, days.Where(d => !d.absents.ContainsKey(us.user) &&
													 !d.assigned.Contains(us.user) &&
													 d.assigned.Intersect(userMax.Select(u => u.user)).Any()).ToList());

				}

				List<KeyValuePair<UserStats, List<Day>>> dayUsersOrder = dayUsers.OrderBy(du => du.Value.Count).ToList();

				List<Day> possibleDaysToChange = dayUsersOrder[0].Value;
				UserStats selectedUser = dayUsersOrder[0].Key;
				foreach (Day pdc in possibleDaysToChange)
				{
					List<User> possibleUserToChange = pdc.assigned.Intersect(userMax.Select(um => um.user)).ToList();
					foreach(User puc in possibleUserToChange)
					{
						pdc.assigned.Remove(puc);
						RevertStatics(userStats.First(us => us.user == puc), pdc, publicHolidays);
						RevertLists(days, pdc, userStats.First(us => us.user == puc), unities, userStats);
						pdc.assigned.Add(selectedUser.user);
						UpdateStatics(selectedUser, pdc, publicHolidays);
						UpdateAbsentsLists(days, pdc, selectedUser, unities, userStats);
						if (ReorderUsers(days, userStats, publicHolidays, unities))
						{
							return true;
						}
						pdc.assigned.Remove(selectedUser.user);
						RevertStatics(selectedUser, pdc, publicHolidays);
						RevertLists(days, pdc, selectedUser, unities, userStats);
						pdc.assigned.Add(puc);
						UpdateStatics(userStats.First(us => us.user == puc), pdc, publicHolidays);
						UpdateAbsentsLists(days, pdc, userStats.First(us => us.user == puc), unities, userStats);
					}
				}
				
				return false;
			}
			else
			{
				return true;
			}
		}

		private Day OrderDays(List<Day> days)
		{
			return days.Where(d => d.assigned.Count < 6).OrderBy(d => d.day).First();
		}

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
					//cogemos el usuario más óptimo de los restantes
					users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id)).ToList();
				}
			}

			users = (day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Friday
					|| publicHolidays.Select(ph => ph.Date).Contains(day.day)) ?
					users.OrderBy(us => us.totalWeekends + us.totaPublicHolidays).ThenBy(us => us.totalWeekendsMonth + us.totaPublicHolidaysMonth).ToList() :
					users.OrderBy(us => us.totalGuardMonth).ToList();

			return users;
		}

		private void UpdateStatics(UserStats us, Day day, List<PublicHoliday> publicHolidays)
		{
			//Se actualizan las estadísitcas del usuario
			us.totalGuard++;
			us.totalGuardMonth++;
			if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Friday)
			{
				us.totalWeekends++;
				us.totalWeekendsMonth++;
			}
			//Si el día esta entre la lista de festivos se suma uno a los festivos
			if (publicHolidays.Exists(ph => ph.Date == day.day) && day.day.DayOfWeek != DayOfWeek.Sunday && 
				day.day.DayOfWeek != DayOfWeek.Saturday && day.day.DayOfWeek != DayOfWeek.Friday)
			{
				us.totaPublicHolidays++;
				us.totaPublicHolidaysMonth++;
			}
		}

		private void RevertStatics(UserStats us, Day day, List<PublicHoliday> publicHolidays)
		{
			//Se actualizan las estadísitcas del usuario
			us.totalGuard--;
			us.totalGuardMonth--;
			if (day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Friday)
			{
				us.totalWeekends--;
				us.totalWeekendsMonth--;
			}
			//Si el día esta entre la lista de festivos se suma uno a los festivos
			if (publicHolidays.Exists(ph => ph.Date == day.day) && day.day.DayOfWeek != DayOfWeek.Sunday &&
				day.day.DayOfWeek != DayOfWeek.Saturday && day.day.DayOfWeek != DayOfWeek.Friday)
			{
				us.totaPublicHolidays--;
				us.totaPublicHolidaysMonth--;
			}
		}

		private void UpdateAbsentsLists(List<Day> days, Day day, UserStats us, List<Unity> unities, List<UserStats> userStats)
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

			//R2 nunca viernes-domingo ni sábado-martes
			if (day.day.DayOfWeek == DayOfWeek.Friday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(2) && !d.absents.Keys.Contains(us.user)))
				{
					days.First(d => d.day == day.day.AddDays(2)).absents.Add(us.user, "nunca viernes-domingo");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Saturday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(3) && !d.absents.Keys.Contains(us.user)))
				{
					days.First(d => d.day == day.day.AddDays(3)).absents.Add(us.user, "nunca sábado-martes");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Sunday)
			{
				if (days.Exists(d => d.day == day.day.AddDays(-2) && !d.absents.Keys.Contains(us.user)))
				{
					days.First(d => d.day == day.day.AddDays(-2)).absents.Add(us.user, "nunca viernes-domingo");
				}
			}
			else if (day.day.DayOfWeek == DayOfWeek.Tuesday && days.Exists(d => d.day == day.day.AddDays(-3) && !d.absents.Keys.Contains(us.user)))
			{
				days.First(d => d.day == day.day.AddDays(-3)).absents.Add(us.user, "nunca sábado-martes");
			}

			//R3 Nunca mas de 2 de la misma unidad de guardia, salvo de esófago gástrica que solo
			//se puede 1, de lunes a jueves. Sábado y domingo no mas de 3 de la misma unidad. 
			decimal idUnityEsofago = unities.First(u => u.Name.Equals("Esófago gástrica") &&
												   u.IdSpecialty == us.user.IdSpecialty).Id;

			if (((day.day.DayOfWeek == DayOfWeek.Monday || day.day.DayOfWeek == DayOfWeek.Tuesday ||
				 day.day.DayOfWeek == DayOfWeek.Wednesday || day.day.DayOfWeek == DayOfWeek.Thursday) &&
				day.assigned.Count < 6 &&
				day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
										a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDay
										/*(us.user.IdUnity == idUnityEsofago && 
										day.assigned.Exists(a => a.IdSpecialty == us.user.IdSpecialty &&
																a.IdUnity == idUnityEsofago)) ||
										(us.user.IdUnity != idUnityEsofago &&
										day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
																a.IdUnity == us.user.IdUnity) == 2)*/) ||
				((day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday) &&
				  day.assigned.Count < 6 &&
				  day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
										a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDayWeekend
										/*day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
															  a.IdUnity == us.user.IdUnity) == 3)*/))
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
				foreach (var d in				
						 from Day d in days.Where(d => d.day != day.day && !d.assigned.Contains(us.user))
						 where !d.absents.Keys.Contains(us.user)
						 select d)
				{
					//queda inelegible para el resto de días pero no para el propio
					d.absents.Add(us.user, totalGuards + " guardias asignadas");
				}
			}

			//si hubiera alguno de los inelegibles en la lista de posibles lo quitamos
			foreach (User key in day.absents.Keys.Where(k => day.possible.ContainsKey(k)))
			{
				day.possible.Remove(key);
			}

			UpdatePossiblesLists(days, day, us, unities, userStats);
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

		private void RevertLists(List<Day> days, Day day, UserStats us, List<Unity> unities, List<UserStats> userStats)
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
				&& day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
										a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDay
					/*((unities.Exists(u => u.Name.Equals("Esófago gástrica\r\n") &&
										 u.IdSpecialty == us.user.IdSpecialty) &&
					 us.user.IdUnity == unities.Find(u => u.Name.Equals("Esófago gástrica\r\n") &&
														 u.IdSpecialty == us.user.IdSpecialty)?.Id)
					|| (day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
												a.IdUnity == us.user.IdUnity) == 2))*/)
				|| ((day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday)
					&& day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
										a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDayWeekend
					/*day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
											   a.IdUnity == us.user.IdUnity) == 3*/))
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

		#endregion
	}
}
