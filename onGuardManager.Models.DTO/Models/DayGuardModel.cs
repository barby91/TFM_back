using onGuardManager.Models.Entities;
using System.Text.Json.Serialization;

namespace onGuardManager.Models.DTO.Models;

public class DayGuardModel
{
	#region properties
	public decimal Id { get; set; }

	public DateOnly Day { get; set; }

	public virtual ICollection<UserModel> assignedUsers { get; set; } = new List<UserModel>();

	#endregion

	#region constructor
	[JsonConstructor]
	public DayGuardModel() { }
	public DayGuardModel(DayGuard dayGuard)
	{
		Id = dayGuard.Id;
		Day = dayGuard.Day;
		assignedUsers = new List<UserModel>();
		List<User> asignedUserOrder = dayGuard.assignedUsers.OrderBy(u => u.Name).ToList();
		foreach (User user in asignedUserOrder)
		{
			assignedUsers.Add(new UserModel(user, new List<PublicHolidayModel>()));
		}
	}
	#endregion

}
