using onGuardManager.Models.Entities;

namespace onGuardManager.Models.DTO.Entities
{
	public class UserStats
	{
		#region properties

		public User user { get; set; }
		public int totalGuard { get; set; }
		public int totalGuardMonth { get; set; }
		public int totalWeekends { get; set; }
		public int totalWeekendsMonth { get; set; }
		public int totaPublicHolidays { get; set; }
		public int totaPublicHolidaysMonth { get; set; }

		#endregion

		#region constructor

		public UserStats(User user, int totalGuard, int totalWeekends, int totaPublicHolidays)
		{
			this.user = user;
			this.totalGuard = totalGuard;
			this.totalWeekends = totalWeekends;
			this.totaPublicHolidays = totaPublicHolidays;
			this.totalGuardMonth = 0;
			this.totalWeekendsMonth = 0;
			this.totaPublicHolidaysMonth = 0;
		}

		#endregion

	}
}
