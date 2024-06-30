namespace onGuardManager.Models.Entities;

public partial class PublicHolidayType
{
    public decimal Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<PublicHoliday> PublicHolidays { get; set; } = new List<PublicHoliday>();
}
