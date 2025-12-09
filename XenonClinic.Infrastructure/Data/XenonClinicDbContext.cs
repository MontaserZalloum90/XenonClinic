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

        // Multi-tenant configuration entities
        public DbSet<CompanyType> CompanyTypes { get; set; } = null!;
        public DbSet<ClinicType> ClinicTypes { get; set; } = null!;
        public DbSet<Feature> Features { get; set; } = null!;
        public DbSet<TenantFeature> TenantFeatures { get; set; } = null!;
        public DbSet<CompanyTypeTemplate> CompanyTypeTemplates { get; set; } = null!;
        public DbSet<ClinicTypeTemplate> ClinicTypeTemplates { get; set; } = null!;
        public DbSet<TenantTerminology> TenantTerminology { get; set; } = null!;
        public DbSet<TenantUISchema> TenantUISchemas { get; set; } = null!;
        public DbSet<TenantFormLayout> TenantFormLayouts { get; set; } = null!;
        public DbSet<TenantListLayout> TenantListLayouts { get; set; } = null!;
        public DbSet<TenantNavigation> TenantNavigations { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;

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

            // CompanyType configuration
            builder.Entity<CompanyType>(b =>
            {
                b.HasKey(ct => ct.Code);
                b.Property(ct => ct.Code).HasMaxLength(50);
                b.Property(ct => ct.Name).HasMaxLength(100).IsRequired();

                b.HasOne(ct => ct.Template)
                    .WithOne(t => t.CompanyType)
                    .HasForeignKey<CompanyTypeTemplate>(t => t.CompanyTypeCode);
            });

            // ClinicType configuration
            builder.Entity<ClinicType>(b =>
            {
                b.HasKey(ct => ct.Code);
                b.Property(ct => ct.Code).HasMaxLength(50);
                b.Property(ct => ct.Name).HasMaxLength(100).IsRequired();

                b.HasOne(ct => ct.Template)
                    .WithOne(t => t.ClinicType)
                    .HasForeignKey<ClinicTypeTemplate>(t => t.ClinicTypeCode);
            });

            // Feature configuration
            builder.Entity<Feature>(b =>
            {
                b.HasKey(f => f.Code);
                b.Property(f => f.Code).HasMaxLength(50);
                b.Property(f => f.Name).HasMaxLength(100).IsRequired();
                b.Property(f => f.Category).HasMaxLength(50);
            });

            // TenantFeature configuration
            builder.Entity<TenantFeature>(b =>
            {
                b.HasIndex(tf => new { tf.TenantId, tf.FeatureCode }).IsUnique();

                b.HasOne(tf => tf.Tenant)
                    .WithMany(t => t.Features)
                    .HasForeignKey(tf => tf.TenantId);

                b.HasOne(tf => tf.Feature)
                    .WithMany(f => f.TenantFeatures)
                    .HasForeignKey(tf => tf.FeatureCode);
            });

            // CompanyTypeTemplate configuration
            builder.Entity<CompanyTypeTemplate>(b =>
            {
                b.HasIndex(t => t.CompanyTypeCode).IsUnique();
                b.Property(t => t.CompanyTypeCode).HasMaxLength(50);
            });

            // ClinicTypeTemplate configuration
            builder.Entity<ClinicTypeTemplate>(b =>
            {
                b.HasIndex(t => t.ClinicTypeCode).IsUnique();
                b.Property(t => t.ClinicTypeCode).HasMaxLength(50);
            });

            // TenantTerminology configuration
            builder.Entity<TenantTerminology>(b =>
            {
                b.HasIndex(tt => new { tt.TenantId, tt.Key }).IsUnique();
                b.Property(tt => tt.Key).HasMaxLength(100).IsRequired();
                b.Property(tt => tt.Value).HasMaxLength(500).IsRequired();

                b.HasOne(tt => tt.Tenant)
                    .WithMany(t => t.Terminology)
                    .HasForeignKey(tt => tt.TenantId);
            });

            // TenantUISchema configuration
            builder.Entity<TenantUISchema>(b =>
            {
                b.HasIndex(s => new { s.TenantId, s.EntityName }).IsUnique();
                b.Property(s => s.EntityName).HasMaxLength(50).IsRequired();

                b.HasOne(s => s.Tenant)
                    .WithMany(t => t.UISchemas)
                    .HasForeignKey(s => s.TenantId);
            });

            // TenantFormLayout configuration
            builder.Entity<TenantFormLayout>(b =>
            {
                b.HasIndex(l => new { l.TenantId, l.EntityName }).IsUnique();
                b.Property(l => l.EntityName).HasMaxLength(50).IsRequired();

                b.HasOne(l => l.Tenant)
                    .WithMany(t => t.FormLayouts)
                    .HasForeignKey(l => l.TenantId);
            });

            // TenantListLayout configuration
            builder.Entity<TenantListLayout>(b =>
            {
                b.HasIndex(l => new { l.TenantId, l.EntityName }).IsUnique();
                b.Property(l => l.EntityName).HasMaxLength(50).IsRequired();

                b.HasOne(l => l.Tenant)
                    .WithMany(t => t.ListLayouts)
                    .HasForeignKey(l => l.TenantId);
            });

            // TenantNavigation configuration
            builder.Entity<TenantNavigation>(b =>
            {
                b.HasIndex(n => n.TenantId).IsUnique();

                b.HasOne(n => n.Tenant)
                    .WithOne(t => t.Navigation)
                    .HasForeignKey<TenantNavigation>(n => n.TenantId);
            });

            // Company - CompanyType/ClinicType relationships
            builder.Entity<Company>(b =>
            {
                b.Property(c => c.CompanyTypeCode).HasMaxLength(50);
                b.Property(c => c.ClinicTypeCode).HasMaxLength(50);

                b.HasOne(c => c.CompanyType)
                    .WithMany(ct => ct.Companies)
                    .HasForeignKey(c => c.CompanyTypeCode)
                    .IsRequired(false);

                b.HasOne(c => c.ClinicType)
                    .WithMany(ct => ct.Companies)
                    .HasForeignKey(c => c.ClinicTypeCode)
                    .IsRequired(false);
            });

            // Tenant configuration
            builder.Entity<Tenant>(b =>
            {
                b.Property(t => t.Code).HasMaxLength(50).IsRequired();
                b.HasIndex(t => t.Code).IsUnique();
            });
        }
    }
}
