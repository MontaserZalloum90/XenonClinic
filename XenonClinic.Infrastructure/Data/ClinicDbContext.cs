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
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

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

        // Supplier configuration
        builder.Entity<Supplier>()
            .HasIndex(s => s.Name);

        // InventoryItem configuration
        builder.Entity<InventoryItem>()
            .HasIndex(i => i.ItemCode)
            .IsUnique();

        builder.Entity<InventoryItem>()
            .HasIndex(i => i.Name);

        builder.Entity<InventoryItem>()
            .HasIndex(i => i.BranchId);

        builder.Entity<InventoryItem>()
            .HasIndex(i => i.Category);

        builder.Entity<InventoryItem>()
            .HasIndex(i => new { i.BranchId, i.Category });

        builder.Entity<InventoryItem>()
            .HasOne(i => i.Branch)
            .WithMany()
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InventoryItem>()
            .HasOne(i => i.Supplier)
            .WithMany(s => s.InventoryItems)
            .HasForeignKey(i => i.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<InventoryItem>()
            .Property(i => i.CostPrice)
            .HasPrecision(18, 2);

        builder.Entity<InventoryItem>()
            .Property(i => i.SellingPrice)
            .HasPrecision(18, 2);

        // InventoryTransaction configuration
        builder.Entity<InventoryTransaction>()
            .HasIndex(t => t.TransactionDate);

        builder.Entity<InventoryTransaction>()
            .HasIndex(t => t.InventoryItemId);

        builder.Entity<InventoryTransaction>()
            .HasIndex(t => new { t.InventoryItemId, t.TransactionDate });

        builder.Entity<InventoryTransaction>()
            .HasOne(t => t.InventoryItem)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<InventoryTransaction>()
            .HasOne(t => t.Patient)
            .WithMany()
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<InventoryTransaction>()
            .HasOne(t => t.TransferToBranch)
            .WithMany()
            .HasForeignKey(t => t.TransferToBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<InventoryTransaction>()
            .Property(t => t.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<InventoryTransaction>()
            .Property(t => t.TotalAmount)
            .HasPrecision(18, 2);

        // Department configuration
        builder.Entity<Department>()
            .HasIndex(d => d.Name);

        builder.Entity<Department>()
            .HasIndex(d => d.BranchId);

        builder.Entity<Department>()
            .HasOne(d => d.Branch)
            .WithMany()
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Department>()
            .HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        // JobPosition configuration
        builder.Entity<JobPosition>()
            .HasIndex(j => j.Title);

        builder.Entity<JobPosition>()
            .Property(j => j.MinSalary)
            .HasPrecision(18, 2);

        builder.Entity<JobPosition>()
            .Property(j => j.MaxSalary)
            .HasPrecision(18, 2);

        // Employee configuration
        builder.Entity<Employee>()
            .HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.Entity<Employee>()
            .HasIndex(e => e.EmiratesId)
            .IsUnique();

        builder.Entity<Employee>()
            .HasIndex(e => e.Email);

        builder.Entity<Employee>()
            .HasIndex(e => e.BranchId);

        builder.Entity<Employee>()
            .HasIndex(e => e.DepartmentId);

        builder.Entity<Employee>()
            .HasIndex(e => new { e.BranchId, e.DepartmentId });

        builder.Entity<Employee>()
            .HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Employee>()
            .HasOne(e => e.JobPosition)
            .WithMany(j => j.Employees)
            .HasForeignKey(e => e.JobPositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Employee>()
            .Property(e => e.BasicSalary)
            .HasPrecision(18, 2);

        builder.Entity<Employee>()
            .Property(e => e.HousingAllowance)
            .HasPrecision(18, 2);

        builder.Entity<Employee>()
            .Property(e => e.TransportAllowance)
            .HasPrecision(18, 2);

        builder.Entity<Employee>()
            .Property(e => e.OtherAllowances)
            .HasPrecision(18, 2);

        // Attendance configuration
        builder.Entity<Attendance>()
            .HasIndex(a => a.Date);

        builder.Entity<Attendance>()
            .HasIndex(a => a.EmployeeId);

        builder.Entity<Attendance>()
            .HasIndex(a => new { a.EmployeeId, a.Date });

        builder.Entity<Attendance>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // LeaveRequest configuration
        builder.Entity<LeaveRequest>()
            .HasIndex(l => l.StartDate);

        builder.Entity<LeaveRequest>()
            .HasIndex(l => l.EmployeeId);

        builder.Entity<LeaveRequest>()
            .HasIndex(l => l.Status);

        builder.Entity<LeaveRequest>()
            .HasIndex(l => new { l.EmployeeId, l.Status });

        builder.Entity<LeaveRequest>()
            .HasOne(l => l.Employee)
            .WithMany(e => e.LeaveRequests)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // PerformanceReview configuration
        builder.Entity<PerformanceReview>()
            .HasIndex(p => p.ReviewDate);

        builder.Entity<PerformanceReview>()
            .HasIndex(p => p.EmployeeId);

        builder.Entity<PerformanceReview>()
            .HasIndex(p => new { p.EmployeeId, p.ReviewDate });

        builder.Entity<PerformanceReview>()
            .HasOne(p => p.Employee)
            .WithMany(e => e.PerformanceReviews)
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PerformanceReview>()
            .Property(p => p.OverallRating)
            .HasPrecision(3, 2);
    }
}
