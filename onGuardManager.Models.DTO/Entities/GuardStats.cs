
using onGuardManager.Models.Entities;

namespace onGuardManager.Models.DTO.Entities
{
	public class GuardUserStats
	{
		public required string UserName { get; set; }
		public int GuardByUser { get; set; }
		public int HolidaysByUser { get; set; }
		public int WeekendsbyUser { get; set; }
	}

	public class GuardStats
	{
		public List<GuardUserStats> users { get; set; }
		public int TotalDobletes { get; set; }
		public int TotalTripletes { get; set; }
		public int TotalCuatripletes { get; set; }

	}
}
