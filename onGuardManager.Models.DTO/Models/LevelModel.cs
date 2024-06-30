using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models
{
	public class LevelModel
	{
		#region properties
		public decimal Id { get; set; }

		public string Name { get; set; } = null!;
		#endregion

		#region constructor
		[JsonConstructor]
		public LevelModel() { }
		public LevelModel(Level level)
		{
			Id = level.Id;
			Name = level.Name;
		}
		#endregion
	}
}
