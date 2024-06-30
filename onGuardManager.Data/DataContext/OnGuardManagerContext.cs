using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using onGuardManager.Models.Entities;

namespace onGuardManager.Data.DataContext;

public partial class OnGuardManagerContext : DbContext
{
    public OnGuardManagerContext()
    {
    }

    public OnGuardManagerContext(DbContextOptions<OnGuardManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AskedHoliday> AskedHolidays { get; set; }

    public virtual DbSet<Center> Centers { get; set; }

    public virtual DbSet<DayGuard> DayGuards { get; set; }

    public virtual DbSet<HolidayStatus> HolidayStatuses { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<PublicHoliday> PublicHolidays { get; set; }

    public virtual DbSet<PublicHolidayCenter> PublicHolidayCenters { get; set; }

    public virtual DbSet<PublicHolidayType> PublicHolidayTypes { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    //public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<Unity> Unities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AskedHoliday>(entity =>
        {
            entity.ToTable("ASKED_HOLIDAY");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdStatus).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdUser).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Period).HasMaxLength(10);

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.AskedHolidays)
                .HasForeignKey(d => d.IdStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ASKED_HOLIDAY_HOLIDAY_STATUS");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.AskedHolidays)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_ASKED_HOLIDAY_USER");
        });

        modelBuilder.Entity<Center>(entity =>
        {
            entity.ToTable("CENTER");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<DayGuard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DAYGUARD");

            entity.ToTable("DAY_GUARD");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");

            entity.HasMany(d => d.assignedUsers).WithMany(p => p.dayGuardsAssigned)
                .UsingEntity<Dictionary<string, object>>(
                    "DayGuardUser",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("IdUser")
                        .HasConstraintName("FK_DAY_GUARD_USER_USER"),
                    l => l.HasOne<DayGuard>().WithMany()
                        .HasForeignKey("IdGuard")
                        .HasConstraintName("FK_DAY_GUARD_USER_DAY_GUARD"),
                    j =>
                    {
                        j.HasKey("IdGuard", "IdUser");
                        j.ToTable("DAY_GUARD_USER");
                        j.IndexerProperty<decimal>("IdGuard").HasColumnType("numeric(18, 0)");
                        j.IndexerProperty<decimal>("IdUser").HasColumnType("numeric(18, 0)");
                    });
        });

        modelBuilder.Entity<HolidayStatus>(entity =>
        {
            entity.ToTable("HOLIDAY_STATUS");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(15);
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_2");

            entity.ToTable("LEVEL");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PublicHoliday>(entity =>
        {
            entity.ToTable("PUBLIC_HOLIDAY");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdType).HasColumnType("numeric(18, 0)");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.PublicHolidays)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PUBLIC_HOLIDAY_PUBLIC_HOLIDAY_TYPE");
        });

        modelBuilder.Entity<PublicHolidayCenter>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PUBLIC_HOLIDAY_CENTER");

            entity.Property(e => e.IdCenter).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdPublicHoliday).HasColumnType("numeric(18, 0)");

            entity.HasOne(d => d.IdCenterNavigation).WithMany()
                .HasForeignKey(d => d.IdCenter)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PUBLIC_HOLIDAY_CENTER_CENTER");

            entity.HasOne(d => d.IdPublicHolidayNavigation).WithMany()
                .HasForeignKey(d => d.IdPublicHoliday)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PUBLIC_HOLIDAY_CENTER_PUBLIC_HOLIDAY");
        });

        modelBuilder.Entity<PublicHolidayType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Type");

            entity.ToTable("PUBLIC_HOLIDAY_TYPE");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(10);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ROL2");

            entity.ToTable("ROL");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        /*modelBuilder.Entity<Rule>(entity =>
        {
            entity.ToTable("RULE");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdSpecialty).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.Operation).HasMaxLength(50);
            entity.Property(e => e.Priority).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.PropOperations).HasMaxLength(100);
            entity.Property(e => e.PropValues).HasMaxLength(100);
            entity.Property(e => e.Properties).HasMaxLength(100);
            entity.Property(e => e.SubMetOperations).HasMaxLength(100);
            entity.Property(e => e.SubMetValues).HasMaxLength(100);
            entity.Property(e => e.SubMethods).HasMaxLength(100);
            entity.Property(e => e.SubPropOperations).HasMaxLength(100);
            entity.Property(e => e.SubPropValues).HasMaxLength(100);
            entity.Property(e => e.SubProperties).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(50);

            entity.HasOne(d => d.IdSpecialtyNavigation).WithMany(p => p.Rules)
                .HasForeignKey(d => d.IdSpecialty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RULE_SPECIALTY");
        });
        */
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SPECIALTY_2");

            entity.ToTable("SPECIALTY");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.IdCenter).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.MaxGuards).HasColumnType("numeric(1, 0)");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.IdCenterNavigation).WithMany(p => p.Specialties)
                .HasForeignKey(d => d.IdCenter)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SPECIALTY_2_CENTER");
        });

        modelBuilder.Entity<Unity>(entity =>
        {
            entity.ToTable("UNITY");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Description).HasMaxLength(150);
            entity.Property(e => e.IdSpecialty).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.IdSpecialtyNavigation).WithMany(p => p.Unities)
                .HasForeignKey(d => d.IdSpecialty)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UNITY_SPECIALTY");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_USER_2");

            entity.ToTable("USER");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.HolidayCurrentPeridod).HasColumnType("numeric(2, 0)");
            entity.Property(e => e.HolidayPreviousPeriod).HasColumnType("numeric(2, 0)");
            entity.Property(e => e.IdCenter).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdLevel).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdRol).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdSpecialty).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.IdUnity).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(20);
            entity.Property(e => e.Surname).HasMaxLength(100);

            entity.HasOne(d => d.IdCenterNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdCenter)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_2_CENTER");

            entity.HasOne(d => d.IdLevelNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_2_LEVEL");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_2_ROL");

            entity.HasOne(d => d.IdSpecialtyNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdSpecialty)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_USER_SPECIALTY");

            entity.HasOne(d => d.IdUnityNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdUnity)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_USER_UNITY");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
