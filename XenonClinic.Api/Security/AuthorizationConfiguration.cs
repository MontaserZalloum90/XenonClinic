using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Core.DTOs;

namespace XenonClinic.Api.Security;

/// <summary>
/// Authorization configuration for the XenonClinic API
/// </summary>
public static class AuthorizationConfiguration
{
    /// <summary>
    /// Adds authorization policies based on permission codes
    /// </summary>
    public static IServiceCollection AddXenonAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // System Administration
            options.AddPolicy("SystemAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin)));

            options.AddPolicy("SecurityAdmin", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" &&
                              (c.Value == PermissionCodes.SystemAdmin || c.Value == "SECURITY_ADMIN")) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            // User Management
            options.AddPolicy("UserManage", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.UserManage) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            options.AddPolicy("RoleManage", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.RoleManage) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin)));

            options.AddPolicy("SettingsManage", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.SettingsManage) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            // Audit Logging
            options.AddPolicy("AuditLogView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.AuditLogView) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            // Patient Management
            options.AddPolicy("PatientView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.PatientView) ||
                          HasClinicalRole(context)));

            options.AddPolicy("PatientCreate", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.PatientCreate) ||
                          HasClinicalRole(context)));

            options.AddPolicy("PatientEdit", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.PatientEdit) ||
                          HasClinicalRole(context)));

            // Medical Records
            options.AddPolicy("MedicalRecordView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.MedicalRecordView) ||
                          HasClinicalRole(context)));

            options.AddPolicy("MedicalRecordCreate", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.MedicalRecordCreate) ||
                          context.User.IsInRole(RoleTypes.Physician) ||
                          context.User.IsInRole(RoleTypes.Nurse)));

            // Prescriptions
            options.AddPolicy("PrescriptionCreate", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.PrescriptionCreate) ||
                          context.User.IsInRole(RoleTypes.Physician)));

            options.AddPolicy("ControlledSubstancePrescribe", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.ControlledSubstancePrescribe) ||
                          (context.User.IsInRole(RoleTypes.Physician) &&
                           context.User.HasClaim(c => c.Type == "dea_license"))));

            // Billing & Financial
            options.AddPolicy("BillingView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.BillingView) ||
                          context.User.IsInRole(RoleTypes.BillingStaff) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            options.AddPolicy("BillingCreate", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.BillingCreate) ||
                          context.User.IsInRole(RoleTypes.BillingStaff)));

            options.AddPolicy("FinancialReportView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.FinancialReportView) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            // Reports
            options.AddPolicy("ReportView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.ReportView) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin) ||
                          context.User.IsInRole(RoleTypes.Physician)));

            options.AddPolicy("ReportExport", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.ReportExport) ||
                          context.User.IsInRole(RoleTypes.SystemAdmin) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            options.AddPolicy("ClinicalReportView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.ClinicalReportView) ||
                          context.User.IsInRole(RoleTypes.Physician) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            // Lab & Imaging
            options.AddPolicy("LabResultView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.LabResultView) ||
                          HasClinicalRole(context) ||
                          context.User.IsInRole(RoleTypes.LabTechnician)));

            options.AddPolicy("ImagingView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.ImagingView) ||
                          HasClinicalRole(context)));

            // Emergency Access
            options.AddPolicy("EmergencyAccess", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.EmergencyAccess) ||
                          context.User.IsInRole(RoleTypes.Physician) ||
                          context.User.IsInRole(RoleTypes.Nurse)));

            options.AddPolicy("BreakTheGlass", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.BreakTheGlass) ||
                          context.User.IsInRole(RoleTypes.Physician)));

            // Appointments
            options.AddPolicy("AppointmentView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.AppointmentView) ||
                          HasAnyRole(context)));

            options.AddPolicy("AppointmentCreate", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.AppointmentCreate) ||
                          HasAnyRole(context)));

            // Insurance
            options.AddPolicy("InsuranceClaimView", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.InsuranceClaimView) ||
                          context.User.IsInRole(RoleTypes.BillingStaff) ||
                          context.User.IsInRole(RoleTypes.ClinicAdmin)));

            options.AddPolicy("InsuranceClaimSubmit", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(context =>
                          context.User.HasClaim(c => c.Type == "permission" && c.Value == PermissionCodes.InsuranceClaimSubmit) ||
                          context.User.IsInRole(RoleTypes.BillingStaff)));
        });

        // Add authorization handler for permission-based authorization
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    private static bool HasClinicalRole(AuthorizationHandlerContext context)
    {
        return context.User.IsInRole(RoleTypes.Physician) ||
               context.User.IsInRole(RoleTypes.Nurse) ||
               context.User.IsInRole(RoleTypes.MedicalAssistant) ||
               context.User.IsInRole(RoleTypes.SystemAdmin) ||
               context.User.IsInRole(RoleTypes.ClinicAdmin);
    }

    private static bool HasAnyRole(AuthorizationHandlerContext context)
    {
        return context.User.IsInRole(RoleTypes.SystemAdmin) ||
               context.User.IsInRole(RoleTypes.ClinicAdmin) ||
               context.User.IsInRole(RoleTypes.Physician) ||
               context.User.IsInRole(RoleTypes.Nurse) ||
               context.User.IsInRole(RoleTypes.MedicalAssistant) ||
               context.User.IsInRole(RoleTypes.Receptionist) ||
               context.User.IsInRole(RoleTypes.BillingStaff);
    }
}

/// <summary>
/// Custom authorization handler for permission-based access control
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Permission requirement for authorization
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
