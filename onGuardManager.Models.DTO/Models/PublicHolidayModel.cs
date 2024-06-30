using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models
{
	public class PublicHolidayModel
	{
		#region properties
		public decimal Id { get; set; }

		public DateOnly Date { get; set; }

		public string TypeLabel { get; set; }

		public string Colour { get; set; }
		#endregion

		#region constructor
		[JsonConstructor]
		public PublicHolidayModel() 
		{
			Colour = "#7FD028";
		}
		public PublicHolidayModel(PublicHoliday publicHoliday)
		{
			Id = publicHoliday.Id;
			Date = publicHoliday.Date;
			TypeLabel = publicHoliday.IdTypeNavigation.Description;

			switch(publicHoliday.IdType)
			{
				case 1:
					//festivo nacional
					Colour = "#7FD028";
					break;
				case 2:
					//festivo regional
					Colour = "#559AEC";
					break;
				default:
					//festivo local
					Colour = "#F1F";
					break;
			}

		}
		#endregion
	}
}
