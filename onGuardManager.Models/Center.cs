namespace onGuardManager.Models.Entities;

public partial class Center
{
    public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string City { get; set; } = null!;

    public virtual ICollection<Specialty> Specialties { get; set; } = new List<Specialty>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
