using onGuardManager.Models.Entities;

namespace onGuardManager.Models.DTO.Entities
{
	public class Day //: ICloneable
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

		/*public Day(DateOnly day, Dictionary<User, string> absents, List<User> assigned, Dictionary<User, string> possible)
		{
			this.day = day;
			this.absents = absents;
			this.assigned = assigned;
			this.possible = possible;
		}*/

		#endregion

		#region interface ICloneable
		/*public object Clone()
		{
			Dictionary<User, string> absentsClone = new Dictionary<User, string>();
			Dictionary<User, string> possibleClone = new Dictionary<User, string>();
			List<User> assignedClone = new List<User>();

			foreach(User item in absents.Keys)
			{
				absentsClone.Add((User)item.Clone(), absents[item]);
			}

			foreach (User item in assigned)
			{
				assignedClone.Add((User)item.Clone());
			}

			foreach (User item in possible.Keys)
			{
				possibleClone.Add((User)item.Clone(), possible[item]);
			}

			return new Day(day, absentsClone, assignedClone, possibleClone);
		}*/

		#endregion
	}
}
