using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Role-Based Access Control service implementation
/// </summary>
public class RbacService : IRbacService
{
    private readonly ClinicDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RbacService> _logger;
    private readonly IAuditService _auditService;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public RbacService(ClinicDbContext context, IMemoryCache cache, ILogger<RbacService> logger, IAuditService auditService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _auditService = auditService;
    }

    #region Permission Management

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Set<Permission>().OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
        return permissions.Select(MapToDto).ToList();
    }

    public async Task<List<PermissionDto>> GetPermissionsByCategoryAsync(string category)
    {
        var permissions = await _context.Set<Permission>().Where(p => p.Category == category).OrderBy(p => p.Name).ToListAsync();
        return permissions.Select(MapToDto).ToList();
    }

    public async Task<PermissionDto?> GetPermissionByCodeAsync(string code)
    {
        var permission = await _context.Set<Permission>().FirstOrDefaultAsync(p => p.Code == code);
        return permission != null ? MapToDto(permission) : null;
    }

    public async Task InitializeDefaultPermissionsAsync()
    {
        var existingCodes = await _context.Set<Permission>().Select(p => p.Code).ToListAsync();
        var defaultPerms = GetDefaultPermissions().Where(p => !existingCodes.Contains(p.Code)).ToList();
        
        if (defaultPerms.Any())
        {
            _context.Set<Permission>().AddRange(defaultPerms);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Initialized {Count} default permissions", defaultPerms.Count);
        }
    }

    private static List<Permission> GetDefaultPermissions()
    {
        return new List<Permission>
        {
            // Patient Management
            new() { Code = PermissionCodes.PatientView, Name = "View Patients", Category = PermissionCategories.PatientManagement, ResourceType = "Patient", IsPHIRelated = true },
            new() { Code = PermissionCodes.PatientCreate, Name = "Create Patients", Category = PermissionCategories.PatientManagement, ResourceType = "Patient", IsPHIRelated = true },
            new() { Code = PermissionCodes.PatientEdit, Name = "Edit Patients", Category = PermissionCategories.PatientManagement, ResourceType = "Patient", IsPHIRelated = true },
            new() { Code = PermissionCodes.PatientDelete, Name = "Delete Patients", Category = PermissionCategories.PatientManagement, ResourceType = "Patient", IsPHIRelated = true },
            new() { Code = PermissionCodes.PatientExport, Name = "Export Patient Data", Category = PermissionCategories.PatientManagement, ResourceType = "Patient", IsPHIRelated = true },
            
            // Medical Records
            new() { Code = PermissionCodes.MedicalRecordView, Name = "View Medical Records", Category = PermissionCategories.ClinicalCare, ResourceType = "MedicalRecord", IsPHIRelated = true },
            new() { Code = PermissionCodes.MedicalRecordCreate, Name = "Create Medical Records", Category = PermissionCategories.ClinicalCare, ResourceType = "MedicalRecord", IsPHIRelated = true },
            new() { Code = PermissionCodes.MedicalRecordEdit, Name = "Edit Medical Records", Category = PermissionCategories.ClinicalCare, ResourceType = "MedicalRecord", IsPHIRelated = true },
            new() { Code = PermissionCodes.MedicalRecordDelete, Name = "Delete Medical Records", Category = PermissionCategories.ClinicalCare, ResourceType = "MedicalRecord", IsPHIRelated = true },
            
            // Prescriptions
            new() { Code = PermissionCodes.PrescriptionView, Name = "View Prescriptions", Category = PermissionCategories.Prescriptions, ResourceType = "Prescription", IsPHIRelated = true },
            new() { Code = PermissionCodes.PrescriptionCreate, Name = "Create Prescriptions", Category = PermissionCategories.Prescriptions, ResourceType = "Prescription", IsPHIRelated = true },
            new() { Code = PermissionCodes.ControlledSubstancePrescribe, Name = "Prescribe Controlled Substances", Category = PermissionCategories.Prescriptions, ResourceType = "Prescription", IsPHIRelated = true },
            
            // Appointments
            new() { Code = PermissionCodes.AppointmentView, Name = "View Appointments", Category = PermissionCategories.Scheduling, ResourceType = "Appointment", IsPHIRelated = false },
            new() { Code = PermissionCodes.AppointmentCreate, Name = "Create Appointments", Category = PermissionCategories.Scheduling, ResourceType = "Appointment", IsPHIRelated = false },
            new() { Code = PermissionCodes.AppointmentEdit, Name = "Edit Appointments", Category = PermissionCategories.Scheduling, ResourceType = "Appointment", IsPHIRelated = false },
            new() { Code = PermissionCodes.AppointmentCancel, Name = "Cancel Appointments", Category = PermissionCategories.Scheduling, ResourceType = "Appointment", IsPHIRelated = false },
            
            // Billing
            new() { Code = PermissionCodes.BillingView, Name = "View Billing", Category = PermissionCategories.Billing, ResourceType = "Invoice", IsPHIRelated = false },
            new() { Code = PermissionCodes.BillingCreate, Name = "Create Bills", Category = PermissionCategories.Billing, ResourceType = "Invoice", IsPHIRelated = false },
            new() { Code = PermissionCodes.BillingRefund, Name = "Process Refunds", Category = PermissionCategories.Billing, ResourceType = "Invoice", IsPHIRelated = false },
            new() { Code = PermissionCodes.InsuranceClaimSubmit, Name = "Submit Insurance Claims", Category = PermissionCategories.Billing, ResourceType = "InsuranceClaim", IsPHIRelated = true },
            
            // Lab & Imaging
            new() { Code = PermissionCodes.LabResultView, Name = "View Lab Results", Category = PermissionCategories.Laboratory, ResourceType = "LabResult", IsPHIRelated = true },
            new() { Code = PermissionCodes.LabResultCreate, Name = "Create Lab Results", Category = PermissionCategories.Laboratory, ResourceType = "LabResult", IsPHIRelated = true },
            new() { Code = PermissionCodes.ImagingView, Name = "View Imaging", Category = PermissionCategories.Imaging, ResourceType = "Imaging", IsPHIRelated = true },
            
            // Reports
            new() { Code = PermissionCodes.ReportView, Name = "View Reports", Category = PermissionCategories.Reporting, ResourceType = "Report", IsPHIRelated = false },
            new() { Code = PermissionCodes.FinancialReportView, Name = "View Financial Reports", Category = PermissionCategories.Reporting, ResourceType = "Report", IsPHIRelated = false },
            
            // Administration
            new() { Code = PermissionCodes.UserManage, Name = "Manage Users", Category = PermissionCategories.Administration, ResourceType = "User", IsPHIRelated = false, IsSystemPermission = true },
            new() { Code = PermissionCodes.RoleManage, Name = "Manage Roles", Category = PermissionCategories.Administration, ResourceType = "Role", IsPHIRelated = false, IsSystemPermission = true },
            new() { Code = PermissionCodes.SettingsManage, Name = "Manage Settings", Category = PermissionCategories.Administration, ResourceType = "Settings", IsPHIRelated = false, IsSystemPermission = true },
            new() { Code = PermissionCodes.AuditLogView, Name = "View Audit Logs", Category = PermissionCategories.Administration, ResourceType = "AuditLog", IsPHIRelated = true, IsSystemPermission = true },
            new() { Code = PermissionCodes.SystemAdmin, Name = "System Administrator", Category = PermissionCategories.Administration, ResourceType = "System", IsPHIRelated = false, IsSystemPermission = true },
            
            // Emergency
            new() { Code = PermissionCodes.EmergencyAccess, Name = "Emergency Access", Category = PermissionCategories.Emergency, ResourceType = "Patient", IsPHIRelated = true },
            new() { Code = PermissionCodes.BreakTheGlass, Name = "Break-the-Glass Access", Category = PermissionCategories.Emergency, ResourceType = "Patient", IsPHIRelated = true }
        };
    }

    #endregion

    #region Role Management

    public async Task<List<RoleDto>> GetAllRolesAsync(int? branchId = null)
    {
        var query = _context.Set<Role>().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).AsQueryable();
        if (branchId.HasValue)
            query = query.Where(r => r.BranchId == null || r.BranchId == branchId.Value);
        
        var roles = await query.OrderBy(r => r.Name).ToListAsync();
        
        var userCounts = await _context.Set<UserRole>().GroupBy(ur => ur.RoleId).Select(g => new { RoleId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.RoleId, x => x.Count);
        
        return roles.Select(r => MapToDto(r, userCounts.GetValueOrDefault(r.Id, 0))).ToList();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
    {
        var role = await _context.Set<Role>().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return null;
        
        var userCount = await _context.Set<UserRole>().CountAsync(ur => ur.RoleId == roleId);
        return MapToDto(role, userCount);
    }

    public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
    {
        var role = await _context.Set<Role>().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).FirstOrDefaultAsync(r => r.Name == roleName);
        return role != null ? MapToDto(role, 0) : null;
    }

    public async Task<RoleDto> CreateRoleAsync(SaveRoleDto request, int createdByUserId)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            RoleType = request.RoleType,
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        _context.Set<Role>().Add(role);
        await _context.SaveChangesAsync();

        if (request.PermissionIds.Any())
        {
            var rolePermissions = request.PermissionIds.Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }).ToList();
            _context.Set<RolePermission>().AddRange(rolePermissions);
            await _context.SaveChangesAsync();
        }

        await _auditService.LogDataChangeAsync(createdByUserId, "Role", role.Id.ToString(), "CREATE", null, role);
        await ClearAllCachesAsync();
        
        return (await GetRoleByIdAsync(role.Id))!;
    }

    public async Task<RoleDto> UpdateRoleAsync(int roleId, SaveRoleDto request, int updatedByUserId)
    {
        var role = await _context.Set<Role>().Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) throw new InvalidOperationException("Role not found");
        if (role.IsSystemRole) throw new InvalidOperationException("Cannot modify system roles");

        var oldRole = JsonSerializer.Serialize(role);
        role.Name = request.Name;
        role.Description = request.Description;
        role.RoleType = request.RoleType;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedByUserId = updatedByUserId;

        // Update permissions
        _context.Set<RolePermission>().RemoveRange(role.RolePermissions);
        var newPermissions = request.PermissionIds.Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }).ToList();
        _context.Set<RolePermission>().AddRange(newPermissions);
        
        await _context.SaveChangesAsync();
        await _auditService.LogDataChangeAsync(updatedByUserId, "Role", role.Id.ToString(), "UPDATE", oldRole, role);
        await ClearAllCachesAsync();

        return (await GetRoleByIdAsync(role.Id))!;
    }

    public async Task<bool> DeleteRoleAsync(int roleId, int deletedByUserId)
    {
        var role = await _context.Set<Role>().FindAsync(roleId);
        if (role == null) return false;
        if (role.IsSystemRole) throw new InvalidOperationException("Cannot delete system roles");

        var userCount = await _context.Set<UserRole>().CountAsync(ur => ur.RoleId == roleId);
        if (userCount > 0) throw new InvalidOperationException($"Cannot delete role with {userCount} assigned users");

        _context.Set<Role>().Remove(role);
        await _context.SaveChangesAsync();
        await _auditService.LogDataChangeAsync(deletedByUserId, "Role", roleId.ToString(), "DELETE", role, null);
        await ClearAllCachesAsync();
        
        return true;
    }

    public async Task<RoleDto> DuplicateRoleAsync(int roleId, string newName, int createdByUserId)
    {
        var source = await _context.Set<Role>().Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == roleId);
        if (source == null) throw new InvalidOperationException("Source role not found");

        return await CreateRoleAsync(new SaveRoleDto
        {
            Name = newName,
            Description = source.Description,
            RoleType = RoleTypes.Custom,
            PermissionIds = source.RolePermissions.Select(rp => rp.PermissionId).ToList()
        }, createdByUserId);
    }

    public async Task InitializeDefaultRolesAsync()
    {
        await InitializeDefaultPermissionsAsync();
        
        var existingRoles = await _context.Set<Role>().Select(r => r.Name).ToListAsync();
        var allPermissions = await _context.Set<Permission>().ToDictionaryAsync(p => p.Code, p => p.Id);

        var defaultRoles = new Dictionary<string, (string desc, string[] perms)>
        {
            [RoleTypes.SystemAdmin] = ("System Administrator with full access", allPermissions.Keys.ToArray()),
            [RoleTypes.Physician] = ("Physician with clinical access", new[] { PermissionCodes.PatientView, PermissionCodes.PatientEdit, PermissionCodes.MedicalRecordView, PermissionCodes.MedicalRecordCreate, PermissionCodes.MedicalRecordEdit, PermissionCodes.PrescriptionView, PermissionCodes.PrescriptionCreate, PermissionCodes.ControlledSubstancePrescribe, PermissionCodes.AppointmentView, PermissionCodes.LabResultView, PermissionCodes.ImagingView, PermissionCodes.EmergencyAccess }),
            [RoleTypes.Nurse] = ("Nurse with clinical support access", new[] { PermissionCodes.PatientView, PermissionCodes.MedicalRecordView, PermissionCodes.MedicalRecordCreate, PermissionCodes.PrescriptionView, PermissionCodes.AppointmentView, PermissionCodes.AppointmentEdit, PermissionCodes.LabResultView }),
            [RoleTypes.Receptionist] = ("Front desk with scheduling access", new[] { PermissionCodes.PatientView, PermissionCodes.PatientCreate, PermissionCodes.AppointmentView, PermissionCodes.AppointmentCreate, PermissionCodes.AppointmentEdit, PermissionCodes.AppointmentCancel }),
            [RoleTypes.BillingStaff] = ("Billing department access", new[] { PermissionCodes.PatientView, PermissionCodes.BillingView, PermissionCodes.BillingCreate, PermissionCodes.BillingRefund, PermissionCodes.InsuranceClaimSubmit, PermissionCodes.InsuranceClaimView })
        };

        foreach (var (roleName, (desc, perms)) in defaultRoles)
        {
            if (existingRoles.Contains(roleName)) continue;

            var role = new Role { Name = roleName, Description = desc, RoleType = roleName, IsSystemRole = true, IsActive = true, CreatedAt = DateTime.UtcNow };
            _context.Set<Role>().Add(role);
            await _context.SaveChangesAsync();

            var rolePerms = perms.Where(p => allPermissions.ContainsKey(p)).Select(p => new RolePermission { RoleId = role.Id, PermissionId = allPermissions[p] }).ToList();
            _context.Set<RolePermission>().AddRange(rolePerms);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Initialized default roles");
    }

    #endregion

    #region User Role Assignment

    public async Task<UserRoleDto> GetUserRolesAsync(int userId)
    {
        var cacheKey = $"user_roles_{userId}";
        if (_cache.TryGetValue(cacheKey, out UserRoleDto? cached) && cached != null)
            return cached;

        var userRoles = await _context.Set<UserRole>()
            .Include(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();

        var directPerms = await _context.Set<UserPermission>()
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId)
            .ToListAsync();

        var result = new UserRoleDto
        {
            UserId = userId,
            Roles = userRoles.Select(ur => MapToDto(ur.Role, 0)).ToList(),
            DirectPermissions = directPerms.Select(up => MapToDto(up.Permission)).ToList(),
            EffectivePermissions = userRoles.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
                .Union(directPerms.Select(up => up.Permission.Code))
                .Distinct().ToList()
        };

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<bool> AssignRolesToUserAsync(AssignRolesDto request, int assignedByUserId)
    {
        // Remove existing roles
        var existing = await _context.Set<UserRole>().Where(ur => ur.UserId == request.UserId).ToListAsync();
        _context.Set<UserRole>().RemoveRange(existing);

        // Add new roles
        var newRoles = request.RoleIds.Select(rid => new UserRole { UserId = request.UserId, RoleId = rid, AssignedAt = DateTime.UtcNow, AssignedByUserId = assignedByUserId }).ToList();
        _context.Set<UserRole>().AddRange(newRoles);

        // Handle direct permissions if provided
        if (request.DirectPermissionIds != null)
        {
            var existingPerms = await _context.Set<UserPermission>().Where(up => up.UserId == request.UserId).ToListAsync();
            _context.Set<UserPermission>().RemoveRange(existingPerms);

            var newPerms = request.DirectPermissionIds.Select(pid => new UserPermission { UserId = request.UserId, PermissionId = pid, GrantedAt = DateTime.UtcNow, GrantedByUserId = assignedByUserId }).ToList();
            _context.Set<UserPermission>().AddRange(newPerms);
        }

        await _context.SaveChangesAsync();
        await _auditService.LogAsync(new AuditLogEntry { EventType = AuditEventTypes.RoleAssigned, EventCategory = AuditEventCategories.Authorization, Action = "ASSIGN_ROLES", ResourceType = "User", ResourceId = request.UserId.ToString(), UserId = assignedByUserId, NewValues = new { RoleIds = request.RoleIds }, IsSuccess = true });
        await ClearUserCacheAsync(request.UserId);
        
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId, int removedByUserId)
    {
        var userRole = await _context.Set<UserRole>().FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole == null) return false;

        _context.Set<UserRole>().Remove(userRole);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync(new AuditLogEntry { EventType = AuditEventTypes.RoleRevoked, EventCategory = AuditEventCategories.Authorization, Action = "REMOVE_ROLE", ResourceType = "User", ResourceId = userId.ToString(), UserId = removedByUserId, OldValues = new { RoleId = roleId }, IsSuccess = true });
        await ClearUserCacheAsync(userId);
        
        return true;
    }

    public async Task<List<UserRoleDto>> GetUsersInRoleAsync(int roleId)
    {
        var userRoles = await _context.Set<UserRole>().Where(ur => ur.RoleId == roleId).Select(ur => ur.UserId).ToListAsync();
        var results = new List<UserRoleDto>();
        foreach (var userId in userRoles)
            results.Add(await GetUserRolesAsync(userId));
        return results;
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.Roles.Any(r => r.Name == roleName);
    }

    #endregion

    #region Access Control

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.EffectivePermissions.Contains(permissionCode);
    }

    public async Task<bool> HasAnyPermissionAsync(int userId, params string[] permissionCodes)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return permissionCodes.Any(p => userRoles.EffectivePermissions.Contains(p));
    }

    public async Task<bool> HasAllPermissionsAsync(int userId, params string[] permissionCodes)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return permissionCodes.All(p => userRoles.EffectivePermissions.Contains(p));
    }

    public async Task<AccessCheckResultDto> CheckAccessAsync(AccessCheckRequestDto request)
    {
        var userRoles = await GetUserRolesAsync(request.UserId);
        var requiredPermission = $"{request.ResourceType.ToUpper()}_{request.Action.ToUpper()}";
        
        // Check for system admin
        if (userRoles.EffectivePermissions.Contains(PermissionCodes.SystemAdmin))
            return new AccessCheckResultDto { IsAllowed = true, MatchedPermissions = new List<string> { PermissionCodes.SystemAdmin } };

        // Check specific permission
        if (userRoles.EffectivePermissions.Contains(requiredPermission))
            return new AccessCheckResultDto { IsAllowed = true, MatchedPermissions = new List<string> { requiredPermission } };

        // Check data access rules
        var rules = await GetDataAccessRulesAsync(request.ResourceType);
        foreach (var rule in rules.OrderBy(r => r.Priority))
        {
            if (rule.RoleId.HasValue && !userRoles.Roles.Any(r => r.Id == rule.RoleId.Value))
                continue;
            // Evaluate rule condition here (simplified)
            if (rule.AllowAccess)
                return new AccessCheckResultDto { IsAllowed = true, MatchedPermissions = new List<string> { $"RULE:{rule.RuleName}" } };
        }

        // Denied
        await _auditService.LogAuthorizationAsync(request.UserId, request.ResourceType, request.Action, false, "Permission not found");
        return new AccessCheckResultDto
        {
            IsAllowed = false,
            DenialReason = $"Missing permission: {requiredPermission}",
            RequiredPermissions = new List<string> { requiredPermission },
            RequiresEmergencyAccess = userRoles.EffectivePermissions.Contains(PermissionCodes.EmergencyAccess)
        };
    }

    public async Task<List<string>> GetEffectivePermissionsAsync(int userId)
    {
        var userRoles = await GetUserRolesAsync(userId);
        return userRoles.EffectivePermissions;
    }

    public async Task<bool> ValidateEmergencyAccessAsync(int userId, int patientId, string justification)
    {
        if (string.IsNullOrWhiteSpace(justification) || justification.Length < 10)
            return false;

        var hasEmergencyPerm = await HasPermissionAsync(userId, PermissionCodes.EmergencyAccess);
        if (!hasEmergencyPerm) return false;

        await _auditService.LogEmergencyAccessAsync(userId, patientId, "PATIENT", justification);
        return true;
    }

    #endregion

    #region Data Access Rules

    public async Task<List<DataAccessRuleDto>> GetDataAccessRulesAsync(string? resourceType = null)
    {
        var query = _context.Set<DataAccessRule>().Include(r => r.Role).AsQueryable();
        if (!string.IsNullOrEmpty(resourceType))
            query = query.Where(r => r.ResourceType == resourceType);

        var rules = await query.OrderBy(r => r.Priority).ToListAsync();
        return rules.Select(r => new DataAccessRuleDto { Id = r.Id, RuleName = r.RuleName, ResourceType = r.ResourceType, Condition = r.Condition, RoleId = r.RoleId, RoleName = r.Role?.Name, AllowAccess = r.AllowAccess, Priority = r.Priority, IsActive = r.IsActive }).ToList();
    }

    public async Task<DataAccessRuleDto> CreateDataAccessRuleAsync(CreateDataAccessRuleDto request, int createdByUserId)
    {
        var rule = new DataAccessRule { RuleName = request.RuleName, ResourceType = request.ResourceType, Condition = request.Condition, RoleId = request.RoleId, AllowAccess = request.AllowAccess, Priority = request.Priority, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedByUserId = createdByUserId };
        _context.Set<DataAccessRule>().Add(rule);
        await _context.SaveChangesAsync();
        return (await GetDataAccessRulesAsync()).First(r => r.Id == rule.Id);
    }

    public async Task<DataAccessRuleDto> UpdateDataAccessRuleAsync(int ruleId, CreateDataAccessRuleDto request, int updatedByUserId)
    {
        var rule = await _context.Set<DataAccessRule>().FindAsync(ruleId);
        if (rule == null) throw new InvalidOperationException("Rule not found");
        rule.RuleName = request.RuleName; rule.ResourceType = request.ResourceType; rule.Condition = request.Condition; rule.RoleId = request.RoleId; rule.AllowAccess = request.AllowAccess; rule.Priority = request.Priority; rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (await GetDataAccessRulesAsync()).First(r => r.Id == rule.Id);
    }

    public async Task<bool> DeleteDataAccessRuleAsync(int ruleId, int deletedByUserId)
    {
        var rule = await _context.Set<DataAccessRule>().FindAsync(ruleId);
        if (rule == null) return false;
        _context.Set<DataAccessRule>().Remove(rule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetDataFilterAsync(int userId, string resourceType)
    {
        var userRoles = await GetUserRolesAsync(userId);
        if (userRoles.EffectivePermissions.Contains(PermissionCodes.SystemAdmin))
            return null; // No filter for admin

        var rules = await GetDataAccessRulesAsync(resourceType);
        var applicableRules = rules.Where(r => r.IsActive && (!r.RoleId.HasValue || userRoles.Roles.Any(ur => ur.Id == r.RoleId.Value))).ToList();
        
        return applicableRules.Any() ? string.Join(" AND ", applicableRules.Select(r => r.Condition)) : null;
    }

    #endregion

    #region Permission Matrix

    public async Task<PermissionMatrixDto> GetPermissionMatrixAsync()
    {
        var roles = await _context.Set<Role>().Include(r => r.RolePermissions).Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
        var permissions = await _context.Set<Permission>().OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();

        var matrix = new PermissionMatrixDto
        {
            Roles = roles.Select(r => new RoleSummaryDto { Id = r.Id, Name = r.Name, RoleType = r.RoleType, IsSystemRole = r.IsSystemRole }).ToList(),
            Categories = permissions.GroupBy(p => p.Category).Select(g => new PermissionCategoryDto
            {
                Category = g.Key,
                DisplayName = g.Key.Replace("_", " "),
                Permissions = g.Select(p => new PermissionMatrixItemDto
                {
                    PermissionId = p.Id,
                    PermissionCode = p.Code,
                    PermissionName = p.Name,
                    IsPHIRelated = p.IsPHIRelated,
                    RoleAssignments = roles.ToDictionary(r => r.Id, r => r.RolePermissions.Any(rp => rp.PermissionId == p.Id))
                }).ToList()
            }).ToList()
        };

        return matrix;
    }

    public async Task<bool> BulkUpdatePermissionsAsync(BulkPermissionUpdateDto request, int updatedByUserId)
    {
        var role = await _context.Set<Role>().Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == request.RoleId);
        if (role == null || role.IsSystemRole) return false;

        // Remove specified permissions
        var toRemove = role.RolePermissions.Where(rp => request.RemovePermissionIds.Contains(rp.PermissionId)).ToList();
        _context.Set<RolePermission>().RemoveRange(toRemove);

        // Add new permissions
        var existingIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var toAdd = request.AddPermissionIds.Where(pid => !existingIds.Contains(pid)).Select(pid => new RolePermission { RoleId = role.Id, PermissionId = pid }).ToList();
        _context.Set<RolePermission>().AddRange(toAdd);

        await _context.SaveChangesAsync();
        await ClearAllCachesAsync();
        return true;
    }

    #endregion

    #region Caching

    public Task ClearUserCacheAsync(int userId)
    {
        _cache.Remove($"user_roles_{userId}");
        return Task.CompletedTask;
    }

    public Task ClearAllCachesAsync()
    {
        // In production, use distributed cache with pattern deletion
        // For IMemoryCache, we'd need to track keys or use CancellationTokenSource
        return Task.CompletedTask;
    }

    #endregion

    #region Mapping

    private static PermissionDto MapToDto(Permission p) => new() { Id = p.Id, Code = p.Code, Name = p.Name, Description = p.Description, Category = p.Category, ResourceType = p.ResourceType, IsPHIRelated = p.IsPHIRelated, IsSystemPermission = p.IsSystemPermission };

    private static RoleDto MapToDto(Role r, int userCount) => new() { Id = r.Id, Name = r.Name, Description = r.Description, RoleType = r.RoleType, IsSystemRole = r.IsSystemRole, IsActive = r.IsActive, Permissions = r.RolePermissions?.Select(rp => MapToDto(rp.Permission)).ToList() ?? new(), UserCount = userCount, CreatedAt = r.CreatedAt };

    #endregion
}

#region RBAC Entities

[Table("Permissions")]
public class Permission
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [Required, MaxLength(50)] public string Category { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string ResourceType { get; set; } = string.Empty;
    public bool IsPHIRelated { get; set; }
    public bool IsSystemPermission { get; set; }
}

[Table("Roles")]
public class Role : IBranchEntity
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [Required, MaxLength(50)] public string RoleType { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; } = true;
    public int? BranchId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    int IBranchEntity.BranchId { get => BranchId ?? 0; set => BranchId = value; }
}

[Table("RolePermissions")]
public class RolePermission
{
    [Key] public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    [ForeignKey(nameof(RoleId))] public virtual Role Role { get; set; } = null!;
    [ForeignKey(nameof(PermissionId))] public virtual Permission Permission { get; set; } = null!;
}

[Table("UserRoles")]
public class UserRole
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public int? AssignedByUserId { get; set; }
    [ForeignKey(nameof(RoleId))] public virtual Role Role { get; set; } = null!;
}

[Table("UserPermissions")]
public class UserPermission
{
    [Key] public int Id { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public DateTime GrantedAt { get; set; }
    public int? GrantedByUserId { get; set; }
    [ForeignKey(nameof(PermissionId))] public virtual Permission Permission { get; set; } = null!;
}

[Table("DataAccessRules")]
public class DataAccessRule
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(200)] public string RuleName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string ResourceType { get; set; } = string.Empty;
    [Required] public string Condition { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public bool AllowAccess { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [ForeignKey(nameof(RoleId))] public virtual Role? Role { get; set; }
}

#endregion
