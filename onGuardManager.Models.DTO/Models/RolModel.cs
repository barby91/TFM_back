using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models
{
	public class RolModel
	{
		#region properties
		public decimal Id { get; set; }

		public string Name { get; set; } = null!;
		#endregion

		#region constructor
		[JsonConstructor]
		public RolModel() { }
		public RolModel(Rol rol)
		{
			Id = rol.Id;
			Name = rol.Name;
		}
		#endregion
	}
}