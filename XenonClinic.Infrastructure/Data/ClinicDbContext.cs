using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Entities.Cardiology;
using XenonClinic.Core.Entities.Chiropractic;
using XenonClinic.Core.Entities.Dental;
using XenonClinic.Core.Entities.Dermatology;
using XenonClinic.Core.Entities.Dialysis;
using XenonClinic.Core.Entities.ENT;
using XenonClinic.Core.Entities.Fertility;
using XenonClinic.Core.Entities.Gastroenterology;
using XenonClinic.Core.Entities.Gynecology;
using XenonClinic.Core.Entities.Lookups;
using XenonClinic.Core.Entities.Neurology;
using XenonClinic.Core.Entities.Oncology;
using XenonClinic.Core.Entities.Ophthalmology;
using XenonClinic.Core.Entities.Orthopedics;
using XenonClinic.Core.Entities.PainManagement;
using XenonClinic.Core.Entities.Pediatrics;
using XenonClinic.Core.Entities.Physiotherapy;
using XenonClinic.Core.Entities.Podiatry;
using XenonClinic.Core.Entities.Psychiatry;
using XenonClinic.Core.Entities.SleepMedicine;
using XenonClinic.Core.Entities.Veterinary;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Entities;
using XenonClinic.Infrastructure.Services;

namespace XenonClinic.Infrastructure.Data;

public class ClinicDbContext : IdentityDbContext<Entities.ApplicationUser>
{
    private readonly ITenantContextAccessor? _tenantContextAccessor;

    /// <summary>
    /// Constructor for runtime use with tenant context.
    /// </summary>
    public ClinicDbContext(
        DbContextOptions<ClinicDbContext> options,
        ITenantContextAccessor tenantContextAccessor) : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    /// <summary>
    /// Constructor for design-time and testing without tenant context.
    /// When no tenant context is provided, global filters are disabled.
    /// </summary>
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
    {
        _tenantContextAccessor = null;
    }

    /// <summary>
    /// Gets the current tenant ID from the tenant context accessor.
    /// Returns null if no tenant context is available (super admin or design-time).
    /// </summary>
    private int? CurrentTenantId => _tenantContextAccessor?.TenantId;

    /// <summary>
    /// Determines if tenant filtering should be applied.
    /// Returns false for super admins or when no tenant context is available.
    /// </summary>
    private bool ShouldFilterByTenant => _tenantContextAccessor?.ShouldFilterByTenant ?? false;

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
    public DbSet<Entities.UserMfaConfiguration> UserMfaConfigurations => Set<Entities.UserMfaConfiguration>();

    // Case Management entities
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseType> CaseTypes => Set<CaseType>();
    public DbSet<CaseStatus> CaseStatuses => Set<CaseStatus>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<CaseActivity> CaseActivities => Set<CaseActivity>();

    // Audit Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // OAuth Linked Accounts
    public DbSet<OAuthLinkedAccount> OAuthLinkedAccounts => Set<OAuthLinkedAccount>();

    // ========================================
    // Payment Gateway Entities
    // ========================================
    public DbSet<PaymentGatewayConfig> PaymentGatewayConfigs => Set<PaymentGatewayConfig>();
    public DbSet<PaymentGatewayTransaction> PaymentGatewayTransactions => Set<PaymentGatewayTransaction>();

    // ========================================
    // Insurance Entities
    // ========================================
    public DbSet<InsuranceProvider> InsuranceProviders => Set<InsuranceProvider>();
    public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
    public DbSet<PatientInsurance> PatientInsurances => Set<PatientInsurance>();
    public DbSet<InsuranceClaim> InsuranceClaims => Set<InsuranceClaim>();
    public DbSet<InsuranceClaimItem> InsuranceClaimItems => Set<InsuranceClaimItem>();
    public DbSet<InsurancePreAuthorization> InsurancePreAuthorizations => Set<InsurancePreAuthorization>();

    // ========================================
    // Medical Coding Entities
    // ========================================
    public DbSet<ICD10Code> ICD10Codes => Set<ICD10Code>();
    public DbSet<CPTCode> CPTCodes => Set<CPTCode>();
    public DbSet<HCPCSCode> HCPCSCodes => Set<HCPCSCode>();
    public DbSet<MedicalCodeModifier> MedicalCodeModifiers => Set<MedicalCodeModifier>();

    // ========================================
    // Payroll Entities
    // ========================================
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<WpsSubmission> WpsSubmissions => Set<WpsSubmission>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();

    // ========================================
    // DICOM/PACS Entities
    // ========================================
    public DbSet<DicomStudy> DicomStudies => Set<DicomStudy>();
    public DbSet<DicomSeries> DicomSeries => Set<DicomSeries>();
    public DbSet<DicomInstance> DicomInstances => Set<DicomInstance>();
    public DbSet<RadiologyReport> RadiologyReports => Set<RadiologyReport>();
    public DbSet<PacsServerConfig> PacsServerConfigs => Set<PacsServerConfig>();
    public DbSet<DicomWorklistEntry> DicomWorklistEntries => Set<DicomWorklistEntry>();

    // ========================================
    // Patient Portal Entities
    // ========================================
    public DbSet<PortalAccount> PortalAccounts => Set<PortalAccount>();
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageAttachment> MessageAttachments => Set<MessageAttachment>();
    public DbSet<RefillRequest> RefillRequests => Set<RefillRequest>();
    public DbSet<PatientNotification> PatientNotifications => Set<PatientNotification>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    // ========================================
    // Push Notification Entities
    // ========================================
    public DbSet<PushDevice> PushDevices => Set<PushDevice>();
    public DbSet<PushNotificationTopic> PushNotificationTopics => Set<PushNotificationTopic>();
    public DbSet<TopicSubscription> TopicSubscriptions => Set<TopicSubscription>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationTemplateLocalization> NotificationTemplateLocalizations => Set<NotificationTemplateLocalization>();
    public DbSet<ScheduledPushNotification> ScheduledPushNotifications => Set<ScheduledPushNotification>();
    public DbSet<PushNotificationHistoryEntry> PushNotificationHistory => Set<PushNotificationHistoryEntry>();
    public DbSet<PushConfiguration> PushConfigurations => Set<PushConfiguration>();

    // ========================================
    // Custom Report Builder Entities
    // ========================================
    public DbSet<CustomReportDefinition> CustomReportDefinitions => Set<CustomReportDefinition>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();
    public DbSet<ReportExecutionHistoryEntry> ReportExecutionHistory => Set<ReportExecutionHistoryEntry>();
    public DbSet<ReportWidget> ReportWidgets => Set<ReportWidget>();
    public DbSet<ReportPermission> ReportPermissions => Set<ReportPermission>();

    // ========================================
    // Calendar Sync Entities
    // ========================================
    public DbSet<CalendarConnection> CalendarConnections => Set<CalendarConnection>();
    public DbSet<CalendarSyncSettings> CalendarSyncSettings => Set<CalendarSyncSettings>();
    public DbSet<AppointmentCalendarMapping> AppointmentCalendarMappings => Set<AppointmentCalendarMapping>();
    public DbSet<CalendarSyncHistoryEntry> CalendarSyncHistory => Set<CalendarSyncHistoryEntry>();
    public DbSet<CalendarSyncConflict> CalendarSyncConflicts => Set<CalendarSyncConflict>();
    public DbSet<OAuthConfig> OAuthConfigs => Set<OAuthConfig>();
    public DbSet<OAuthState> OAuthStates => Set<OAuthState>();

    // ========================================
    // Drug Database Entities
    // ========================================
    public DbSet<FormularyDrug> FormularyDrugs => Set<FormularyDrug>();
    public DbSet<DrugPricing> DrugPricings => Set<DrugPricing>();
    public DbSet<PatientAssistanceProgram> PatientAssistancePrograms => Set<PatientAssistanceProgram>();
    public DbSet<ControlledSubstanceInfo> ControlledSubstanceInfos => Set<ControlledSubstanceInfo>();
    public DbSet<DrugAdverseEvent> DrugAdverseEvents => Set<DrugAdverseEvent>();
    public DbSet<DrugDatabaseUpdate> DrugDatabaseUpdates => Set<DrugDatabaseUpdate>();

    // ========================================
    // Advanced Analytics & BI Dashboard Entities
    // ========================================
    public DbSet<AnalyticsDashboard> AnalyticsDashboards => Set<AnalyticsDashboard>();
    public DbSet<AnalyticsDashboardWidget> AnalyticsDashboardWidgets => Set<AnalyticsDashboardWidget>();
    public DbSet<AnalyticsAlert> AnalyticsAlerts => Set<AnalyticsAlert>();
    public DbSet<AnalyticsAlertRule> AnalyticsAlertRules => Set<AnalyticsAlertRule>();
    public DbSet<AnalyticsAnomaly> AnalyticsAnomalies => Set<AnalyticsAnomaly>();
    public DbSet<DashboardShare> DashboardShares => Set<DashboardShare>();
    public DbSet<DashboardShareLink> DashboardShareLinks => Set<DashboardShareLink>();
    public DbSet<AnalyticsDashboardSubscription> AnalyticsDashboardSubscriptions => Set<AnalyticsDashboardSubscription>();

    // ========================================
    // Security & Compliance Entities
    // ========================================
    // Audit Trail
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditRetentionPolicy> AuditRetentionPolicies => Set<AuditRetentionPolicy>();
    public DbSet<AuditAlertConfig> AuditAlertConfigs => Set<AuditAlertConfig>();

    // RBAC
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<DataAccessRule> DataAccessRules => Set<DataAccessRule>();

    // Patient Consent
    public DbSet<PatientConsent> PatientConsents => Set<PatientConsent>();
    public DbSet<ConsentHistory> ConsentHistory => Set<ConsentHistory>();
    public DbSet<ConsentFormTemplate> ConsentFormTemplates => Set<ConsentFormTemplate>();

    // Security Configuration
    public DbSet<SecuritySettingsEntity> SecuritySettings => Set<SecuritySettingsEntity>();
    public DbSet<SecretEntity> Secrets => Set<SecretEntity>();
    public DbSet<ApiKeyEntity> ApiKeys => Set<ApiKeyEntity>();
    public DbSet<PasswordHistoryEntity> PasswordHistory => Set<PasswordHistoryEntity>();

    // Backup & Recovery
    public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();

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

    // ========================================
    // Cardiology Clinic Entities
    // ========================================
    public DbSet<CardioVisit> CardioVisits => Set<CardioVisit>();
    public DbSet<ECGRecord> ECGRecords => Set<ECGRecord>();
    public DbSet<EchoResult> EchoResults => Set<EchoResult>();
    public DbSet<StressTest> StressTests => Set<StressTest>();
    public DbSet<CardiacProcedure> CardiacProcedures => Set<CardiacProcedure>();
    public DbSet<HeartCondition> HeartConditions => Set<HeartCondition>();

    // ========================================
    // Orthopedics Clinic Entities
    // ========================================
    public DbSet<OrthoVisit> OrthoVisits => Set<OrthoVisit>();
    public DbSet<OrthoInjury> OrthoInjuries => Set<OrthoInjury>();
    public DbSet<OrthoProcedure> OrthoProcedures => Set<OrthoProcedure>();
    public DbSet<JointAssessment> JointAssessments => Set<JointAssessment>();
    public DbSet<CastRecord> CastRecords => Set<CastRecord>();
    public DbSet<OrthoImaging> OrthoImagings => Set<OrthoImaging>();

    // ========================================
    // ENT Clinic Entities
    // ========================================
    public DbSet<ENTVisit> ENTVisits => Set<ENTVisit>();
    public DbSet<HearingScreening> HearingScreenings => Set<HearingScreening>();
    public DbSet<SinusAssessment> SinusAssessments => Set<SinusAssessment>();
    public DbSet<ThroatExam> ThroatExams => Set<ThroatExam>();
    public DbSet<ENTProcedure> ENTProcedures => Set<ENTProcedure>();
    public DbSet<AllergyTest> AllergyTests => Set<AllergyTest>();

    // ========================================
    // Gynecology/Obstetrics Clinic Entities
    // ========================================
    public DbSet<GynVisit> GynVisits => Set<GynVisit>();
    public DbSet<PregnancyRecord> PregnancyRecords => Set<PregnancyRecord>();
    public DbSet<PrenatalVisit> PrenatalVisits => Set<PrenatalVisit>();
    public DbSet<ObUltrasound> ObUltrasounds => Set<ObUltrasound>();
    public DbSet<GynProcedure> GynProcedures => Set<GynProcedure>();
    public DbSet<PapSmearRecord> PapSmearRecords => Set<PapSmearRecord>();

    // ========================================
    // Psychiatry/Psychology Clinic Entities
    // ========================================
    public DbSet<MentalHealthVisit> MentalHealthVisits => Set<MentalHealthVisit>();
    public DbSet<PsychAssessment> PsychAssessments => Set<PsychAssessment>();
    public DbSet<TherapySession> TherapySessions => Set<TherapySession>();
    public DbSet<PsychMedicationPlan> PsychMedicationPlans => Set<PsychMedicationPlan>();
    public DbSet<MoodRecord> MoodRecords => Set<MoodRecord>();
    public DbSet<TreatmentGoal> TreatmentGoals => Set<TreatmentGoal>();

    // ========================================
    // Gastroenterology Clinic Entities
    // ========================================
    public DbSet<GastroVisit> GastroVisits => Set<GastroVisit>();
    public DbSet<EndoscopyRecord> EndoscopyRecords => Set<EndoscopyRecord>();
    public DbSet<LiverFunctionTest> LiverFunctionTests => Set<LiverFunctionTest>();
    public DbSet<GastroProcedure> GastroProcedures => Set<GastroProcedure>();
    public DbSet<DigestiveCondition> DigestiveConditions => Set<DigestiveCondition>();

    // ========================================
    // Neurology Clinic Entities
    // ========================================
    public DbSet<NeuroVisit> NeuroVisits => Set<NeuroVisit>();
    public DbSet<NeuroExam> NeuroExams => Set<NeuroExam>();
    public DbSet<EEGRecord> EEGRecords => Set<EEGRecord>();
    public DbSet<NerveStudy> NerveStudies => Set<NerveStudy>();
    public DbSet<NeuroProcedure> NeuroProcedures => Set<NeuroProcedure>();
    public DbSet<NeuroCondition> NeuroConditions => Set<NeuroCondition>();

    // ========================================
    // Fertility/IVF Clinic Entities
    // ========================================
    public DbSet<FertilityVisit> FertilityVisits => Set<FertilityVisit>();
    public DbSet<FertilityAssessment> FertilityAssessments => Set<FertilityAssessment>();
    public DbSet<IVFCycle> IVFCycles => Set<IVFCycle>();
    public DbSet<EmbryoRecord> EmbryoRecords => Set<EmbryoRecord>();
    public DbSet<HormoneLevel> HormoneLevels => Set<HormoneLevel>();
    public DbSet<SpermAnalysis> SpermAnalyses => Set<SpermAnalysis>();

    // ========================================
    // Pain Management Clinic Entities
    // ========================================
    public DbSet<PainVisit> PainVisits => Set<PainVisit>();
    public DbSet<PainAssessment> PainAssessments => Set<PainAssessment>();
    public DbSet<PainProcedure> PainProcedures => Set<PainProcedure>();
    public DbSet<PainMedicationRegimen> PainMedicationRegimens => Set<PainMedicationRegimen>();
    public DbSet<PainScore> PainScores => Set<PainScore>();
    public DbSet<TriggerPointRecord> TriggerPointRecords => Set<TriggerPointRecord>();

    // ========================================
    // Sleep Medicine Clinic Entities
    // ========================================
    public DbSet<SleepVisit> SleepVisits => Set<SleepVisit>();
    public DbSet<SleepStudy> SleepStudies => Set<SleepStudy>();
    public DbSet<SleepDiary> SleepDiaries => Set<SleepDiary>();
    public DbSet<CPAPRecord> CPAPRecords => Set<CPAPRecord>();
    public DbSet<SleepDisorder> SleepDisorders => Set<SleepDisorder>();

    // ========================================
    // Dialysis Clinic Entities
    // ========================================
    public DbSet<DialysisPatientRecord> DialysisPatientRecords => Set<DialysisPatientRecord>();
    public DbSet<DialysisSession> DialysisSessions => Set<DialysisSession>();
    public DbSet<DialysisAccessRecord> DialysisAccessRecords => Set<DialysisAccessRecord>();
    public DbSet<DialysisLabResult> DialysisLabResults => Set<DialysisLabResult>();
    public DbSet<FluidBalance> FluidBalances => Set<FluidBalance>();

    // ========================================
    // Oncology Clinic Entities
    // ========================================
    public DbSet<OncologyVisit> OncologyVisits => Set<OncologyVisit>();
    public DbSet<CancerDiagnosis> CancerDiagnoses => Set<CancerDiagnosis>();
    public DbSet<ChemotherapySession> ChemotherapySessions => Set<ChemotherapySession>();
    public DbSet<RadiationRecord> RadiationRecords => Set<RadiationRecord>();
    public DbSet<TumorMarker> TumorMarkers => Set<TumorMarker>();
    public DbSet<OncologyTreatmentPlan> OncologyTreatmentPlans => Set<OncologyTreatmentPlan>();

    // ========================================
    // Chiropractic Clinic Entities
    // ========================================
    public DbSet<ChiroVisit> ChiroVisits => Set<ChiroVisit>();
    public DbSet<SpinalAssessment> SpinalAssessments => Set<SpinalAssessment>();
    public DbSet<ChiroAdjustment> ChiroAdjustments => Set<ChiroAdjustment>();
    public DbSet<PostureAnalysis> PostureAnalyses => Set<PostureAnalysis>();
    public DbSet<ChiroXRayFinding> ChiroXRayFindings => Set<ChiroXRayFinding>();
    public DbSet<ChiroTreatmentPlan> ChiroTreatmentPlans => Set<ChiroTreatmentPlan>();

    // ========================================
    // Podiatry Clinic Entities
    // ========================================
    public DbSet<PodiatryVisit> PodiatryVisits => Set<PodiatryVisit>();
    public DbSet<FootAssessment> FootAssessments => Set<FootAssessment>();
    public DbSet<PodiatryGaitAnalysis> PodiatryGaitAnalyses => Set<PodiatryGaitAnalysis>();
    public DbSet<PodiatryProcedure> PodiatryProcedures => Set<PodiatryProcedure>();
    public DbSet<OrthoticPrescription> OrthoticPrescriptions => Set<OrthoticPrescription>();
    public DbSet<FootCondition> FootConditions => Set<FootCondition>();

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

        // ========================================
        // Global Query Filters for Tenant Isolation
        // These filters automatically apply tenant filtering to all queries.
        // Super admins bypass these filters (ShouldFilterByTenant = false).
        // ========================================

        // Company: Filter by tenant
        builder.Entity<Company>().HasQueryFilter(c =>
            !ShouldFilterByTenant || c.TenantId == CurrentTenantId);

        // ApplicationUser: Filter by tenant (null TenantId = super admin, allowed)
        builder.Entity<Entities.ApplicationUser>().HasQueryFilter(u =>
            !ShouldFilterByTenant || u.TenantId == CurrentTenantId || u.TenantId == null);

        // TenantSettings: Filter by tenant
        builder.Entity<TenantSettings>().HasQueryFilter(ts =>
            !ShouldFilterByTenant || ts.TenantId == CurrentTenantId);

        // TenantFeature: Filter by tenant
        builder.Entity<TenantFeature>().HasQueryFilter(tf =>
            !ShouldFilterByTenant || tf.TenantId == CurrentTenantId);

        // TenantTerminology: Filter by tenant
        builder.Entity<TenantTerminology>().HasQueryFilter(tt =>
            !ShouldFilterByTenant || tt.TenantId == CurrentTenantId);

        // TenantUISchema: Filter by tenant
        builder.Entity<TenantUISchema>().HasQueryFilter(ts =>
            !ShouldFilterByTenant || ts.TenantId == CurrentTenantId);

        // TenantFormLayout: Filter by tenant
        builder.Entity<TenantFormLayout>().HasQueryFilter(tfl =>
            !ShouldFilterByTenant || tfl.TenantId == CurrentTenantId);

        // TenantListLayout: Filter by tenant
        builder.Entity<TenantListLayout>().HasQueryFilter(tll =>
            !ShouldFilterByTenant || tll.TenantId == CurrentTenantId);

        // ========================================
        // Branch-Level Query Filters (143+ entities)
        // Apply global filters to all entities with BranchId property.
        // This ensures automatic tenant isolation at the branch level.
        // Super admins and company admins bypass these filters.
        // ========================================
        builder.ApplyBranchQueryFiltersByConvention(_tenantContextAccessor);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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

        // Invoice configuration
        builder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceDate);

        builder.Entity<Invoice>()
            .HasIndex(i => i.BranchId);

        builder.Entity<Invoice>()
            .HasIndex(i => i.Status);

        builder.Entity<Invoice>()
            .HasIndex(i => new { i.BranchId, i.InvoiceDate });

        builder.Entity<Invoice>()
            .HasOne(i => i.Patient)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.Branch)
            .WithMany(b => b.Invoices)
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.Sale)
            .WithMany()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>()
            .Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Entity<Invoice>()
            .Property(i => i.PaidAmount)
            .HasPrecision(18, 2);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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

        // UserMfaConfiguration configuration (Infrastructure entity)
        builder.Entity<Entities.UserMfaConfiguration>()
            .HasIndex(m => m.UserId)
            .IsUnique();

        builder.Entity<Entities.UserMfaConfiguration>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Entities.UserMfaConfiguration>()
            .Property(m => m.UserId)
            .HasMaxLength(450)
            .IsRequired();

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
        // OAuth Linked Accounts Configuration
        // ========================================

        builder.Entity<OAuthLinkedAccount>()
            .HasIndex(o => new { o.UserId, o.Provider })
            .IsUnique();

        builder.Entity<OAuthLinkedAccount>()
            .HasIndex(o => new { o.Provider, o.ProviderUserId })
            .IsUnique();

        builder.Entity<OAuthLinkedAccount>()
            .Property(o => o.UserId)
            .HasMaxLength(450);

        builder.Entity<OAuthLinkedAccount>()
            .Property(o => o.Provider)
            .HasMaxLength(50);

        builder.Entity<OAuthLinkedAccount>()
            .Property(o => o.ProviderUserId)
            .HasMaxLength(500);

        builder.Entity<OAuthLinkedAccount>()
            .Property(o => o.ProviderEmail)
            .HasMaxLength(255);

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
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DentalProcedure>()
            .HasOne(p => p.DentalVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.DentalVisitId)
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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
            .OnDelete(DeleteBehavior.Restrict);

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

        // ========================================
        // Cardiology Clinic Configuration
        // ========================================

        builder.Entity<CardioVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<CardioVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<CardioVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.CardioVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ECGRecord>()
            .HasIndex(e => e.RecordDate);

        builder.Entity<ECGRecord>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.ECGRecords)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ECGRecord>()
            .HasOne(e => e.CardioVisit)
            .WithMany(v => v.ECGRecords)
            .HasForeignKey(e => e.CardioVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EchoResult>()
            .HasIndex(e => e.ExamDate);

        builder.Entity<EchoResult>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.EchoResults)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EchoResult>()
            .HasOne(e => e.CardioVisit)
            .WithMany(v => v.EchoResults)
            .HasForeignKey(e => e.CardioVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EchoResult>()
            .Property(e => e.EjectionFraction)
            .HasPrecision(5, 2);

        builder.Entity<StressTest>()
            .HasIndex(s => s.TestDate);

        builder.Entity<StressTest>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.StressTests)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StressTest>()
            .HasOne(s => s.CardioVisit)
            .WithMany(v => v.StressTests)
            .HasForeignKey(s => s.CardioVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CardiacProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<CardiacProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.CardiacProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CardiacProcedure>()
            .HasOne(p => p.CardioVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.CardioVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CardiacProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<HeartCondition>()
            .HasIndex(h => h.PatientId);

        builder.Entity<HeartCondition>()
            .HasOne(h => h.Patient)
            .WithMany(p => p.HeartConditions)
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Orthopedics Clinic Configuration
        // ========================================

        builder.Entity<OrthoVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<OrthoVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<OrthoVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.OrthoVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrthoInjury>()
            .HasIndex(i => i.PatientId);

        builder.Entity<OrthoInjury>()
            .HasOne(i => i.Patient)
            .WithMany(p => p.OrthoInjuries)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrthoInjury>()
            .HasOne(i => i.OrthoVisit)
            .WithMany(v => v.Injuries)
            .HasForeignKey(i => i.OrthoVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrthoProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<OrthoProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.OrthoProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrthoProcedure>()
            .HasOne(p => p.OrthoVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.OrthoVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrthoProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<JointAssessment>()
            .HasIndex(j => j.PatientId);

        builder.Entity<JointAssessment>()
            .HasOne(j => j.Patient)
            .WithMany(p => p.JointAssessments)
            .HasForeignKey(j => j.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<JointAssessment>()
            .HasOne(j => j.OrthoVisit)
            .WithMany(v => v.JointAssessments)
            .HasForeignKey(j => j.OrthoVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CastRecord>()
            .HasIndex(c => c.PatientId);

        builder.Entity<CastRecord>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.CastRecords)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CastRecord>()
            .HasOne(c => c.OrthoInjury)
            .WithMany(i => i.CastRecords)
            .HasForeignKey(c => c.OrthoInjuryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrthoImaging>()
            .HasIndex(o => o.ImagingDate);

        builder.Entity<OrthoImaging>()
            .HasOne(o => o.Patient)
            .WithMany(p => p.OrthoImagings)
            .HasForeignKey(o => o.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrthoImaging>()
            .HasOne(o => o.OrthoVisit)
            .WithMany(v => v.Imagings)
            .HasForeignKey(o => o.OrthoVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // ENT Clinic Configuration
        // ========================================

        builder.Entity<ENTVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<ENTVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<ENTVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.ENTVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HearingScreening>()
            .HasIndex(h => h.ScreeningDate);

        builder.Entity<HearingScreening>()
            .HasOne(h => h.Patient)
            .WithMany(p => p.HearingScreenings)
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HearingScreening>()
            .HasOne(h => h.ENTVisit)
            .WithMany(v => v.HearingScreenings)
            .HasForeignKey(h => h.ENTVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SinusAssessment>()
            .HasIndex(s => s.AssessmentDate);

        builder.Entity<SinusAssessment>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.SinusAssessments)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SinusAssessment>()
            .HasOne(s => s.ENTVisit)
            .WithMany(v => v.SinusAssessments)
            .HasForeignKey(s => s.ENTVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ThroatExam>()
            .HasIndex(t => t.ExamDate);

        builder.Entity<ThroatExam>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.ThroatExams)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ThroatExam>()
            .HasOne(t => t.ENTVisit)
            .WithMany(v => v.ThroatExams)
            .HasForeignKey(t => t.ENTVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ENTProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<ENTProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.ENTProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ENTProcedure>()
            .HasOne(p => p.ENTVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.ENTVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ENTProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<AllergyTest>()
            .HasIndex(a => a.TestDate);

        builder.Entity<AllergyTest>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.AllergyTests)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AllergyTest>()
            .HasOne(a => a.ENTVisit)
            .WithMany(v => v.AllergyTests)
            .HasForeignKey(a => a.ENTVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Gynecology/Obstetrics Configuration
        // ========================================

        builder.Entity<GynVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<GynVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<GynVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.GynVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PregnancyRecord>()
            .HasIndex(p => p.PatientId);

        builder.Entity<PregnancyRecord>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PregnancyRecords)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PrenatalVisit>()
            .HasIndex(p => p.VisitDate);

        builder.Entity<PrenatalVisit>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PrenatalVisits)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PrenatalVisit>()
            .HasOne(p => p.PregnancyRecord)
            .WithMany(pr => pr.PrenatalVisits)
            .HasForeignKey(p => p.PregnancyRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PrenatalVisit>()
            .Property(p => p.Weight)
            .HasPrecision(6, 2);

        builder.Entity<ObUltrasound>()
            .HasIndex(u => u.ExamDate);

        builder.Entity<ObUltrasound>()
            .HasOne(u => u.Patient)
            .WithMany(p => p.ObUltrasounds)
            .HasForeignKey(u => u.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ObUltrasound>()
            .HasOne(u => u.PregnancyRecord)
            .WithMany(pr => pr.Ultrasounds)
            .HasForeignKey(u => u.PregnancyRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ObUltrasound>()
            .Property(u => u.EstimatedFetalWeight)
            .HasPrecision(8, 2);

        builder.Entity<GynProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<GynProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.GynProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GynProcedure>()
            .HasOne(p => p.GynVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.GynVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GynProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<PapSmearRecord>()
            .HasIndex(p => p.TestDate);

        builder.Entity<PapSmearRecord>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PapSmearRecords)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PapSmearRecord>()
            .HasOne(p => p.GynVisit)
            .WithMany(v => v.PapSmearRecords)
            .HasForeignKey(p => p.GynVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Psychiatry/Psychology Configuration
        // ========================================

        builder.Entity<MentalHealthVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<MentalHealthVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<MentalHealthVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.MentalHealthVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PsychAssessment>()
            .HasIndex(a => a.AssessmentDate);

        builder.Entity<PsychAssessment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.PsychAssessments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PsychAssessment>()
            .HasOne(a => a.MentalHealthVisit)
            .WithMany(v => v.Assessments)
            .HasForeignKey(a => a.MentalHealthVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<TherapySession>()
            .HasIndex(t => t.SessionDate);

        builder.Entity<TherapySession>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.TherapySessions)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TherapySession>()
            .HasOne(t => t.MentalHealthVisit)
            .WithMany(v => v.TherapySessions)
            .HasForeignKey(t => t.MentalHealthVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<TherapySession>()
            .Property(t => t.Fee)
            .HasPrecision(18, 2);

        builder.Entity<PsychMedicationPlan>()
            .HasIndex(m => m.PatientId);

        builder.Entity<PsychMedicationPlan>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.PsychMedicationPlans)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MoodRecord>()
            .HasIndex(m => m.RecordDate);

        builder.Entity<MoodRecord>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.MoodRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreatmentGoal>()
            .HasIndex(t => t.PatientId);

        builder.Entity<TreatmentGoal>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.TreatmentGoals)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Gastroenterology Configuration
        // ========================================

        builder.Entity<GastroVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<GastroVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<GastroVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.GastroVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EndoscopyRecord>()
            .HasIndex(e => e.ProcedureDate);

        builder.Entity<EndoscopyRecord>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.EndoscopyRecords)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EndoscopyRecord>()
            .HasOne(e => e.GastroVisit)
            .WithMany(v => v.EndoscopyRecords)
            .HasForeignKey(e => e.GastroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<LiverFunctionTest>()
            .HasIndex(l => l.TestDate);

        builder.Entity<LiverFunctionTest>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.LiverFunctionTests)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LiverFunctionTest>()
            .HasOne(l => l.GastroVisit)
            .WithMany(v => v.LiverFunctionTests)
            .HasForeignKey(l => l.GastroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GastroProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<GastroProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.GastroProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GastroProcedure>()
            .HasOne(p => p.GastroVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.GastroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GastroProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<DigestiveCondition>()
            .HasIndex(d => d.PatientId);

        builder.Entity<DigestiveCondition>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.DigestiveConditions)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Neurology Configuration
        // ========================================

        builder.Entity<NeuroVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<NeuroVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<NeuroVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.NeuroVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NeuroExam>()
            .HasIndex(e => e.ExamDate);

        builder.Entity<NeuroExam>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.NeuroExams)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NeuroExam>()
            .HasOne(e => e.NeuroVisit)
            .WithMany(v => v.NeuroExams)
            .HasForeignKey(e => e.NeuroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EEGRecord>()
            .HasIndex(e => e.RecordDate);

        builder.Entity<EEGRecord>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.EEGRecords)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EEGRecord>()
            .HasOne(e => e.NeuroVisit)
            .WithMany(v => v.EEGRecords)
            .HasForeignKey(e => e.NeuroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<NerveStudy>()
            .HasIndex(n => n.StudyDate);

        builder.Entity<NerveStudy>()
            .HasOne(n => n.Patient)
            .WithMany(p => p.NerveStudies)
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NerveStudy>()
            .HasOne(n => n.NeuroVisit)
            .WithMany(v => v.NerveStudies)
            .HasForeignKey(n => n.NeuroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<NeuroProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<NeuroProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.NeuroProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<NeuroProcedure>()
            .HasOne(p => p.NeuroVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.NeuroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<NeuroProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<NeuroCondition>()
            .HasIndex(n => n.PatientId);

        builder.Entity<NeuroCondition>()
            .HasOne(n => n.Patient)
            .WithMany(p => p.NeuroConditions)
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Fertility/IVF Configuration
        // ========================================

        builder.Entity<FertilityVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<FertilityVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<FertilityVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.FertilityVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FertilityAssessment>()
            .HasIndex(a => a.AssessmentDate);

        builder.Entity<FertilityAssessment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.FertilityAssessments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FertilityAssessment>()
            .HasOne(a => a.FertilityVisit)
            .WithMany(v => v.Assessments)
            .HasForeignKey(a => a.FertilityVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<IVFCycle>()
            .HasIndex(i => i.CycleStartDate);

        builder.Entity<IVFCycle>()
            .HasOne(i => i.Patient)
            .WithMany(p => p.IVFCycles)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IVFCycle>()
            .Property(i => i.TotalCost)
            .HasPrecision(18, 2);

        builder.Entity<EmbryoRecord>()
            .HasIndex(e => e.IVFCycleId);

        builder.Entity<EmbryoRecord>()
            .HasOne(e => e.IVFCycle)
            .WithMany(i => i.Embryos)
            .HasForeignKey(e => e.IVFCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HormoneLevel>()
            .HasIndex(h => h.TestDate);

        builder.Entity<HormoneLevel>()
            .HasOne(h => h.Patient)
            .WithMany(p => p.HormoneLevels)
            .HasForeignKey(h => h.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HormoneLevel>()
            .Property(h => h.Value)
            .HasPrecision(10, 4);

        builder.Entity<SpermAnalysis>()
            .HasIndex(s => s.AnalysisDate);

        builder.Entity<SpermAnalysis>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.SpermAnalyses)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SpermAnalysis>()
            .Property(s => s.Volume)
            .HasPrecision(6, 2);

        builder.Entity<SpermAnalysis>()
            .Property(s => s.Concentration)
            .HasPrecision(10, 2);

        // ========================================
        // Pain Management Configuration
        // ========================================

        builder.Entity<PainVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<PainVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<PainVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.PainVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PainAssessment>()
            .HasIndex(a => a.AssessmentDate);

        builder.Entity<PainAssessment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.PainAssessments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PainAssessment>()
            .HasOne(a => a.PainVisit)
            .WithMany(v => v.Assessments)
            .HasForeignKey(a => a.PainVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PainProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<PainProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PainProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PainProcedure>()
            .HasOne(p => p.PainVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.PainVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PainProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<PainMedicationRegimen>()
            .HasIndex(m => m.PatientId);

        builder.Entity<PainMedicationRegimen>()
            .HasOne(m => m.Patient)
            .WithMany(p => p.PainMedicationRegimens)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PainScore>()
            .HasIndex(s => s.RecordDate);

        builder.Entity<PainScore>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.PainScores)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TriggerPointRecord>()
            .HasIndex(t => t.PatientId);

        builder.Entity<TriggerPointRecord>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.TriggerPointRecords)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TriggerPointRecord>()
            .HasOne(t => t.PainAssessment)
            .WithMany(a => a.TriggerPoints)
            .HasForeignKey(t => t.PainAssessmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // ========================================
        // Sleep Medicine Configuration
        // ========================================

        builder.Entity<SleepVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<SleepVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<SleepVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.SleepVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SleepStudy>()
            .HasIndex(s => s.StudyDate);

        builder.Entity<SleepStudy>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.SleepStudies)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SleepStudy>()
            .HasOne(s => s.SleepVisit)
            .WithMany(v => v.SleepStudies)
            .HasForeignKey(s => s.SleepVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<SleepStudy>()
            .Property(s => s.AHI)
            .HasPrecision(6, 2);

        builder.Entity<SleepDiary>()
            .HasIndex(d => d.EntryDate);

        builder.Entity<SleepDiary>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.SleepDiaries)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CPAPRecord>()
            .HasIndex(c => c.PatientId);

        builder.Entity<CPAPRecord>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.CPAPRecords)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CPAPRecord>()
            .Property(c => c.PressureSetting)
            .HasPrecision(4, 1);

        builder.Entity<SleepDisorder>()
            .HasIndex(d => d.PatientId);

        builder.Entity<SleepDisorder>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.SleepDisorders)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Dialysis Configuration
        // ========================================

        builder.Entity<DialysisPatientRecord>()
            .HasIndex(d => d.PatientId)
            .IsUnique();

        builder.Entity<DialysisPatientRecord>()
            .HasOne(d => d.Patient)
            .WithMany(p => p.DialysisPatientRecords)
            .HasForeignKey(d => d.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DialysisPatientRecord>()
            .Property(d => d.DryWeight)
            .HasPrecision(6, 2);

        builder.Entity<DialysisSession>()
            .HasIndex(s => s.SessionDate);

        builder.Entity<DialysisSession>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.DialysisSessions)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DialysisSession>()
            .HasOne(s => s.DialysisPatientRecord)
            .WithMany(r => r.Sessions)
            .HasForeignKey(s => s.DialysisPatientRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DialysisSession>()
            .Property(s => s.PreWeight)
            .HasPrecision(6, 2);

        builder.Entity<DialysisSession>()
            .Property(s => s.PostWeight)
            .HasPrecision(6, 2);

        builder.Entity<DialysisSession>()
            .Property(s => s.UltraFiltrationVolume)
            .HasPrecision(8, 2);

        builder.Entity<DialysisAccessRecord>()
            .HasIndex(a => a.PatientId);

        builder.Entity<DialysisAccessRecord>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.DialysisAccessRecords)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DialysisLabResult>()
            .HasIndex(l => l.TestDate);

        builder.Entity<DialysisLabResult>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.DialysisLabResults)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DialysisLabResult>()
            .HasOne(l => l.DialysisSession)
            .WithMany(s => s.LabResults)
            .HasForeignKey(l => l.DialysisSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DialysisLabResult>()
            .Property(l => l.Value)
            .HasPrecision(10, 4);

        builder.Entity<FluidBalance>()
            .HasIndex(f => f.RecordDate);

        builder.Entity<FluidBalance>()
            .HasOne(f => f.Patient)
            .WithMany(p => p.FluidBalances)
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FluidBalance>()
            .Property(f => f.FluidIntake)
            .HasPrecision(8, 2);

        builder.Entity<FluidBalance>()
            .Property(f => f.FluidOutput)
            .HasPrecision(8, 2);

        builder.Entity<FluidBalance>()
            .Property(f => f.NetBalance)
            .HasPrecision(8, 2);

        // ========================================
        // Oncology Configuration
        // ========================================

        builder.Entity<OncologyVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<OncologyVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<OncologyVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.OncologyVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CancerDiagnosis>()
            .HasIndex(c => c.PatientId);

        builder.Entity<CancerDiagnosis>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.CancerDiagnoses)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChemotherapySession>()
            .HasIndex(c => c.SessionDate);

        builder.Entity<ChemotherapySession>()
            .HasOne(c => c.Patient)
            .WithMany(p => p.ChemotherapySessions)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChemotherapySession>()
            .HasOne(c => c.OncologyVisit)
            .WithMany(v => v.ChemotherapySessions)
            .HasForeignKey(c => c.OncologyVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChemotherapySession>()
            .HasOne(c => c.TreatmentPlan)
            .WithMany(t => t.ChemotherapySessions)
            .HasForeignKey(c => c.OncologyTreatmentPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RadiationRecord>()
            .HasIndex(r => r.SessionDate);

        builder.Entity<RadiationRecord>()
            .HasOne(r => r.Patient)
            .WithMany(p => p.RadiationRecords)
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RadiationRecord>()
            .HasOne(r => r.OncologyVisit)
            .WithMany(v => v.RadiationRecords)
            .HasForeignKey(r => r.OncologyVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RadiationRecord>()
            .HasOne(r => r.TreatmentPlan)
            .WithMany(t => t.RadiationRecords)
            .HasForeignKey(r => r.OncologyTreatmentPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<RadiationRecord>()
            .Property(r => r.DoseDelivered)
            .HasPrecision(8, 3);

        builder.Entity<TumorMarker>()
            .HasIndex(t => t.TestDate);

        builder.Entity<TumorMarker>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.TumorMarkers)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TumorMarker>()
            .Property(t => t.Value)
            .HasPrecision(12, 4);

        builder.Entity<OncologyTreatmentPlan>()
            .HasIndex(o => o.PatientId);

        builder.Entity<OncologyTreatmentPlan>()
            .HasOne(o => o.Patient)
            .WithMany(p => p.OncologyTreatmentPlans)
            .HasForeignKey(o => o.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OncologyTreatmentPlan>()
            .HasOne(o => o.CancerDiagnosis)
            .WithMany(c => c.TreatmentPlans)
            .HasForeignKey(o => o.CancerDiagnosisId)
            .OnDelete(DeleteBehavior.Restrict);

        // ========================================
        // Chiropractic Configuration
        // ========================================

        builder.Entity<ChiroVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<ChiroVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<ChiroVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.ChiroVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SpinalAssessment>()
            .HasIndex(s => s.AssessmentDate);

        builder.Entity<SpinalAssessment>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.SpinalAssessments)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SpinalAssessment>()
            .HasOne(s => s.ChiroVisit)
            .WithMany(v => v.SpinalAssessments)
            .HasForeignKey(s => s.ChiroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChiroAdjustment>()
            .HasIndex(a => a.AdjustmentDate);

        builder.Entity<ChiroAdjustment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.ChiroAdjustments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChiroAdjustment>()
            .HasOne(a => a.ChiroVisit)
            .WithMany(v => v.Adjustments)
            .HasForeignKey(a => a.ChiroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChiroAdjustment>()
            .Property(a => a.Fee)
            .HasPrecision(18, 2);

        builder.Entity<PostureAnalysis>()
            .HasIndex(p => p.AnalysisDate);

        builder.Entity<PostureAnalysis>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PostureAnalyses)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PostureAnalysis>()
            .HasOne(p => p.ChiroVisit)
            .WithMany(v => v.PostureAnalyses)
            .HasForeignKey(p => p.ChiroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChiroXRayFinding>()
            .HasIndex(x => x.XRayDate);

        builder.Entity<ChiroXRayFinding>()
            .HasOne(x => x.Patient)
            .WithMany(p => p.ChiroXRayFindings)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChiroXRayFinding>()
            .HasOne(x => x.ChiroVisit)
            .WithMany(v => v.XRayFindings)
            .HasForeignKey(x => x.ChiroVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChiroTreatmentPlan>()
            .HasIndex(t => t.PatientId);

        builder.Entity<ChiroTreatmentPlan>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.ChiroTreatmentPlans)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChiroTreatmentPlan>()
            .Property(t => t.EstimatedCost)
            .HasPrecision(18, 2);

        // ========================================
        // Podiatry Configuration
        // ========================================

        builder.Entity<PodiatryVisit>()
            .HasIndex(v => v.VisitDate);

        builder.Entity<PodiatryVisit>()
            .HasIndex(v => v.PatientId);

        builder.Entity<PodiatryVisit>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.PodiatryVisits)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FootAssessment>()
            .HasIndex(f => f.AssessmentDate);

        builder.Entity<FootAssessment>()
            .HasOne(f => f.Patient)
            .WithMany(p => p.FootAssessments)
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FootAssessment>()
            .HasOne(f => f.PodiatryVisit)
            .WithMany(v => v.FootAssessments)
            .HasForeignKey(f => f.PodiatryVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PodiatryGaitAnalysis>()
            .HasIndex(g => g.AnalysisDate);

        builder.Entity<PodiatryGaitAnalysis>()
            .HasOne(g => g.Patient)
            .WithMany(p => p.PodiatryGaitAnalyses)
            .HasForeignKey(g => g.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PodiatryGaitAnalysis>()
            .HasOne(g => g.PodiatryVisit)
            .WithMany(v => v.GaitAnalyses)
            .HasForeignKey(g => g.PodiatryVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PodiatryProcedure>()
            .HasIndex(p => p.ProcedureDate);

        builder.Entity<PodiatryProcedure>()
            .HasOne(p => p.Patient)
            .WithMany(pa => pa.PodiatryProcedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PodiatryProcedure>()
            .HasOne(p => p.PodiatryVisit)
            .WithMany(v => v.Procedures)
            .HasForeignKey(p => p.PodiatryVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PodiatryProcedure>()
            .Property(p => p.Fee)
            .HasPrecision(18, 2);

        builder.Entity<OrthoticPrescription>()
            .HasIndex(o => o.PrescriptionDate);

        builder.Entity<OrthoticPrescription>()
            .HasOne(o => o.Patient)
            .WithMany(p => p.OrthoticPrescriptions)
            .HasForeignKey(o => o.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrthoticPrescription>()
            .HasOne(o => o.PodiatryVisit)
            .WithMany(v => v.OrthoticPrescriptions)
            .HasForeignKey(o => o.PodiatryVisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrthoticPrescription>()
            .Property(o => o.Cost)
            .HasPrecision(18, 2);

        builder.Entity<FootCondition>()
            .HasIndex(f => f.PatientId);

        builder.Entity<FootCondition>()
            .HasOne(f => f.Patient)
            .WithMany(p => p.FootConditions)
            .HasForeignKey(f => f.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
