namespace XenonClinic.Core.DTOs;

#region Permission DTOs

/// <summary>
/// System permission definition
/// </summary>
public class PermissionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public bool IsPHIRelated { get; set; }
    public bool IsSystemPermission { get; set; }
}

/// <summary>
/// Role definition
/// </summary>
public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create/Update role request
/// </summary>
public class SaveRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string RoleType { get; set; } = string.Empty;
    public List<int> PermissionIds { get; set; } = new();
}

/// <summary>
/// User role assignment
/// </summary>
public class UserRoleDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<RoleDto> Roles { get; set; } = new();
    public List<PermissionDto> DirectPermissions { get; set; } = new();
    public List<string> EffectivePermissions { get; set; } = new();
}

/// <summary>
/// Assign roles to user request
/// </summary>
public class AssignRolesDto
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = new();
    public List<int>? DirectPermissionIds { get; set; }
}

#endregion

#region Access Control DTOs

/// <summary>
/// Resource access check request
/// </summary>
public class AccessCheckRequestDto
{
    public int UserId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

/// <summary>
/// Resource access check result
/// </summary>
public class AccessCheckResultDto
{
    public bool IsAllowed { get; set; }
    public string? DenialReason { get; set; }
    public List<string> MatchedPermissions { get; set; } = new();
    public bool RequiresEmergencyAccess { get; set; }
    public List<string>? RequiredPermissions { get; set; }
}

/// <summary>
/// Data-level access rule
/// </summary>
public class DataAccessRuleDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public bool AllowAccess { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Create data access rule request
/// </summary>
public class CreateDataAccessRuleDto
{
    public string RuleName { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public bool AllowAccess { get; set; }
    public int Priority { get; set; }
}

#endregion

#region Permission Categories

/// <summary>
/// Standard permission codes
/// </summary>
public static class PermissionCodes
{
    // Patient Management
    public const string PatientView = "PATIENT_VIEW";
    public const string PatientCreate = "PATIENT_CREATE";
    public const string PatientEdit = "PATIENT_EDIT";
    public const string PatientDelete = "PATIENT_DELETE";
    public const string PatientExport = "PATIENT_EXPORT";
    public const string PatientMerge = "PATIENT_MERGE";

    // Medical Records
    public const string MedicalRecordView = "MEDICAL_RECORD_VIEW";
    public const string MedicalRecordCreate = "MEDICAL_RECORD_CREATE";
    public const string MedicalRecordEdit = "MEDICAL_RECORD_EDIT";
    public const string MedicalRecordDelete = "MEDICAL_RECORD_DELETE";
    public const string MedicalRecordPrint = "MEDICAL_RECORD_PRINT";

    // Prescriptions
    public const string PrescriptionView = "PRESCRIPTION_VIEW";
    public const string PrescriptionCreate = "PRESCRIPTION_CREATE";
    public const string PrescriptionEdit = "PRESCRIPTION_EDIT";
    public const string PrescriptionCancel = "PRESCRIPTION_CANCEL";
    public const string ControlledSubstancePrescribe = "CONTROLLED_SUBSTANCE_PRESCRIBE";

    // Appointments
    public const string AppointmentView = "APPOINTMENT_VIEW";
    public const string AppointmentCreate = "APPOINTMENT_CREATE";
    public const string AppointmentEdit = "APPOINTMENT_EDIT";
    public const string AppointmentCancel = "APPOINTMENT_CANCEL";

    // Billing & Insurance
    public const string BillingView = "BILLING_VIEW";
    public const string BillingCreate = "BILLING_CREATE";
    public const string BillingEdit = "BILLING_EDIT";
    public const string BillingRefund = "BILLING_REFUND";
    public const string InsuranceClaimSubmit = "INSURANCE_CLAIM_SUBMIT";
    public const string InsuranceClaimView = "INSURANCE_CLAIM_VIEW";

    // Lab & Imaging
    public const string LabResultView = "LAB_RESULT_VIEW";
    public const string LabResultCreate = "LAB_RESULT_CREATE";
    public const string LabResultEdit = "LAB_RESULT_EDIT";
    public const string ImagingView = "IMAGING_VIEW";
    public const string ImagingUpload = "IMAGING_UPLOAD";

    // Reports
    public const string ReportView = "REPORT_VIEW";
    public const string ReportCreate = "REPORT_CREATE";
    public const string ReportExport = "REPORT_EXPORT";
    public const string FinancialReportView = "FINANCIAL_REPORT_VIEW";
    public const string ClinicalReportView = "CLINICAL_REPORT_VIEW";

    // Administration
    public const string UserManage = "USER_MANAGE";
    public const string RoleManage = "ROLE_MANAGE";
    public const string SettingsManage = "SETTINGS_MANAGE";
    public const string AuditLogView = "AUDIT_LOG_VIEW";
    public const string SystemAdmin = "SYSTEM_ADMIN";

    // Emergency Access
    public const string EmergencyAccess = "EMERGENCY_ACCESS";
    public const string BreakTheGlass = "BREAK_THE_GLASS";
}

/// <summary>
/// Standard role types
/// </summary>
public static class RoleTypes
{
    public const string SystemAdmin = "SYSTEM_ADMIN";
    public const string ClinicAdmin = "CLINIC_ADMIN";
    public const string Physician = "PHYSICIAN";
    public const string Nurse = "NURSE";
    public const string MedicalAssistant = "MEDICAL_ASSISTANT";
    public const string Receptionist = "RECEPTIONIST";
    public const string BillingStaff = "BILLING_STAFF";
    public const string LabTechnician = "LAB_TECHNICIAN";
    public const string Pharmacist = "PHARMACIST";
    public const string Patient = "PATIENT";
    public const string Custom = "CUSTOM";
}

/// <summary>
/// Permission categories
/// </summary>
public static class PermissionCategories
{
    public const string PatientManagement = "PATIENT_MANAGEMENT";
    public const string ClinicalCare = "CLINICAL_CARE";
    public const string Prescriptions = "PRESCRIPTIONS";
    public const string Scheduling = "SCHEDULING";
    public const string Billing = "BILLING";
    public const string Laboratory = "LABORATORY";
    public const string Imaging = "IMAGING";
    public const string Reporting = "REPORTING";
    public const string Administration = "ADMINISTRATION";
    public const string Emergency = "EMERGENCY";
}

#endregion

#region Permission Matrix DTOs

/// <summary>
/// Role-Permission matrix for UI display
/// </summary>
public class PermissionMatrixDto
{
    public List<RoleSummaryDto> Roles { get; set; } = new();
    public List<PermissionCategoryDto> Categories { get; set; } = new();
}

/// <summary>
/// Role summary for matrix
/// </summary>
public class RoleSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoleType { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
}

/// <summary>
/// Permission category with permissions
/// </summary>
public class PermissionCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<PermissionMatrixItemDto> Permissions { get; set; } = new();
}

/// <summary>
/// Permission matrix item showing role assignments
/// </summary>
public class PermissionMatrixItemDto
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public bool IsPHIRelated { get; set; }
    public Dictionary<int, bool> RoleAssignments { get; set; } = new(); // RoleId -> HasPermission
}

/// <summary>
/// Bulk permission update request
/// </summary>
public class BulkPermissionUpdateDto
{
    public int RoleId { get; set; }
    public List<int> AddPermissionIds { get; set; } = new();
    public List<int> RemovePermissionIds { get; set; } = new();
}

#endregion
