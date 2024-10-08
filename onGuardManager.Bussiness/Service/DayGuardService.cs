﻿using onGuardManager.Bussiness.IService;
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
		private static int maxUsersAssignedByDay = 5;
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
			totalGuards = new Dictionary<int, int>();
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
			if (guardRequest.month == 1)
			{
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year - 1, 12);
			}
			else
			{
				previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year, 0);
				previousGuards = previousGuards.Where(pg => pg.Day.Month != (guardRequest.month > 0 ? guardRequest.month : (DateTime.Now.Month + 1))).ToList();
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
				userStats.Add(new UserStats(user, guardRequest.month == 1 ? 0 : totalUserGuards, guardRequest.month == 1 ? 0 : totalWeekends, guardRequest.month == 1 ? 0 : totalPublicHolidays));
			}

			if (guardRequest.month > 0)
			{
				return await AsignMonthGuards(guardRequest.month, guardRequest.idCenter, users, previousGuards,
											  unities, userStats, publicHolidays);
			}
			else
			{
				string result = "";
				bool continueLoop = true;
				int month = DateTime.Now.Month + 1;

				while (month <= 12 && continueLoop)
				{
					result = await AsignMonthGuards(month, guardRequest.idCenter, users, previousGuards,
													unities, userStats, publicHolidays);
					if (!result.Contains("OK"))
					{
						continueLoop = false;
					}

					if (guardRequest.month == 1)

					{
						previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year - 1, 12);
					}
					else
					{
						previousGuards = await _dayGuardRepository.GetGuards(guardRequest.idCenter, DateTime.Now.Year, 0);
						previousGuards = previousGuards.Where(pg => pg.Day.Month != guardRequest.month).ToList();
					}

					//reseteamos el total de fines de semana y festivos por mes
					userStats.ForEach(u => u.totalGuardMonth = u.totalWeekendsMonth = u.totaPublicHolidaysMonth = 0);
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

			if (!BacktrackingGuard(days, userstats, OrderDays(days), publicHolidays, unities))
			{
				return await Task.FromResult("No se pueden asignar las guardias del mes " + month);
			}
			else
			{
				//si se ha asignado, se reorganizan aquellos usuarios que tengnan menos guardias del mínimo
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

						foreach (string us in guard.assignedUsers.Select(us => us.NameSurname))
						{
							stat.TotalDobletes += guards.Count(g => g.Day == guard.Day.AddDays(2) && g.assignedUsers.Any(au => au.NameSurname.Equals(us)));

							//tripletes
							if ((guards.Exists(d => d.Day == guard.Day.AddDays(2) &&
												d.assignedUsers.Any(a => a.NameSurname.Equals(us))) &&
								guards.Exists(d => d.Day == guard.Day.AddDays(4) &&
												d.assignedUsers.Any(a => a.NameSurname.Equals(us)))
								))
							{
								stat.TotalTripletes++;
							}

							//cuadrupletes
							if ((guards.Exists(d => d.Day == guard.Day.AddDays(2) &&
													d.assignedUsers.Any(a => a.NameSurname.Equals(us))) &&
									guards.Exists(d => d.Day == guard.Day.AddDays(4) &&
													d.assignedUsers.Any(a => a.NameSurname.Equals(us))) &&
									guards.Exists(d => d.Day == guard.Day.AddDays(6) &&
													d.assignedUsers.Any(a => a.NameSurname.Equals(us)))
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

		private void CalculateAbsentsUsersByPrevMonth(List<DayGuard> previousGuards,
											 List<User> users, Day newDay)
		{
			//R2 nunca viernes-domingo ni sábado-lunes
			if (newDay.day.DayOfWeek == DayOfWeek.Sunday || newDay.day.DayOfWeek == DayOfWeek.Monday)
			{
				//si el día actual es domingo o lunes, los usuarios que han tenido guardia dos días antes (viernes o sábado) son ilegibles
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
					newDay.absents.Add(user, "nunca viernes-domingo o sábado-lunes");
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
					newDay.possible.Add(user, "Ha tenido guardia dos días antes");
				}
			}
		}

		#endregion
		
		#region backtracking

		private bool BacktrackingGuard(List<Day> days, List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays, List<Unity> unities)
		{
			if(!days.Exists(d => d.assigned.Count < maxUsersAssignedByDay))
			{
				return true;
			}
			else
			{
				if(day.assigned.Count == maxUsersAssignedByDay)
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

		private Day OrderDays(List<Day> days)
		{
			return days.Where(d => d.assigned.Count < maxUsersAssignedByDay).OrderBy(d => d.day).First();
		}

		private List<UserStats> GetUsersOrdered(List<UserStats> userStats, Day day, List<PublicHoliday> publicHolidays)
		{
			List<UserStats> users;
			//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
			users = userStats.Where(u => !day.assigned.Select(a => a.Id).Contains(u.user.Id) &&
											!day.absents.Keys.Select(k => k.Id).Contains(u.user.Id) &&
											!day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
											!day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();

			//si no queda ninguno de los usuarios que cumplen todas las reglas, cogemos los posibles
			if (users.Count == 0)
			{
				//debemos escoger el usuario más óptimo cuyo nivel no esté ya en asignados
				users = userStats.Where(u => day.possible.Keys.Select(a => a.Id).Contains(u.user.Id) &&
												!day.assigned.Select(a => a.IdLevel).Contains(u.user.IdLevel)).ToList();
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

			//R2 nunca viernes-domingo ni sábado-lunes
			if ((day.day.DayOfWeek == DayOfWeek.Friday || day.day.DayOfWeek == DayOfWeek.Saturday) && 
				days.Exists(d => d.day == day.day.AddDays(2) && !d.possible.Keys.Contains(us.user)))
			{
				days.First(d => d.day == day.day.AddDays(2)).possible.Add(us.user, "nunca viernes-domingo o sábado-lunes");
			}
			else if ((day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Monday) && 
					days.Exists(d => d.day == day.day.AddDays(-2) && !d.possible.Keys.Contains(us.user)))
			{
				days.First(d => d.day == day.day.AddDays(-2)).possible.Add(us.user, "nunca viernes-domingo o o sábado-lunes");
			}

			//R3 Nunca mas de 2 de la misma unidad de guardia, salvo de esófago gástrica que solo
			//se puede 1, de lunes a jueves. Sábado y domingo no mas de 3 de la misma unidad. 
			if (((day.day.DayOfWeek == DayOfWeek.Monday || day.day.DayOfWeek == DayOfWeek.Tuesday ||
				 day.day.DayOfWeek == DayOfWeek.Wednesday || day.day.DayOfWeek == DayOfWeek.Thursday) &&
				day.assigned.Count < maxUsersAssignedByDay &&
				day.assigned.Count(a => a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDay) ||
				((day.day.DayOfWeek == DayOfWeek.Sunday || day.day.DayOfWeek == DayOfWeek.Saturday) &&
				  day.assigned.Count < maxUsersAssignedByDay &&
				  day.assigned.Count(a => a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDayWeekend))
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
				foreach (Day d in days.Where(d => d.day != day.day && !d.assigned.Contains(us.user) && !d.absents.Keys.Contains(us.user)))
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
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Contains("endocrino")) && us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														u.Name.ToLower().Contains("endocrino")).Id &&
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
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Contains("endocrino")) && 
				us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														u.Name.ToLower().Contains("endocrino")).Id &&
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

			//R8 Evitar tripletes y cuatrupletes aunque técnicamente se puede
			//cuatripletes			
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.First(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)).possible.Add(us.user, "Posible cuadriplete");
			}

			//tripletes, como los días van siempre en orden del 1 al 30, solo hay que comprobar si dos días antes hay
			//un usuario asignado y marcar como posible dos días después
			
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user))
			   )
			{
				days.First(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)).possible.Add(us.user, "Posible triplete");
			}
			
			//R2 evitar dobletes
			if (days.Exists(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)))
			{
				days.First(d => d.day == day.day.AddDays(2) &&
								 !d.assigned.Contains(us.user) &&
								 !d.possible.ContainsKey(us.user) &&
								 !d.absents.ContainsKey(us.user)).possible.Add(us.user, "posible doblete");
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
				days.First(d => d.day == day.day.AddDays(-1)).absents[us.user] == "nunca 2 días seguidos (día anterior)")
			{
				days.First(d => d.day == day.day.AddDays(-1)).absents.Remove(us.user);
			}

			//R2 Se puede dobletes (guardia-libre-guardia) pero evitarlos en la medida de lo posible y
			//nunca viernes-domingo ni sábado-lunes
			if ((day.day.DayOfWeek == DayOfWeek.Friday || day.day.DayOfWeek == DayOfWeek.Saturday) &&
				(days.Exists(d => d.day == day.day.AddDays(2)) &&
				 days.First(d => d.day == day.day.AddDays(2)).possible.Keys.Contains(us.user) &&
				 days.First(d => d.day == day.day.AddDays(2)).possible[us.user] == "nunca viernes-domingo o sábado-lunes"))
			{
				days.First(d => d.day == day.day.AddDays(2)).possible.Remove(us.user);
			}
			else if (day.day.DayOfWeek == DayOfWeek.Sunday || (day.day.DayOfWeek == DayOfWeek.Monday))
			{
				if (days.Exists(d => d.day == day.day.AddDays(-2)) &&
					days.First(d => d.day == day.day.AddDays(-2)).possible.Keys.Contains(us.user) &&
					days.First(d => d.day == day.day.AddDays(-2)).possible[us.user] == "nunca viernes-domingo o sábado-lunes")
				{
					days.First(d => d.day == day.day.AddDays(-2)).possible.Remove(us.user);
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
				&& day.assigned.Count(a => a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDay
					/*((unities.Exists(u => u.Name.Equals("Esófago gástrica\r\n") &&
										 u.IdSpecialty == us.user.IdSpecialty) &&
					 us.user.IdUnity == unities.Find(u => u.Name.Equals("Esófago gástrica\r\n") &&
														 u.IdSpecialty == us.user.IdSpecialty)?.Id)
					|| (day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
												a.IdUnity == us.user.IdUnity) == 2))*/)
				|| ((day.day.DayOfWeek == DayOfWeek.Saturday || day.day.DayOfWeek == DayOfWeek.Sunday)
					&& day.assigned.Count(a => a.IdUnity == us.user.IdUnity) == unities.First(u => u.Id == us.user.IdUnity).MaxByDayWeekend
					/*day.assigned.Count(a => a.IdSpecialty == us.user.IdSpecialty &&
											   a.IdUnity == us.user.IdUnity) == 3*/))
			{
				//El resto de residentes de la misma unidad quedan inelegibles para este día
				foreach (User user in userStats.Where(u => u.user.IdSpecialty == us.user.IdSpecialty &&
														  u.user.IdUnity == us.user.IdUnity &&
														  !day.assigned.Contains(u.user) &&
														  day.absents.Keys.Contains(u.user) &&
														  day.absents[u.user] == "no más de dos de la misma unidad")
											  .Select(u => u.user))
				{
					day.absents.Remove(user);
				}
			}

			//R4 Máx 6 guardias por cabeza, aunque el máximo legal son 7
			if (days.Count(d => d.assigned.Exists(u => u.Id == us.user.Id)) == totalGuards[(int)us.user.IdSpecialty] &&
				days.Exists(d => d.absents.Keys.Contains(us.user) && d.absents[us.user] == totalGuards + " guardias asignadas"))
			{
				//queda inelegible para el resto de días
				foreach (Day d in days.Where(d => d.absents.Keys.Contains(us.user) && d.absents[us.user] == totalGuards+" guardias asignadas"))
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
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Contains("endocrino")) &&
				us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
													  u.Name.ToLower().Contains("endocrino")).Id &&
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
			if (unities.Exists(u => u.IdSpecialty == us.user.IdSpecialty && u.Name.ToLower().Contains("endocrino")) &&
				us.user.IdUnity == unities.First(u => u.IdSpecialty == us.user.IdSpecialty &&
														u.Name.ToLower().Contains("endocrino"))?.Id &&
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

			//R8 Evitar tripletes y cuatrupletes aunque técnicamente se puede
			//tripletes, como solo se va hacia adelante, solo hay que mirar si existe uno dos días antes
			//y desmarcarlo dos días después
			
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible triplete")
			   )
			{
				days.First(d => d.day == day.day.AddDays(2)).possible.Remove(us.user);
			}

			//cuatripletes. Como siempre se va hacia adelante, solo hay que mirar si existe dos días antes y
			//4 días antes y desmarcarlo dos días después
			if (days.Exists(d => d.day == day.day.AddDays(-2) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(-4) &&
								 d.assigned.Contains(us.user)) &&
				days.Exists(d => d.day == day.day.AddDays(2) &&
								 d.possible.ContainsKey(us.user) && d.possible[us.user] == "Posible cuadriplete")
			   )
			{
				days.First(d => d.day == day.day.AddDays(2)).possible.Remove(us.user);
			}

			//Se comprueba si hay que añadir algún user a posibles
			UpdatePossiblesLists(days, day, us, unities, userStats);
		}

		#endregion
	}
}
