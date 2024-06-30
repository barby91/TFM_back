namespace onGuardManager.Models.Entities;

public partial class PublicHoliday
{
    public decimal Id { get; set; }

    public DateOnly Date { get; set; }

    public decimal IdType { get; set; }

    public virtual PublicHolidayType IdTypeNavigation { get; set; } = null!;
}
