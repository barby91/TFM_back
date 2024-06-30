using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models;

public class AskedHolidayModel
{
	#region properties
	public decimal Id { get; set; }

	public DateOnly DateFrom { get; set; }

	public DateOnly DateTo { get; set; }

	public string Period { get; set; } = string.Empty;

	public string StatusDes { get; set; } = string.Empty;

	public int IdUser { get; set; }
	#endregion

	#region constructor
	[JsonConstructor]
	public AskedHolidayModel() { }
	public AskedHolidayModel(AskedHoliday askedHolidays)
	{
		Id = askedHolidays.Id;
		DateFrom  = askedHolidays.DateFrom;
		DateTo = askedHolidays.DateTo;
		Period = askedHolidays.Period;
		StatusDes = askedHolidays.IdStatusNavigation.Description;
		IdUser = (int)askedHolidays.IdUser;
	}
	#endregion

	#region methdos
	public AskedHoliday Map()
	{
		return new AskedHoliday()
		{
			Id = this.Id,
			DateFrom = this.DateFrom,
			DateTo = this.DateTo,
			Period = this.Period,
			IdUser = this.IdUser
		};
	}
	#endregion
}
