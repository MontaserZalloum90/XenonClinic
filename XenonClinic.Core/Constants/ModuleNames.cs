namespace XenonClinic.Core.Constants;

/// <summary>
/// Centralized module name constants for the XenonClinic system
/// </summary>
public static class ModuleNames
{
    // Core Modules
    public const string CaseManagement = "CaseManagement";
    public const string Audiology = "Audiology";
    public const string Laboratory = "Laboratory";

    // Operations Modules
    public const string HR = "HR";
    public const string Inventory = "Inventory";

    // Financial Modules
    public const string Financial = "Financial";
    public const string Sales = "Sales";
    public const string Procurement = "Procurement";

    /// <summary>
    /// All available module names
    /// </summary>
    public static readonly string[] AllModules = new[]
    {
        CaseManagement,
        Audiology,
        Laboratory,
        HR,
        Inventory,
        Financial,
        Sales,
        Procurement
    };

    /// <summary>
    /// Module categories for organization
    /// </summary>
    public static class Categories
    {
        public const string Clinical = "Clinical";
        public const string Operations = "Operations";
        public const string Financial = "Financial";
    }
}
