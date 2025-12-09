using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Dermatology;
using XenonClinic.Core.Entities.Lookups;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Pediatrics;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Entities.Veterinary;
using XenonClinic.Infrastructure.Entities;

namespace XenonClinic.Infrastructure.Data;

public class ClinicDbContext : IdentityDbContext<Entities.ApplicationUser>
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
    }

    // Multi-tenancy entities
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();

    // Existing entities
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<LicenseConfig> LicenseConfigs => Set<LicenseConfig>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientMedicalHistory> PatientMedicalHistories => Set<PatientMedicalHistory>();
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
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
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptItem> GoodsReceiptItems => Set<GoodsReceiptItem>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<ExternalLab> ExternalLabs => Set<ExternalLab>();
    public DbSet<LabTest> LabTests => Set<LabTest>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabOrderItem> LabOrderItems => Set<LabOrderItem>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

    // Authentication configuration entities (from Infrastructure)
    public DbSet<Entities.CompanyAuthSettings> CompanyAuthSettings => Set<Entities.CompanyAuthSettings>();
    public DbSet<Entities.CompanyIdentityProvider> CompanyIdentityProviders => Set<Entities.CompanyIdentityProvider>();

    // Case Management entities
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseType> CaseTypes => Set<CaseType>();
    public DbSet<CaseStatus> CaseStatuses => Set<CaseStatus>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<CaseActivity> CaseActivities => Set<CaseActivity>();

    // Audit Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ========================================
    // Dental Clinic Entities
    // ========================================
    public DbSet<DentalVisit> DentalVisits => Set<DentalVisit>();
    public DbSet<ToothChart> ToothCharts => Set<ToothChart>();
    public DbSet<ToothRecord> ToothRecords => Set<ToothRecord>();
    public DbSet<DentalProcedure> DentalProcedures => Set<DentalProcedure>();
    public DbSet<DentalTreatmentPlan> DentalTreatmentPlans => Set<DentalTreatmentPlan>();
    public DbSet<DentalTreatmentPlanItem> DentalTreatmentPlanItems => Set<DentalTreatmentPlanItem>();
    public DbSet<DentalXRay> DentalXRays => Set<DentalXRay>();

    // ========================================
    // Veterinary Clinic Entities
    // ========================================
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<PetOwner> PetOwners => Set<PetOwner>();
    public DbSet<VetVisit> VetVisits => Set<VetVisit>();
    public DbSet<VetProcedure> VetProcedures => Set<VetProcedure>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();
    public DbSet<GroomingAppointment> GroomingAppointments => Set<GroomingAppointment>();
    public DbSet<BoardingReservation> BoardingReservations => Set<BoardingReservation>();

    // ========================================
    // Ophthalmology Clinic Entities
    // ========================================
    public DbSet<OphthalmologyVisit> OphthalmologyVisits => Set<OphthalmologyVisit>();
    public DbSet<VisionTest> VisionTests => Set<VisionTest>();
    public DbSet<EyeExam> EyeExams => Set<EyeExam>();
    public DbSet<EyePrescription> EyePrescriptions => Set<EyePrescription>();
    public DbSet<EyeProcedure> EyeProcedures => Set<EyeProcedure>();
    public DbSet<EyeCondition> EyeConditions => Set<EyeCondition>();

    // ========================================
    // Dermatology Clinic Entities
    // ========================================
    public DbSet<DermatologyVisit> DermatologyVisits => Set<DermatologyVisit>();
    public DbSet<SkinCondition> SkinConditions => Set<SkinCondition>();
    public DbSet<LesionRecord> LesionRecords => Set<LesionRecord>();
    public DbSet<SkinProcedure> SkinProcedures => Set<SkinProcedure>();
    public DbSet<SkinTreatmentPlan> SkinTreatmentPlans => Set<SkinTreatmentPlan>();

    // ========================================
    // Physiotherapy Clinic Entities
    // ========================================
    public DbSet<PhysioAssessment> PhysioAssessments => Set<PhysioAssessment>();
    public DbSet<PhysioSession> PhysioSessions => Set<PhysioSession>();
    public DbSet<ExerciseProgram> ExercisePrograms => Set<ExerciseProgram>();
    public DbSet<ExerciseProgramItem> ExerciseProgramItems => Set<ExerciseProgramItem>();
    public DbSet<RangeOfMotionRecord> RangeOfMotionRecords => Set<RangeOfMotionRecord>();
    public DbSet<FunctionalOutcome> FunctionalOutcomes => Set<FunctionalOutcome>();

    // ========================================
    // Pediatrics Clinic Entities
    // ========================================
    public DbSet<PediatricVisit> PediatricVisits => Set<PediatricVisit>();
    public DbSet<GrowthRecord> GrowthRecords => Set<GrowthRecord>();
    public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
    public DbSet<DevelopmentalMilestone> DevelopmentalMilestones => Set<DevelopmentalMilestone>();
    public DbSet<DevelopmentalScreening> DevelopmentalScreenings => Set<DevelopmentalScreening>();
    public DbSet<NewbornRecord> NewbornRecords => Set<NewbornRecord>();

    // Dynamic Lookup entities
    public DbSet<AppointmentTypeLookup> AppointmentTypeLookups => Set<AppointmentTypeLookup>();
    public DbSet<AppointmentStatusLookup> AppointmentStatusLookups => Set<AppointmentStatusLookup>();
    public DbSet<PaymentMethodLookup> PaymentMethodLookups => Set<PaymentMethodLookup>();
    public DbSet<PaymentStatusLookup> PaymentStatusLookups => Set<PaymentStatusLookup>();
    public DbSet<CasePriorityLookup> CasePriorityLookups => Set<CasePriorityLookup>();
    public DbSet<CaseActivityTypeLookup> CaseActivityTypeLookups => Set<CaseActivityTypeLookup>();
    public DbSet<CaseActivityStatusLookup> CaseActivityStatusLookups => Set<CaseActivityStatusLookup>();
    public DbSet<CaseNoteTypeLookup> CaseNoteTypeLookups => Set<CaseNoteTypeLookup>();
    public DbSet<LeaveTypeLookup> LeaveTypeLookups => Set<LeaveTypeLookup>();
    public DbSet<LeaveStatusLookup> LeaveStatusLookups => Set<LeaveStatusLookup>();
    public DbSet<EmploymentStatusLookup> EmploymentStatusLookups => Set<EmploymentStatusLookup>();
    public DbSet<InventoryCategoryLookup> InventoryCategoryLookups => Set<InventoryCategoryLookup>();
    public DbSet<HearingLossTypeLookup> HearingLossTypeLookups => Set<HearingLossTypeLookup>();
    public DbSet<AccountTypeLookup> AccountTypeLookups => Set<AccountTypeLookup>();
    public DbSet<ExpenseStatusLookup> ExpenseStatusLookups => Set<ExpenseStatusLookup>();
    public DbSet<QuotationStatusLookup> QuotationStatusLookups => Set<QuotationStatusLookup>();
    public DbSet<SaleStatusLookup> SaleStatusLookups => Set<SaleStatusLookup>();
    public DbSet<PurchaseOrderStatusLookup> PurchaseOrderStatusLookups => Set<PurchaseOrderStatusLookup>();
    public DbSet<GoodsReceiptStatusLookup> GoodsReceiptStatusLookups => Set<GoodsReceiptStatusLookup>();
    public DbSet<SupplierPaymentStatusLookup> SupplierPaymentStatusLookups => Set<SupplierPaymentStatusLookup>();
    public DbSet<VoucherStatusLookup> VoucherStatusLookups => Set<VoucherStatusLookup>();
    public DbSet<TransactionTypeLookup> TransactionTypeLookups => Set<TransactionTypeLookup>();
    public DbSet<LabOrderStatusLookup> LabOrderStatusLookups => Set<LabOrderStatusLookup>();
    public DbSet<LabResultStatusLookup> LabResultStatusLookups => Set<LabResultStatusLookup>();
    public DbSet<SpecimenTypeLookup> SpecimenTypeLookups => Set<SpecimenTypeLookup>();
    public DbSet<TestCategoryLookup> TestCategoryLookups => Set<TestCategoryLookup>();
    public DbSet<InventoryTransactionTypeLookup> InventoryTransactionTypeLookups => Set<InventoryTransactionTypeLookup>();
    public DbSet<AttendanceStatusLookup> AttendanceStatusLookups => Set<AttendanceStatusLookup>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ========================================
        // Multi-tenancy Configuration
        // ========================================

        // Tenant configuration
        builder.Entity<Tenant>()
            .HasIndex(t => t.Code)
            .IsUnique();

        builder.Entity<Tenant>()
            .HasIndex(t => t.Name);

        builder.Entity<Tenant>()
            .HasIndex(t => t.IsActive);

        builder.Entity<Tenant>()
            .HasMany(t => t.Companies)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Tenant.Users relationship moved to Infrastructure (ApplicationUser has TenantId)
        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.TenantId);

        builder.Entity<Tenant>()
            .HasOne(t => t.Settings)
            .WithOne(s => s.Tenant)
            .HasForeignKey<TenantSettings>(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Company configuration
        builder.Entity<Company>()
            .HasIndex(c => c.Code)
            .IsUnique();

        builder.Entity<Company>()
            .HasIndex(c => c.Name);

        builder.Entity<Company>()
            .HasIndex(c => c.TenantId);

        builder.Entity<Company>()
            .HasIndex(c => c.IsActive);

        builder.Entity<Company>()
            .HasIndex(c => new { c.TenantId, c.Code });

        builder.Entity<Company>()
            .HasMany(c => c.Branches)
            .WithOne(b => b.Company)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: Company.Users relationship moved to Infrastructure (ApplicationUser has CompanyId)
        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.CompanyId);

        // TenantSettings configuration
        builder.Entity<TenantSettings>()
            .HasIndex(ts => ts.TenantId)
            .IsUnique();

        builder.Entity<TenantSettings>()
            .Property(ts => ts.ExpenseApprovalThreshold)
            .HasPrecision(18, 2);

        builder.Entity<TenantSettings>()
            .Property(ts => ts.DefaultTaxRate)
            .HasPrecision(5, 2);

        // CompanySettings configuration
        builder.Entity<CompanySettings>()
            .HasIndex(cs => cs.CompanyId)
            .IsUnique();

        builder.Entity<CompanySettings>()
            .HasOne(cs => cs.Company)
            .WithOne(c => c.Settings)
            .HasForeignKey<CompanySettings>(cs => cs.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Branch configuration - add company relationship
        builder.Entity<Branch>()
            .HasIndex(b => b.Code)
            .IsUnique();

        builder.Entity<Branch>()
            .HasIndex(b => b.CompanyId);

        builder.Entity<Branch>()
            .HasIndex(b => b.IsActive);

        builder.Entity<Branch>()
            .HasIndex(b => new { b.CompanyId, b.Code });

        // ApplicationUser multi-tenancy relationships (Infrastructure entity)
        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.IsActive);

        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.IsSuperAdmin);

        // ========================================
        // Existing Entity Configuration
        // ========================================

        builder.Entity<UserBranch>().HasKey(ub => new { ub.UserId, ub.BranchId });
        builder.Entity<UserBranch>()
            .HasOne<Entities.ApplicationUser>()
            .WithMany()
            .HasForeignKey(ub => ub.UserId);
        builder.Entity<UserBranch>()
            .HasOne(ub => ub.Branch)
            .WithMany(b => b.UserBranches)
            .HasForeignKey(ub => ub.BranchId);

        builder.Entity<Entities.ApplicationUser>()
            .HasOne<Branch>()
            .WithMany()
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

        // PatientMedicalHistory configuration
        builder.Entity<PatientMedicalHistory>()
            .HasIndex(m => m.PatientId)
            .IsUnique();

        builder.Entity<PatientMedicalHistory>()
            .HasOne(m => m.Patient)
            .WithOne(p => p.MedicalHistory)
            .HasForeignKey<PatientMedicalHistory>(m => m.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // PatientDocument configuration
        builder.Entity<PatientDocument>()
            .HasIndex(d => d.PatientId);

        builder.Entity<PatientDocument>()
            .HasIndex(d => d.UploadDate);

        builder.Entity<PatientDocument>()
            .HasIndex(d => d.DocumentType);

        builder.Entity<PatientDocument>()
            .HasIndex(d => d.IsActive);

        builder.Entity<PatientDocument>()
            .HasIndex(d => new { d.PatientId, d.UploadDate });

        builder.Entity<PatientDocument>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

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

        builder.Entity<Appointment>()
            .HasIndex(a => a.ProviderId);

        builder.Entity<Appointment>()
            .HasOne(a => a.Provider)
            .WithMany()
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.SetNull);

        // AudiologyVisit indexes
        builder.Entity<AudiologyVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<AudiologyVisit>()
            .HasIndex(v => v.BranchId);

        builder.Entity<AudiologyVisit>()
            .HasIndex(v => new { v.BranchId, v.VisitDate });

        builder.Entity<AudiologyVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.AudiologyVisits)
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
            .WithMany(p => p.HearingDevices)
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

        // Sale configuration
        builder.Entity<Sale>()
            .HasIndex(s => s.InvoiceNumber)
            .IsUnique();

        builder.Entity<Sale>()
            .HasIndex(s => s.SaleDate);

        builder.Entity<Sale>()
            .HasIndex(s => s.BranchId);

        builder.Entity<Sale>()
            .HasIndex(s => s.PatientId);

        builder.Entity<Sale>()
            .HasIndex(s => s.Status);

        builder.Entity<Sale>()
            .HasIndex(s => s.PaymentStatus);

        builder.Entity<Sale>()
            .HasIndex(s => new { s.BranchId, s.SaleDate });

        builder.Entity<Sale>()
            .HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Sale>()
            .HasOne(s => s.Quotation)
            .WithMany(q => q.Sales)
            .HasForeignKey(s => s.QuotationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Sale>()
            .Property(s => s.SubTotal)
            .HasPrecision(18, 2);

        builder.Entity<Sale>()
            .Property(s => s.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<Sale>()
            .Property(s => s.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<Sale>()
            .Property(s => s.Total)
            .HasPrecision(18, 2);

        builder.Entity<Sale>()
            .Property(s => s.PaidAmount)
            .HasPrecision(18, 2);

        // SaleItem configuration
        builder.Entity<SaleItem>()
            .HasIndex(i => i.SaleId);

        builder.Entity<SaleItem>()
            .HasOne(i => i.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SaleItem>()
            .HasOne(i => i.InventoryItem)
            .WithMany()
            .HasForeignKey(i => i.InventoryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SaleItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<SaleItem>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<SaleItem>()
            .Property(i => i.Subtotal)
            .HasPrecision(18, 2);

        builder.Entity<SaleItem>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<SaleItem>()
            .Property(i => i.Total)
            .HasPrecision(18, 2);

        // Payment configuration
        builder.Entity<Payment>()
            .HasIndex(p => p.PaymentNumber)
            .IsUnique();

        builder.Entity<Payment>()
            .HasIndex(p => p.PaymentDate);

        builder.Entity<Payment>()
            .HasIndex(p => p.SaleId);

        builder.Entity<Payment>()
            .HasIndex(p => new { p.SaleId, p.PaymentDate });

        builder.Entity<Payment>()
            .HasOne(p => p.Sale)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // Quotation configuration
        builder.Entity<Quotation>()
            .HasIndex(q => q.QuotationNumber)
            .IsUnique();

        builder.Entity<Quotation>()
            .HasIndex(q => q.QuotationDate);

        builder.Entity<Quotation>()
            .HasIndex(q => q.BranchId);

        builder.Entity<Quotation>()
            .HasIndex(q => q.PatientId);

        builder.Entity<Quotation>()
            .HasIndex(q => q.Status);

        builder.Entity<Quotation>()
            .HasIndex(q => new { q.BranchId, q.QuotationDate });

        builder.Entity<Quotation>()
            .HasOne(q => q.Patient)
            .WithMany()
            .HasForeignKey(q => q.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quotation>()
            .HasOne(q => q.Branch)
            .WithMany()
            .HasForeignKey(q => q.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Quotation>()
            .Property(q => q.SubTotal)
            .HasPrecision(18, 2);

        builder.Entity<Quotation>()
            .Property(q => q.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<Quotation>()
            .Property(q => q.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<Quotation>()
            .Property(q => q.Total)
            .HasPrecision(18, 2);

        // QuotationItem configuration
        builder.Entity<QuotationItem>()
            .HasIndex(i => i.QuotationId);

        builder.Entity<QuotationItem>()
            .HasOne(i => i.Quotation)
            .WithMany(q => q.Items)
            .HasForeignKey(i => i.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuotationItem>()
            .HasOne(i => i.InventoryItem)
            .WithMany()
            .HasForeignKey(i => i.InventoryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<QuotationItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<QuotationItem>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<QuotationItem>()
            .Property(i => i.Subtotal)
            .HasPrecision(18, 2);

        builder.Entity<QuotationItem>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<QuotationItem>()
            .Property(i => i.Total)
            .HasPrecision(18, 2);

        // Supplier configuration (enhanced for procurement)
        builder.Entity<Supplier>()
            .HasIndex(s => s.Code)
            .IsUnique();

        builder.Entity<Supplier>()
            .HasIndex(s => s.BranchId);

        builder.Entity<Supplier>()
            .HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Supplier>()
            .Property(s => s.CreditLimit)
            .HasPrecision(18, 2);

        // PurchaseOrder configuration
        builder.Entity<PurchaseOrder>()
            .HasIndex(p => p.OrderNumber)
            .IsUnique();

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => p.OrderDate);

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => p.BranchId);

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => p.SupplierId);

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => p.Status);

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => new { p.BranchId, p.OrderDate });

        builder.Entity<PurchaseOrder>()
            .HasIndex(p => new { p.SupplierId, p.OrderDate });

        builder.Entity<PurchaseOrder>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PurchaseOrder>()
            .HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.SubTotal)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.ShippingCost)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.Total)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrder>()
            .Property(p => p.ReceivedAmount)
            .HasPrecision(18, 2);

        // PurchaseOrderItem configuration
        builder.Entity<PurchaseOrderItem>()
            .HasIndex(i => i.PurchaseOrderId);

        builder.Entity<PurchaseOrderItem>()
            .HasOne(i => i.PurchaseOrder)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PurchaseOrderItem>()
            .HasOne(i => i.InventoryItem)
            .WithMany()
            .HasForeignKey(i => i.InventoryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PurchaseOrderItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrderItem>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrderItem>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<PurchaseOrderItem>()
            .Property(i => i.Total)
            .HasPrecision(18, 2);

        // GoodsReceipt configuration
        builder.Entity<GoodsReceipt>()
            .HasIndex(g => g.ReceiptNumber)
            .IsUnique();

        builder.Entity<GoodsReceipt>()
            .HasIndex(g => g.ReceiptDate);

        builder.Entity<GoodsReceipt>()
            .HasIndex(g => g.BranchId);

        builder.Entity<GoodsReceipt>()
            .HasIndex(g => g.SupplierId);

        builder.Entity<GoodsReceipt>()
            .HasIndex(g => g.PurchaseOrderId);

        builder.Entity<GoodsReceipt>()
            .HasIndex(g => new { g.BranchId, g.ReceiptDate });

        builder.Entity<GoodsReceipt>()
            .HasOne(g => g.PurchaseOrder)
            .WithMany(p => p.GoodsReceipts)
            .HasForeignKey(g => g.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GoodsReceipt>()
            .HasOne(g => g.Supplier)
            .WithMany()
            .HasForeignKey(g => g.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GoodsReceipt>()
            .HasOne(g => g.Branch)
            .WithMany()
            .HasForeignKey(g => g.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // GoodsReceiptItem configuration
        builder.Entity<GoodsReceiptItem>()
            .HasIndex(i => i.GoodsReceiptId);

        builder.Entity<GoodsReceiptItem>()
            .HasOne(i => i.GoodsReceipt)
            .WithMany(g => g.Items)
            .HasForeignKey(i => i.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GoodsReceiptItem>()
            .HasOne(i => i.PurchaseOrderItem)
            .WithMany()
            .HasForeignKey(i => i.PurchaseOrderItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GoodsReceiptItem>()
            .HasOne(i => i.InventoryItem)
            .WithMany()
            .HasForeignKey(i => i.InventoryItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GoodsReceiptItem>()
            .Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        // SupplierPayment configuration
        builder.Entity<SupplierPayment>()
            .HasIndex(p => p.PaymentNumber)
            .IsUnique();

        builder.Entity<SupplierPayment>()
            .HasIndex(p => p.PaymentDate);

        builder.Entity<SupplierPayment>()
            .HasIndex(p => p.SupplierId);

        builder.Entity<SupplierPayment>()
            .HasIndex(p => p.PurchaseOrderId);

        builder.Entity<SupplierPayment>()
            .HasIndex(p => new { p.SupplierId, p.PaymentDate });

        builder.Entity<SupplierPayment>()
            .HasOne(p => p.PurchaseOrder)
            .WithMany(po => po.Payments)
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SupplierPayment>()
            .HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SupplierPayment>()
            .HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SupplierPayment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // ExternalLab configuration
        builder.Entity<ExternalLab>()
            .HasIndex(e => e.Code)
            .IsUnique();

        builder.Entity<ExternalLab>()
            .HasIndex(e => e.Name);

        builder.Entity<ExternalLab>()
            .HasIndex(e => e.BranchId);

        builder.Entity<ExternalLab>()
            .HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // LabTest configuration
        builder.Entity<LabTest>()
            .HasIndex(l => l.TestCode)
            .IsUnique();

        builder.Entity<LabTest>()
            .HasIndex(l => l.TestName);

        builder.Entity<LabTest>()
            .HasIndex(l => l.Category);

        builder.Entity<LabTest>()
            .HasIndex(l => l.BranchId);

        builder.Entity<LabTest>()
            .HasIndex(l => new { l.BranchId, l.Category });

        builder.Entity<LabTest>()
            .HasOne(l => l.Branch)
            .WithMany()
            .HasForeignKey(l => l.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabTest>()
            .HasOne(l => l.ExternalLab)
            .WithMany(e => e.LabTests)
            .HasForeignKey(l => l.ExternalLabId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<LabTest>()
            .Property(l => l.Price)
            .HasPrecision(18, 2);

        // LabOrder configuration
        builder.Entity<LabOrder>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Entity<LabOrder>()
            .HasIndex(o => o.OrderDate);

        builder.Entity<LabOrder>()
            .HasIndex(o => o.BranchId);

        builder.Entity<LabOrder>()
            .HasIndex(o => o.PatientId);

        builder.Entity<LabOrder>()
            .HasIndex(o => o.Status);

        builder.Entity<LabOrder>()
            .HasIndex(o => new { o.BranchId, o.OrderDate });

        builder.Entity<LabOrder>()
            .HasIndex(o => new { o.PatientId, o.OrderDate });

        builder.Entity<LabOrder>()
            .HasOne(o => o.Patient)
            .WithMany()
            .HasForeignKey(o => o.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabOrder>()
            .HasOne(o => o.Branch)
            .WithMany()
            .HasForeignKey(o => o.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabOrder>()
            .HasOne(o => o.ExternalLab)
            .WithMany(e => e.LabOrders)
            .HasForeignKey(o => o.ExternalLabId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<LabOrder>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        // LabOrderItem configuration
        builder.Entity<LabOrderItem>()
            .HasIndex(i => i.LabOrderId);

        builder.Entity<LabOrderItem>()
            .HasOne(i => i.LabOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LabOrderItem>()
            .HasOne(i => i.LabTest)
            .WithMany()
            .HasForeignKey(i => i.LabTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabOrderItem>()
            .Property(i => i.Price)
            .HasPrecision(18, 2);

        // LabResult configuration
        builder.Entity<LabResult>()
            .HasIndex(r => r.LabOrderId);

        builder.Entity<LabResult>()
            .HasIndex(r => r.Status);

        builder.Entity<LabResult>()
            .HasIndex(r => new { r.LabOrderId, r.Status });

        builder.Entity<LabResult>()
            .HasOne(r => r.LabOrder)
            .WithMany(o => o.Results)
            .HasForeignKey(r => r.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LabResult>()
            .HasOne(r => r.LabOrderItem)
            .WithMany()
            .HasForeignKey(r => r.LabOrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LabResult>()
            .HasOne(r => r.LabTest)
            .WithMany()
            .HasForeignKey(r => r.LabTestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Finance Module Configuration

        // Account
        builder.Entity<Account>()
            .HasIndex(a => a.AccountCode)
            .IsUnique();

        builder.Entity<Account>()
            .HasIndex(a => a.BranchId);

        builder.Entity<Account>()
            .HasIndex(a => a.AccountType);

        builder.Entity<Account>()
            .HasIndex(a => a.IsActive);

        builder.Entity<Account>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);

        builder.Entity<Account>()
            .HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Account>()
            .HasOne(a => a.ParentAccount)
            .WithMany(a => a.ChildAccounts)
            .HasForeignKey(a => a.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // ExpenseCategory
        builder.Entity<ExpenseCategory>()
            .HasIndex(ec => ec.BranchId);

        builder.Entity<ExpenseCategory>()
            .HasIndex(ec => ec.IsActive);

        builder.Entity<ExpenseCategory>()
            .HasIndex(ec => ec.AccountId);

        builder.Entity<ExpenseCategory>()
            .HasOne(ec => ec.Branch)
            .WithMany()
            .HasForeignKey(ec => ec.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExpenseCategory>()
            .HasOne(ec => ec.Account)
            .WithMany()
            .HasForeignKey(ec => ec.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // Expense
        builder.Entity<Expense>()
            .HasIndex(e => e.ExpenseNumber)
            .IsUnique();

        builder.Entity<Expense>()
            .HasIndex(e => e.BranchId);

        builder.Entity<Expense>()
            .HasIndex(e => e.ExpenseDate);

        builder.Entity<Expense>()
            .HasIndex(e => e.Status);

        builder.Entity<Expense>()
            .HasIndex(e => e.ExpenseCategoryId);

        builder.Entity<Expense>()
            .HasIndex(e => new { e.BranchId, e.ExpenseDate });

        builder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Entity<Expense>()
            .HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Expense>()
            .HasOne(e => e.ExpenseCategory)
            .WithMany(ec => ec.Expenses)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // FinancialTransaction
        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.TransactionNumber)
            .IsUnique();

        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.BranchId);

        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.TransactionDate);

        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.AccountId);

        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => ft.Status);

        builder.Entity<FinancialTransaction>()
            .HasIndex(ft => new { ft.AccountId, ft.TransactionDate });

        builder.Entity<FinancialTransaction>()
            .Property(ft => ft.Amount)
            .HasPrecision(18, 2);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Branch)
            .WithMany()
            .HasForeignKey(ft => ft.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(ft => ft.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Expense)
            .WithMany()
            .HasForeignKey(ft => ft.ExpenseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<FinancialTransaction>()
            .HasOne(ft => ft.Sale)
            .WithMany()
            .HasForeignKey(ft => ft.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Company Authentication Configuration
        // ========================================

        // CompanyAuthSettings configuration (Infrastructure entity)
        builder.Entity<Entities.CompanyAuthSettings>()
            .HasIndex(cas => cas.CompanyId)
            .IsUnique();

        builder.Entity<Entities.CompanyAuthSettings>()
            .HasIndex(cas => cas.IsEnabled);

        builder.Entity<Entities.CompanyAuthSettings>()
            .HasOne(cas => cas.Company)
            .WithMany()
            .HasForeignKey(cas => cas.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Entities.CompanyAuthSettings>()
            .HasMany(cas => cas.IdentityProviders)
            .WithOne()
            .HasForeignKey(ip => ip.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // CompanyIdentityProvider configuration (Infrastructure entity)
        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasIndex(ip => ip.CompanyId);

        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasIndex(ip => ip.Name);

        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasIndex(ip => ip.IsEnabled);

        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasIndex(ip => ip.IsDefault);

        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasIndex(ip => new { ip.CompanyId, ip.Name })
            .IsUnique();

        builder.Entity<Entities.CompanyIdentityProvider>()
            .HasOne(ip => ip.Company)
            .WithMany()
            .HasForeignKey(ip => ip.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Entities.CompanyIdentityProvider>()
            .Property(ip => ip.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Entity<Entities.CompanyIdentityProvider>()
            .Property(ip => ip.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        // ApplicationUser external login indexes (Infrastructure entity)
        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.ExternalUserId);

        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => u.ExternalProviderName);

        builder.Entity<Entities.ApplicationUser>()
            .HasIndex(u => new { u.CompanyId, u.ExternalProviderName, u.ExternalUserId });

        // ========================================
        // Case Management Configuration
        // ========================================

        // Case configuration
        builder.Entity<Case>()
            .HasIndex(c => c.CaseNumber)
            .IsUnique();

        builder.Entity<Case>()
            .HasIndex(c => c.PatientId);

        builder.Entity<Case>()
            .HasIndex(c => c.BranchId);

        builder.Entity<Case>()
            .HasIndex(c => c.CaseStatusId);

        builder.Entity<Case>()
            .HasIndex(c => c.AssignedToUserId);

        builder.Entity<Case>()
            .HasIndex(c => c.OpenedDate);

        builder.Entity<Case>()
            .HasIndex(c => new { c.BranchId, c.OpenedDate });

        builder.Entity<Case>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.Cases)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Case>()
            .HasOne(c => c.Branch)
            .WithMany(b => b.Cases)
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Case>()
            .HasOne(c => c.CaseType)
            .WithMany(ct => ct.Cases)
            .HasForeignKey(c => c.CaseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Case>()
            .HasOne(c => c.CaseStatus)
            .WithMany(cs => cs.Cases)
            .HasForeignKey(c => c.CaseStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Case>()
            .HasOne(c => c.AssignedToUser)
            .WithMany()
            .HasForeignKey(c => c.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // CaseType configuration
        builder.Entity<CaseType>()
            .HasIndex(ct => ct.TenantId);

        builder.Entity<CaseType>()
            .HasIndex(ct => ct.IsActive);

        builder.Entity<CaseType>()
            .HasIndex(ct => new { ct.TenantId, ct.Name });

        builder.Entity<CaseType>()
            .HasOne(ct => ct.Tenant)
            .WithMany()
            .HasForeignKey(ct => ct.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // CaseStatus configuration
        builder.Entity<CaseStatus>()
            .HasIndex(cs => cs.TenantId);

        builder.Entity<CaseStatus>()
            .HasIndex(cs => cs.IsActive);

        builder.Entity<CaseStatus>()
            .HasIndex(cs => cs.Category);

        builder.Entity<CaseStatus>()
            .HasIndex(cs => new { cs.TenantId, cs.Name });

        builder.Entity<CaseStatus>()
            .HasOne(cs => cs.Tenant)
            .WithMany()
            .HasForeignKey(cs => cs.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // CaseNote configuration
        builder.Entity<CaseNote>()
            .HasIndex(cn => cn.CaseId);

        builder.Entity<CaseNote>()
            .HasIndex(cn => cn.CreatedAt);

        builder.Entity<CaseNote>()
            .HasIndex(cn => cn.IsPinned);

        builder.Entity<CaseNote>()
            .HasOne(cn => cn.Case)
            .WithMany(c => c.Notes)
            .HasForeignKey(cn => cn.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // CaseActivity configuration
        builder.Entity<CaseActivity>()
            .HasIndex(ca => ca.CaseId);

        builder.Entity<CaseActivity>()
            .HasIndex(ca => ca.Status);

        builder.Entity<CaseActivity>()
            .HasIndex(ca => ca.AssignedToUserId);

        builder.Entity<CaseActivity>()
            .HasIndex(ca => ca.DueDate);

        builder.Entity<CaseActivity>()
            .HasIndex(ca => new { ca.CaseId, ca.Status });

        builder.Entity<CaseActivity>()
            .HasOne(ca => ca.Case)
            .WithMany(c => c.Activities)
            .HasForeignKey(ca => ca.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CaseActivity>()
            .HasOne(ca => ca.AssignedToUser)
            .WithMany()
            .HasForeignKey(ca => ca.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Audit Log Configuration
        // ========================================

        builder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.CorrelationId);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.UserId);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.EntityType);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.Action);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.TenantId);

        builder.Entity<AuditLog>()
            .HasIndex(a => a.CompanyId);

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.EntityType, a.EntityId });

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.TenantId, a.Timestamp });

        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.UserId, a.Timestamp });

        builder.Entity<AuditLog>()
            .Property(a => a.Action)
            .HasMaxLength(50)
            .IsRequired();

        builder.Entity<AuditLog>()
            .Property(a => a.EntityType)
            .HasMaxLength(100);

        builder.Entity<AuditLog>()
            .Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Entity<AuditLog>()
            .Property(a => a.CorrelationId)
            .HasMaxLength(50);

        builder.Entity<AuditLog>()
            .Property(a => a.UserId)
            .HasMaxLength(450);

        builder.Entity<AuditLog>()
            .Property(a => a.UserName)
            .HasMaxLength(256);

        builder.Entity<AuditLog>()
            .Property(a => a.UserEmail)
            .HasMaxLength(256);

        builder.Entity<AuditLog>()
            .Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.Entity<AuditLog>()
            .Property(a => a.HttpMethod)
            .HasMaxLength(10);

        builder.Entity<AuditLog>()
            .Property(a => a.RequestPath)
            .HasMaxLength(500);

        builder.Entity<AuditLog>()
            .Property(a => a.ModuleName)
            .HasMaxLength(100);

        // ========================================
        // Dental Clinic Configuration
        // ========================================

        builder.Entity<DentalVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<DentalVisit>()
            .HasIndex(v => v.BranchId);

        builder.Entity<DentalVisit>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DentalVisit>()
            .HasOne(v => v.Branch)
            .WithMany()
            .HasForeignKey(v => v.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DentalVisit>()
            .HasOne(v => v.TreatmentPlan)
            .WithMany(tp => tp.Visits)
            .HasForeignKey(v => v.DentalTreatmentPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ToothChart>()
            .HasIndex(tc => tc.PatientId);

        builder.Entity<ToothChart>()
            .HasOne(tc => tc.Patient)
            .WithMany()
            .HasForeignKey(tc => tc.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ToothRecord>()
            .HasOne(tr => tr.ToothChart)
            .WithMany(tc => tc.TeethRecords)
            .HasForeignKey(tr => tr.ToothChartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DentalProcedure>()
            .HasOne(p => p.DentalVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.DentalVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DentalProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<DentalTreatmentPlan>()
            .HasIndex(tp => tp.PatientId);

        builder.Entity<DentalTreatmentPlan>()
            .HasOne(tp => tp.Patient)
            .WithMany()
            .HasForeignKey(tp => tp.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DentalTreatmentPlan>()
            .Property(tp => tp.EstimatedTotalCost)
            .HasPrecision(18, 2);

        builder.Entity<DentalTreatmentPlan>()
            .Property(tp => tp.ActualTotalCost)
            .HasPrecision(18, 2);

        builder.Entity<DentalTreatmentPlanItem>()
            .HasOne(i => i.TreatmentPlan)
            .WithMany(tp => tp.Items)
            .HasForeignKey(i => i.DentalTreatmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DentalTreatmentPlanItem>()
            .Property(i => i.EstimatedFee)
            .HasPrecision(18, 2);

        builder.Entity<DentalTreatmentPlanItem>()
            .Property(i => i.ActualFee)
            .HasPrecision(18, 2);

        builder.Entity<DentalXRay>()
            .HasIndex(x => x.PatientId);

        builder.Entity<DentalXRay>()
            .HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DentalXRay>()
            .HasOne(x => x.DentalVisit)
            .WithMany()
            .HasForeignKey(x => x.DentalVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Veterinary Clinic Configuration
        // ========================================

        builder.Entity<PetOwner>()
            .HasIndex(po => po.PhoneNumber);

        builder.Entity<PetOwner>()
            .HasOne(po => po.Branch)
            .WithMany()
            .HasForeignKey(po => po.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pet>()
            .HasIndex(p => p.MicrochipId);

        builder.Entity<Pet>()
            .HasOne(p => p.Owner)
            .WithMany(po => po.Pets)
            .HasForeignKey(p => p.PetOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pet>()
            .HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pet>()
            .Property(p => p.Weight)
            .HasPrecision(8, 2);

        builder.Entity<VetVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<VetVisit>()
            .HasOne(v => v.Pet)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VetVisit>()
            .Property(v => v.Weight)
            .HasPrecision(8, 2);

        builder.Entity<VetVisit>()
            .Property(v => v.Temperature)
            .HasPrecision(4, 1);

        builder.Entity<VetProcedure>()
            .HasOne(p => p.VetVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.VetVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VetProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<Vaccination>()
            .HasIndex(v => v.PetId);

        builder.Entity<Vaccination>()
            .HasOne(v => v.Pet)
            .WithMany(p => p.Vaccinations)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroomingAppointment>()
            .HasIndex(g => g.AppointmentDate);

        builder.Entity<GroomingAppointment>()
            .HasOne(g => g.Pet)
            .WithMany(p => p.GroomingAppointments)
            .HasForeignKey(g => g.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroomingAppointment>()
            .Property(g => g.TotalFee)
            .HasPrecision(18, 2);

        builder.Entity<BoardingReservation>()
            .HasIndex(b => b.CheckInDate);

        builder.Entity<BoardingReservation>()
            .HasOne(b => b.Pet)
            .WithMany(p => p.BoardingReservations)
            .HasForeignKey(b => b.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BoardingReservation>()
            .Property(b => b.DailyRate)
            .HasPrecision(18, 2);

        builder.Entity<BoardingReservation>()
            .Property(b => b.TotalFee)
            .HasPrecision(18, 2);

        // ========================================
        // Ophthalmology Clinic Configuration
        // ========================================

        builder.Entity<OphthalmologyVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<OphthalmologyVisit>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OphthalmologyVisit>()
            .Property(v => v.IopOd)
            .HasPrecision(4, 1);

        builder.Entity<OphthalmologyVisit>()
            .Property(v => v.IopOs)
            .HasPrecision(4, 1);

        builder.Entity<VisionTest>()
            .HasOne(vt => vt.Visit)
            .WithOne(v => v.VisionTest)
            .HasForeignKey<VisionTest>(vt => vt.OphthalmologyVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VisionTest>()
            .Property(vt => vt.SphereOd)
            .HasPrecision(5, 2);

        builder.Entity<VisionTest>()
            .Property(vt => vt.SphereOs)
            .HasPrecision(5, 2);

        builder.Entity<EyeExam>()
            .HasOne(e => e.Visit)
            .WithOne(v => v.EyeExam)
            .HasForeignKey<EyeExam>(e => e.OphthalmologyVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EyeExam>()
            .Property(e => e.CupToDiscRatioOd)
            .HasPrecision(3, 2);

        builder.Entity<EyeExam>()
            .Property(e => e.CupToDiscRatioOs)
            .HasPrecision(3, 2);

        builder.Entity<EyePrescription>()
            .HasIndex(p => p.PatientId);

        builder.Entity<EyePrescription>()
            .HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EyePrescription>()
            .Property(p => p.SphereOd)
            .HasPrecision(5, 2);

        builder.Entity<EyePrescription>()
            .Property(p => p.SphereOs)
            .HasPrecision(5, 2);

        builder.Entity<EyeProcedure>()
            .HasOne(p => p.Visit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.OphthalmologyVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EyeProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<EyeCondition>()
            .HasIndex(c => c.PatientId);

        builder.Entity<EyeCondition>()
            .HasOne(c => c.Patient)
            .WithMany()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Dermatology Clinic Configuration
        // ========================================

        builder.Entity<DermatologyVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<DermatologyVisit>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SkinCondition>()
            .HasIndex(c => c.PatientId);

        builder.Entity<SkinCondition>()
            .HasOne(c => c.Patient)
            .WithMany()
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LesionRecord>()
            .HasIndex(l => l.LesionCode)
            .IsUnique();

        builder.Entity<LesionRecord>()
            .HasIndex(l => l.PatientId);

        builder.Entity<LesionRecord>()
            .HasOne(l => l.Patient)
            .WithMany()
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LesionRecord>()
            .HasOne(l => l.Visit)
            .WithMany(v => v.Lesions)
            .HasForeignKey(l => l.DermatologyVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SkinProcedure>()
            .HasOne(p => p.Visit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.DermatologyVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SkinProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<SkinTreatmentPlan>()
            .HasIndex(tp => tp.PatientId);

        builder.Entity<SkinTreatmentPlan>()
            .HasOne(tp => tp.Patient)
            .WithMany()
            .HasForeignKey(tp => tp.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SkinTreatmentPlan>()
            .HasOne(tp => tp.SkinCondition)
            .WithMany()
            .HasForeignKey(tp => tp.SkinConditionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SkinTreatmentPlan>()
            .Property(tp => tp.EstimatedCost)
            .HasPrecision(18, 2);

        builder.Entity<SkinTreatmentPlan>()
            .Property(tp => tp.ActualCost)
            .HasPrecision(18, 2);

        // ========================================
        // Physiotherapy Clinic Configuration
        // ========================================

        builder.Entity<PhysioAssessment>()
            .HasIndex(a => a.AssessmentDate);

        builder.Entity<PhysioAssessment>()
            .HasIndex(a => a.PatientId);

        builder.Entity<PhysioAssessment>()
            .HasOne(a => a.Patient)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PhysioSession>()
            .HasIndex(s => s.SessionDate);

        builder.Entity<PhysioSession>()
            .HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PhysioSession>()
            .HasOne(s => s.ExerciseProgram)
            .WithMany(ep => ep.Sessions)
            .HasForeignKey(s => s.ExerciseProgramId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ExerciseProgram>()
            .HasIndex(ep => ep.PatientId);

        builder.Entity<ExerciseProgram>()
            .HasOne(ep => ep.Patient)
            .WithMany()
            .HasForeignKey(ep => ep.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExerciseProgram>()
            .HasOne(ep => ep.Assessment)
            .WithMany()
            .HasForeignKey(ep => ep.PhysioAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ExerciseProgramItem>()
            .HasOne(i => i.Program)
            .WithMany(ep => ep.Exercises)
            .HasForeignKey(i => i.ExerciseProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RangeOfMotionRecord>()
            .HasIndex(r => r.PatientId);

        builder.Entity<RangeOfMotionRecord>()
            .HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RangeOfMotionRecord>()
            .HasOne(r => r.Assessment)
            .WithMany(a => a.RangeOfMotionRecords)
            .HasForeignKey(r => r.PhysioAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RangeOfMotionRecord>()
            .Property(r => r.ActiveRom)
            .HasPrecision(5, 1);

        builder.Entity<RangeOfMotionRecord>()
            .Property(r => r.PassiveRom)
            .HasPrecision(5, 1);

        builder.Entity<FunctionalOutcome>()
            .HasIndex(f => f.PatientId);

        builder.Entity<FunctionalOutcome>()
            .HasOne(f => f.Patient)
            .WithMany()
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FunctionalOutcome>()
            .Property(f => f.Score)
            .HasPrecision(8, 2);

        // ========================================
        // Pediatrics Clinic Configuration
        // ========================================

        builder.Entity<PediatricVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<PediatricVisit>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PediatricVisit>()
            .Property(v => v.Weight)
            .HasPrecision(6, 2);

        builder.Entity<PediatricVisit>()
            .Property(v => v.Height)
            .HasPrecision(5, 1);

        builder.Entity<PediatricVisit>()
            .Property(v => v.HeadCircumference)
            .HasPrecision(5, 1);

        builder.Entity<GrowthRecord>()
            .HasIndex(g => g.PatientId);

        builder.Entity<GrowthRecord>()
            .HasOne(g => g.Patient)
            .WithMany()
            .HasForeignKey(g => g.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GrowthRecord>()
            .HasOne(g => g.Visit)
            .WithMany()
            .HasForeignKey(g => g.PediatricVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GrowthRecord>()
            .Property(g => g.Weight)
            .HasPrecision(6, 2);

        builder.Entity<GrowthRecord>()
            .Property(g => g.Height)
            .HasPrecision(5, 1);

        builder.Entity<VaccinationRecord>()
            .HasIndex(v => v.PatientId);

        builder.Entity<VaccinationRecord>()
            .HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VaccinationRecord>()
            .HasOne(v => v.Visit)
            .WithMany()
            .HasForeignKey(v => v.PediatricVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DevelopmentalMilestone>()
            .HasIndex(m => m.PatientId);

        builder.Entity<DevelopmentalMilestone>()
            .HasOne(m => m.Patient)
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DevelopmentalMilestone>()
            .HasOne(m => m.Visit)
            .WithMany()
            .HasForeignKey(m => m.PediatricVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DevelopmentalScreening>()
            .HasIndex(s => s.PatientId);

        builder.Entity<DevelopmentalScreening>()
            .HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DevelopmentalScreening>()
            .HasOne(s => s.Visit)
            .WithMany()
            .HasForeignKey(s => s.PediatricVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<NewbornRecord>()
            .HasIndex(n => n.PatientId)
            .IsUnique();

        builder.Entity<NewbornRecord>()
            .HasOne(n => n.Patient)
            .WithMany()
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NewbornRecord>()
            .Property(n => n.BirthWeight)
            .HasPrecision(5, 3);

        builder.Entity<NewbornRecord>()
            .Property(n => n.BirthLength)
            .HasPrecision(4, 1);
    }
}
