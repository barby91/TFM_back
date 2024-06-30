using onGuardManager.Models.Entities;

namespace onGuardManager.Models.DTO.Entities
{
	public class UserStats //: ICloneable
	{
		#region properties

		public User user { get; set; }
		public int totalGuardMonth { get; set; }
		public int totalWeekends { get; set; }
		public int totaPublicHolidays { get; set; }

		#endregion

		#region constructor

		public UserStats(User user, int totalGuardMonth, int totalWeekends, int totaPublicHolidays)
		{
			this.user = user;
			this.totalGuardMonth = totalGuardMonth;
			this.totalWeekends = totalWeekends;
			this.totaPublicHolidays = totaPublicHolidays;
		}

		#endregion

		#region interface ICloneable
		/*public object Clone()
		{
			return new UserStats(user, totalGuardMonth, totalWeekends, totaPublicHolidays);
		}*/

		#endregion


	}
}
