namespace onGuardManager.Models.Entities;

public partial class Unity
{
    public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal MaxByDay { get; set; }

    public decimal MaxByDayWeekend { get; set; }

    public decimal? IdSpecialty { get; set; }

    public decimal? IdCenter { get; set; }

    public virtual Specialty? IdSpecialtyNavigation { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
