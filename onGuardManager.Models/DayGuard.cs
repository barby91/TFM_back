namespace onGuardManager.Models.Entities;

public partial class DayGuard
{
    public decimal Id { get; set; }

    public DateOnly Day { get; set; }
	public virtual ICollection<User> assignedUsers { get; set; } = new List<User>();
	//public virtual ICollection<DayGuardUser> dayGuardsUser { get; set; } = new List<DayGuardUser>();
}
