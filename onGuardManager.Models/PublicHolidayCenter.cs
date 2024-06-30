namespace onGuardManager.Models.Entities;

public partial class PublicHolidayCenter
{
    public decimal IdCenter { get; set; }

    public decimal IdPublicHoliday { get; set; }

    public virtual Center IdCenterNavigation { get; set; } = null!;

    public virtual PublicHoliday IdPublicHolidayNavigation { get; set; } = null!;
}
