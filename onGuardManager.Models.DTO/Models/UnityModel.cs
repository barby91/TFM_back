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
			IdSpecialty = this.IdSpecialty
		};
	}
	#endregion
}
