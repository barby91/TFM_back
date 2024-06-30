namespace onGuardManager.Models.Entities;

public partial class HolidayStatus
{
    public decimal Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<AskedHoliday> AskedHolidays { get; set; } = new List<AskedHoliday>();
}
