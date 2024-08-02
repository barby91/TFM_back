using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models;

public class UnityModel
{
	#region properties
	public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

	public decimal? IdSpecialty { get; set; }

	public decimal? IdCenter { get; set; }

	public decimal MaxByDay { get; set; }

	public decimal MaxByDayWeekend { get; set; }
	#endregion

	#region constructor
	[JsonConstructor]
	public UnityModel() { }
	public UnityModel(Unity unity)
	{
		Id = unity.Id;
		Name = unity.Name;
		Description = unity.Description;
		IdSpecialty = unity.IdSpecialty;
		IdCenter = unity.IdCenter;
		MaxByDay = unity.MaxByDay;
		MaxByDayWeekend = unity.MaxByDayWeekend;
	}
	#endregion

	#region methdos
	public Unity Map()
	{
		return new Unity()
		{
			Id = this.Id,
			Name = this.Name,
			Description = this.Description,
			IdSpecialty = this.IdSpecialty,
			IdCenter = this.IdCenter,
			MaxByDay = this.MaxByDay,
			MaxByDayWeekend = this.MaxByDayWeekend
		};
	}
	#endregion
}
