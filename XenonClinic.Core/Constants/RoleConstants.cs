namespace XenonClinic.Core.Constants;

/// <summary>
/// Centralized constants for all system roles.
/// </summary>
public static class RoleConstants
{
    // System-level roles
    public const string SuperAdmin = "SuperAdmin";

    // Tenant-level roles
    public const string TenantAdmin = "TenantAdmin";

    // Company-level roles
    public const string CompanyAdmin = "CompanyAdmin";

    // Branch-level roles
    public const string BranchAdmin = "BranchAdmin";

    // General administrative role
    public const string Admin = "Admin";

    // Operational roles
    public const string Audiologist = "Audiologist";
    public const string Receptionist = "Receptionist";
    public const string Technician = "Technician";

    /// <summary>
    /// Array of all role names for seeding purposes.
    /// </summary>
    public static readonly string[] AllRoles =
    {
        SuperAdmin,
        TenantAdmin,
        CompanyAdmin,
        Admin,
        BranchAdmin,
        Audiologist,
        Receptionist,
        Technician
    };

    /// <summary>
    /// Roles that have administrative privileges.
    /// </summary>
    public static readonly string[] AdminRoles =
    {
        SuperAdmin,
        TenantAdmin,
        CompanyAdmin,
        Admin
    };

    /// <summary>
    /// Returns a comma-separated string of roles for use in [Authorize] attributes.
    /// </summary>
    public static string GetRolesString(params string[] roles)
    {
        return string.Join(",", roles);
    }

    /// <summary>
    /// Common role combinations for authorization.
    /// </summary>
    public static class Combined
    {
        public static readonly string SuperAndTenantAdmin = GetRolesString(SuperAdmin, TenantAdmin);
        public static readonly string SuperTenantAndCompanyAdmin = GetRolesString(SuperAdmin, TenantAdmin, CompanyAdmin);
        public static readonly string AllAdmins = GetRolesString(AdminRoles);
    }
}
