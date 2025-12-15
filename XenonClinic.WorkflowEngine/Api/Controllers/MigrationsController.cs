using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.WorkflowEngine.Application.Services;

namespace XenonClinic.WorkflowEngine.Api.Controllers;

/// <summary>
/// API controller for process migration operations.
/// </summary>
[ApiController]
[Route("api/workflow/migrations")]
[Authorize(Policy = "ProcessDesigner")]
public class MigrationsController : ControllerBase
{
    private readonly IProcessMigrationService _migrationService;

    public MigrationsController(IProcessMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    /// <summary>
    /// Creates a new migration plan.
    /// </summary>
    [HttpPost("plans")]
    public async Task<ActionResult<MigrationPlan>> CreateMigrationPlan(
        [FromBody] CreateMigrationPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await _migrationService.CreateMigrationPlanAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMigrationPlan), new { planId = plan.Id }, plan);
    }

    /// <summary>
    /// Gets a migration plan by ID.
    /// </summary>
    [HttpGet("plans/{planId}")]
    public async Task<ActionResult<MigrationPlan>> GetMigrationPlan(
        string planId,
        CancellationToken cancellationToken)
    {
        var plan = await _migrationService.GetMigrationPlanAsync(planId, cancellationToken);
        if (plan == null)
        {
            return NotFound(new { message = $"Migration plan '{planId}' not found" });
        }
        return Ok(plan);
    }

    /// <summary>
    /// Validates a migration plan.
    /// </summary>
    [HttpPost("plans/{planId}/validate")]
    public async Task<ActionResult<MigrationValidationResult>> ValidateMigrationPlan(
        string planId,
        CancellationToken cancellationToken)
    {
        var result = await _migrationService.ValidateMigrationPlanAsync(planId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generates activity mappings automatically.
    /// </summary>
    [HttpPost("plans/generate-mappings")]
    public async Task<ActionResult<IList<ActivityMapping>>> GenerateMappings(
        [FromBody] GenerateMappingsRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        var mappings = await _migrationService.GenerateActivityMappingsAsync(
            request.SourceDefinitionId,
            request.TargetDefinitionId,
            tenantId,
            cancellationToken);
        return Ok(mappings);
    }

    /// <summary>
    /// Executes a migration.
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<MigrationExecution>> ExecuteMigration(
        [FromBody] ExecuteMigrationRequest request,
        CancellationToken cancellationToken)
    {
        var execution = await _migrationService.ExecuteMigrationAsync(request, cancellationToken);
        return Ok(execution);
    }

    /// <summary>
    /// Gets a migration execution by ID.
    /// </summary>
    [HttpGet("executions/{executionId}")]
    public async Task<ActionResult<MigrationExecution>> GetMigrationExecution(
        string executionId,
        CancellationToken cancellationToken)
    {
        var execution = await _migrationService.GetMigrationExecutionAsync(executionId, cancellationToken);
        if (execution == null)
        {
            return NotFound(new { message = $"Migration execution '{executionId}' not found" });
        }
        return Ok(execution);
    }

    /// <summary>
    /// Rolls back a migration.
    /// </summary>
    [HttpPost("executions/{executionId}/rollback")]
    public async Task<ActionResult<MigrationExecution>> RollbackMigration(
        string executionId,
        CancellationToken cancellationToken)
    {
        var execution = await _migrationService.RollbackMigrationAsync(executionId, cancellationToken);
        return Ok(execution);
    }

    /// <summary>
    /// Lists migration plans.
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<IList<MigrationPlan>>> ListMigrationPlans(
        [FromQuery] string? tenantId,
        CancellationToken cancellationToken)
    {
        var plans = await _migrationService.ListMigrationPlansAsync(tenantId, cancellationToken);
        return Ok(plans);
    }

    /// <summary>
    /// Lists migration executions.
    /// </summary>
    [HttpGet("executions")]
    public async Task<ActionResult<IList<MigrationExecution>>> ListMigrationExecutions(
        [FromQuery] string? planId,
        [FromQuery] MigrationStatus? status,
        CancellationToken cancellationToken)
    {
        var executions = await _migrationService.ListMigrationExecutionsAsync(planId, status, cancellationToken);
        return Ok(executions);
    }

    /// <summary>
    /// Gets migration statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<MigrationStatistics>> GetMigrationStatistics(
        [FromQuery] string? tenantId,
        CancellationToken cancellationToken)
    {
        var stats = await _migrationService.GetMigrationStatisticsAsync(tenantId, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Deletes a migration plan.
    /// </summary>
    [HttpDelete("plans/{planId}")]
    public async Task<IActionResult> DeleteMigrationPlan(
        string planId,
        CancellationToken cancellationToken)
    {
        await _migrationService.DeleteMigrationPlanAsync(planId, cancellationToken);
        return NoContent();
    }

    private int GetTenantId()
    {
        // In production, extract from claims or tenant resolver
        var claim = User.FindFirst("tenant_id");
        if (claim != null && int.TryParse(claim.Value, out var tenantId))
        {
            return tenantId;
        }
        return 1; // Default tenant for development
    }

    private string GetUserId()
    {
        return User.FindFirst("sub")?.Value
            ?? User.FindFirst("user_id")?.Value
            ?? "system";
    }
}

public class GenerateMappingsRequest
{
    public string SourceDefinitionId { get; set; } = string.Empty;
    public string TargetDefinitionId { get; set; } = string.Empty;
}
