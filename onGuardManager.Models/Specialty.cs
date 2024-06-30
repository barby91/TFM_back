namespace onGuardManager.Models.Entities;

public partial class Specialty
{
    public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal IdCenter { get; set; }

    public decimal MaxGuards { get; set; }

    public virtual Center IdCenterNavigation { get; set; } = null!;

    //public virtual ICollection<Rule> Rules { get; set; } = new List<Rule>();

    public virtual ICollection<Unity> Unities { get; set; } = new List<Unity>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
