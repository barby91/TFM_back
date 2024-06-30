using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models
{
	public class RealUserModel
	{
		#region properties
		public decimal Id { get; set; }

		public string Name { get; set; } = null!;

		public string Surname { get; set; } = null!;

		public string Email { get; set; } = null!;

		public string Password { get; set; } = null!;

		public int CenterId { get; set; }

		public int LevelId { get; set; }

		public virtual int RolId { get; set; }

		public virtual int SpecialtyId {get; set; }

		public virtual int UnityId { get; set; }
		#endregion

		#region constructor
		[JsonConstructor]
		public RealUserModel() { }
		public RealUserModel(User user)
		{
			this.Name = user.Name;
			this.Surname = user.Surname;
			this.Email = user.Email;
			this.Password = user.Password;
			this.Id = user.Id;
			this.CenterId = (int)user.IdCenter;
			this.LevelId = (int)user.IdLevel;
			this.RolId = (int)user.IdRol;
			this.SpecialtyId = (int)user.IdSpecialty;
			this.UnityId = (int)user.IdUnity;
		}
		#endregion

		#region methdos
		public User Map()
		{
			return new User()
			{
				Id = this.Id,
				Name = this.Name,
				Surname = this.Surname,
				Email = this.Email,
				Password = this.Password,
				IdLevel = this.LevelId,
				IdCenter = this.CenterId,
				IdSpecialty = this.SpecialtyId,
				IdRol = this.RolId,
				IdUnity = this.UnityId
			};
		}
		#endregion

	}
}
