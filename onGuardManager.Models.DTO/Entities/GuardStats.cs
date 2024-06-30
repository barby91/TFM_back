
namespace onGuardManager.Models.DTO.Entities
{
	public class GuardStats
	{
		public required string UserName { get; set; }
		public int GuardByUser { get; set; }
		public int HolidaysByUser { get; set; }
		public int WeekendsbyUser { get; set; }
	}
}
