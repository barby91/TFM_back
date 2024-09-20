using onGuardManager.Models.Entities;

namespace onGuardManager.Models.DTO.Entities
{
	public class Day
	{
		#region properties

		public DateOnly day { get; set; }
		public Dictionary<User, string> absents { get; set; }
		public Dictionary<User, string> possible { get; set; }
		public List<User> assigned { get; set; }

		#endregion

		#region constructor

		public Day(DateOnly day, Dictionary<User, string> absents)
		{
			this.day = day;
			this.absents = absents;
			assigned = new List<User>();
			possible = new Dictionary<User, string>();
		}

		#endregion

	}
}
