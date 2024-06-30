using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models;

public class PendingAskedHolidayModel
{
	#region properties
	public decimal Id { get; set; }

	public DateOnly DateFrom { get; set; }

	public DateOnly DateTo { get; set; }

	public string Period { get; set; }

	public string NameSurname { get; set; }

	public decimal IdUser { get; set; }
	#endregion

	#region constructor
	[JsonConstructor]
	public PendingAskedHolidayModel() { }
	public PendingAskedHolidayModel(AskedHoliday askedHolidays)
	{
		Id = askedHolidays.Id;
		DateFrom  = askedHolidays.DateFrom;
		DateTo = askedHolidays.DateTo;
		Period = askedHolidays.Period;
		NameSurname = askedHolidays.IdUserNavigation.Name + " " + askedHolidays.IdUserNavigation.Surname;
		IdUser = askedHolidays.IdUser;
	}
	#endregion
}
