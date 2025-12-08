using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Data;

public class ClinicDbContext : IdentityDbContext<ApplicationUser>
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<LicenseConfig> LicenseConfigs => Set<LicenseConfig>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AudiologyVisit> AudiologyVisits => Set<AudiologyVisit>();
    public DbSet<Audiogram> Audiograms => Set<Audiogram>();
    public DbSet<HearingDevice> HearingDevices => Set<HearingDevice>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserBranch>().HasKey(ub => new { ub.UserId, ub.BranchId });
        builder.Entity<UserBranch>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.UserBranches)
            .HasForeignKey(ub => ub.UserId);
        builder.Entity<UserBranch>()
            .HasOne(ub => ub.Branch)
            .WithMany(b => b.UserBranches)
            .HasForeignKey(ub => ub.BranchId);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.PrimaryBranch)
            .WithMany(b => b.PrimaryUsers)
            .HasForeignKey(u => u.PrimaryBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Patient indexes
        builder.Entity<Patient>()
            .HasIndex(p => p.EmiratesId)
            .IsUnique();

        builder.Entity<Patient>()
            .HasIndex(p => p.FullNameEn);

        builder.Entity<Patient>()
            .HasIndex(p => p.BranchId);

        builder.Entity<Patient>()
            .HasOne(p => p.Branch)
            .WithMany(b => b.Patients)
            .HasForeignKey(p => p.BranchId);

        // Appointment indexes
        builder.Entity<Appointment>()
            .HasIndex(a => a.StartTime);

        builder.Entity<Appointment>()
            .HasIndex(a => a.BranchId);

        builder.Entity<Appointment>()
            .HasIndex(a => new { a.BranchId, a.StartTime });

        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId);

        builder.Entity<Appointment>()
            .HasOne(a => a.Branch)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BranchId);

        // AudiologyVisit indexes
        builder.Entity<AudiologyVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<AudiologyVisit>()
            .HasIndex(v => v.BranchId);

        builder.Entity<AudiologyVisit>()
            .HasIndex(v => new { v.BranchId, v.VisitDate });

        builder.Entity<AudiologyVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PatientId);

        builder.Entity<AudiologyVisit>()
            .HasOne(v => v.Branch)
            .WithMany(b => b.Visits)
            .HasForeignKey(v => v.BranchId);

        builder.Entity<Audiogram>()
            .HasOne(a => a.Visit)
            .WithOne(v => v.Audiogram)
            .HasForeignKey<Audiogram>(a => a.AudiologyVisitId);

        builder.Entity<HearingDevice>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.Devices)
            .HasForeignKey(d => d.PatientId);

        builder.Entity<HearingDevice>()
            .HasOne(d => d.Branch)
            .WithMany(b => b.Devices)
            .HasForeignKey(d => d.BranchId);

        // Invoice indexes
        builder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceDate);

        builder.Entity<Invoice>()
            .HasIndex(i => i.BranchId);

        builder.Entity<Invoice>()
            .HasIndex(i => new { i.BranchId, i.InvoiceDate });

        builder.Entity<Invoice>()
            .HasOne(i => i.Patient)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PatientId);

        builder.Entity<Invoice>()
            .HasOne(i => i.Branch)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BranchId);
    }
}
