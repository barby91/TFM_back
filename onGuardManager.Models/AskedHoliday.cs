namespace onGuardManager.Models.Entities;

public partial class AskedHoliday
{
    public decimal Id { get; set; }

    public DateOnly DateFrom { get; set; }

    public DateOnly DateTo { get; set; }

    public string Period { get; set; } = null!;

    public decimal IdStatus { get; set; }

    public decimal IdUser { get; set; }

    public virtual HolidayStatus IdStatusNavigation { get; set; } = null!;

    public virtual User IdUserNavigation { get; set; } = null!;
}
