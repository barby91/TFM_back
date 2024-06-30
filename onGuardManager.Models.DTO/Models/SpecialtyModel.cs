using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models;

public class SpecialtyModel
{
	#region properties
	public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

	public decimal IdCenter { get; set; }

	public int TotalUsers { get; set; }

	public decimal MaxGuards { get; set; }

	public virtual List<UnityModel> Unities { get; set; } = new List<UnityModel>();
	#endregion

	#region constructor
	[JsonConstructor]
	public SpecialtyModel() { }
	public SpecialtyModel(Specialty specialty)
	{
		Id = specialty.Id;
		Name = specialty.Name;
		Description = specialty.Description;
		IdCenter = specialty.IdCenter;
		TotalUsers = specialty.Users.Count;
		MaxGuards = specialty.MaxGuards;
		Unities = new List<UnityModel>();
		foreach(Unity unit in specialty.Unities)
		{
			Unities.Add(new UnityModel(unit));
		}
	}
	#endregion

	#region methdos
	public Specialty Map()
	{
		return new Specialty()
		{
			Id = this.Id,
			Name = this.Name,
			Description = this.Description,
			IdCenter = this.IdCenter,
			MaxGuards = this.MaxGuards,
			Unities = this.Unities.Select(u => u.Map()).ToList()
		};
	}
	#endregion
}
