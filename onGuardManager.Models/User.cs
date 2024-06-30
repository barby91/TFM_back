namespace onGuardManager.Models.Entities;

public partial class User //: ICloneable
{
    public decimal Id { get; set; }

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public decimal HolidayCurrentPeridod { get; set; }

    public decimal HolidayPreviousPeriod { get; set; }

    public decimal IdLevel { get; set; }

    public decimal IdCenter { get; set; }

    public decimal IdRol { get; set; }

    public decimal IdSpecialty { get; set; }

    public decimal IdUnity { get; set; }

    public virtual ICollection<AskedHoliday> AskedHolidays { get; set; } = new List<AskedHoliday>();
	
	public virtual ICollection<DayGuard> dayGuardsAssigned { get; set; } = new List<DayGuard>();
	//public virtual ICollection<DayGuardUser> dayGuardsUser { get; set; } = new List<DayGuardUser>();

	public virtual Center IdCenterNavigation { get; set; } = null!;

    public virtual Level IdLevelNavigation { get; set; } = null!;

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual Specialty IdSpecialtyNavigation { get; set; } = null!;

    public virtual Unity IdUnityNavigation { get; set; } = null!;


	#region interface ICloneable
	/*public object Clone()
	{
		return new User()
		{
			Id = this.Id,
			Name = this.Name,
			Surname = this.Surname,
			Email = this.Email,
			HolidayCurrentPeridod = this.HolidayCurrentPeridod,
			HolidayPreviousPeriod = this.HolidayPreviousPeriod,
			IdLevel = this.IdLevel,
			IdCenter = this.IdCenter,
			IdRol = this.IdRol,
			IdSpecialty = this.IdSpecialty,
			IdUnity = this.IdUnity
		};
	}*/

	#endregion

}
