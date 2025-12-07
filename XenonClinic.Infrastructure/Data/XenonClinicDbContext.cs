using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;

namespace XenonClinic.Infrastructure.Data
{
    public class XenonClinicDbContext : IdentityDbContext<ApplicationUser>
    {
        public XenonClinicDbContext(DbContextOptions<XenonClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<AudiologyVisit> AudiologyVisits { get; set; } = null!;
        public DbSet<Audiogram> Audiograms { get; set; } = null!;
        public DbSet<HearingDevice> HearingDevices { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<UserBranch> UserBranches { get; set; } = null!;
        public DbSet<LicenseConfig> LicenseConfigs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // UserBranch junction
            builder.Entity<UserBranch>(b =>
            {
                b.HasKey(ub => new { ub.UserId, ub.BranchId });

                b.HasOne(ub => ub.User)
                    .WithMany(u => u.UserBranches)
                    .HasForeignKey(ub => ub.UserId);

                b.HasOne(ub => ub.Branch)
                    .WithMany(br => br.UserBranches)
                    .HasForeignKey(ub => ub.BranchId);
            });

            // Basic relationships (conventions handle most, but these make intent explicit)
            builder.Entity<Branch>()
                .HasMany(b => b.Patients)
                .WithOne(p => p.Branch!)
                .HasForeignKey(p => p.BranchId);

            builder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient!)
                .HasForeignKey(a => a.PatientId);

            builder.Entity<Patient>()
                .HasMany(p => p.Visits)
                .WithOne(v => v.Patient!)
                .HasForeignKey(v => v.PatientId);

            builder.Entity<Patient>()
                .HasMany(p => p.Devices)
                .WithOne(d => d.Patient!)
                .HasForeignKey(d => d.PatientId);

            builder.Entity<Patient>()
                .HasMany(p => p.Invoices)
                .WithOne(i => i.Patient!)
                .HasForeignKey(i => i.PatientId);

            builder.Entity<AudiologyVisit>()
                .HasOne(v => v.Audiogram)
                .WithOne(a => a.Visit!)
                .HasForeignKey<Audiogram>(a => a.AudiologyVisitId);
        }
    }
}
