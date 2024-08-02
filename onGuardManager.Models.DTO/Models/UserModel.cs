using onGuardManager.Models.DTO.Entities;
using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models
{
	public class UserModel
	{
		#region properties
		public decimal Id { get; set; }

		public string NameSurname { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public int HolidayCurrentPeriod { get; set;  } = 0;

		public int HolidayPreviousPeriod { get; set;  } = 0;

		public int CurrentPeriodLeftDay { get; set; } = 0;

		public int PreviousPeriodLeftDay { get; set; } = 0;

		public string centerName { get; set; } = string.Empty;

		public int centerId { get; set; } = 0;

		public string levelName { get; set; } = string.Empty;

		public string rolName { get; set; } = string.Empty;

		public string specialtyName { get; set; } = string.Empty;

		public string unityName { get; set; } = string.Empty;

		public List<AskedHolidayModel> AskedHolidays { get; set; } = new List<AskedHolidayModel>();

		public string Color { get; set; } = string.Empty;
		#endregion

		#region constructor
		[JsonConstructor]
		public UserModel() { }
		public UserModel(User user, List<PublicHolidayModel> publicHolidays)
		{
			this.NameSurname = user.Name + " " + user.Surname;
			this.Email = user.Email;
			this.Id = user.Id;
			this.HolidayCurrentPeriod = (int)user.HolidayCurrentPeridod;
			this.HolidayPreviousPeriod = (int)user.HolidayPreviousPeriod;
			this.AskedHolidays = new List<AskedHolidayModel>();
			foreach(AskedHoliday holiday in user.AskedHolidays)
			{
				this.AskedHolidays.Add(new AskedHolidayModel(holiday));
			}
			this.AskedHolidays = this.AskedHolidays.OrderByDescending(ah => ah.Id).ToList();
			CurrentPeriodLeftDay = this.HolidayCurrentPeriod - CalculatePeriodLeft(publicHolidays, DateTime.Now.Year);
			PreviousPeriodLeftDay = this.HolidayPreviousPeriod == 0 ? 0 : this.HolidayPreviousPeriod - CalculatePeriodLeft(publicHolidays, DateTime.Now.Year-1);
			if (user.IdCenterNavigation != null)
			{
				this.centerName =user.IdCenterNavigation.Name;
				this.centerId = (int)user.IdCenter;
			}
			if (user.IdLevelNavigation != null)
			{
				this.levelName = user.IdLevelNavigation.Name;
				//TODO solo para pruebas de asignación de guardias
				//this.NameSurname = user.Name + "_" + user.IdLevelNavigation.Name;
			}
			if (user.IdRolNavigation != null)
			{
				this.rolName = user.IdRolNavigation.Name;
			}
			if (user.IdSpecialtyNavigation != null)
			{
				this.specialtyName = user.IdSpecialtyNavigation.Name;
			}
			if(user.IdUnityNavigation != null)
			{
				this.unityName = user.IdUnityNavigation.Name;
				//TODO solo para pruebas de asignación de guardias
				//this.NameSurname += "_" + user.IdUnityNavigation.Name;
				//SetColor(user.IdUnityNavigation.Name);
			}
		}

		#endregion

		#region private methods
		private void SetColor(string unityName)
		{
			List<ColorUser> colors = new List<ColorUser>();
			colors.Add(new ColorUser()
			{
				name = "Colon",
				color = "#F30B0B"
			});
			
			colors.Add(new ColorUser()
			{
				name = "Hepáticobilio",
				color = "#12F30B"
			});
			
			colors.Add(new ColorUser()
			{
				name = "Endocrino",
				color = "#F30BDE"
			});
			
			colors.Add(new ColorUser()
			{
				name = "Esófago gástrica",
				color = "#0B7FF3"
			});
			
			colors.Add(new ColorUser()
			{
				name = "Rotatorio",
				color = "#ECF30B"
			});

			ColorUser? color = colors.Find(c => c.name == unityName);
			this.Color = color != null ? color.color : "";
		}

		/// <summary>
		/// Este método calcula los días restante del periodo correspondiente al año pasado por parámetro
		/// </summary>
		/// <param name="publicHolidays">festivos</param>
		/// <param name="year">año del periodo</param>
		/// <returns></returns>
		int CalculatePeriodLeft(List<PublicHolidayModel> publicHolidays, int year)
		{
			List<AskedHolidayModel> askedHolidays = this.AskedHolidays.Where(ah => ah.Period == year.ToString() && ah.StatusDes != "Cancelado").ToList();
			int daysDifference = 0;

			foreach(AskedHolidayModel askedHoliday in  askedHolidays)
			{
				int numDaysOfPeriod = askedHoliday.DateTo.DayNumber - askedHoliday.DateFrom.DayNumber+1;
				DateOnly currentDate = askedHoliday.DateFrom;
				for (int i = 0; i < numDaysOfPeriod; i++)
				{
					if(currentDate.DayOfWeek!= DayOfWeek.Saturday && currentDate.DayOfWeek!= DayOfWeek.Sunday &&
						!publicHolidays.Select(ph => ph.Date).Contains(currentDate))
					{
						daysDifference++;
					}

					currentDate = currentDate.AddDays(1);
				}
			}

			return daysDifference;
		}
		#endregion

	}
}
