using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Controller for laboratory management operations.
/// Handles lab tests, orders, results, and external labs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LaboratoryController : BaseApiController
{
    private readonly ILabService _labService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LaboratoryController> _logger;

    public LaboratoryController(
        ILabService labService,
        ICurrentUserService currentUserService,
        ILogger<LaboratoryController> logger)
    {
        _labService = labService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Lab Tests

    /// <summary>
    /// Get all lab tests for the current branch.
    /// </summary>
    [HttpGet("tests")]
    public async Task<IActionResult>> GetTests(
        [FromQuery] LabTestListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var tests = await _labService.GetLabTestsByBranchIdAsync(branchId.Value);
            var filteredTests = tests.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm;
                filteredTests = filteredTests.Where(t =>
                    t.TestCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    t.TestName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (request.Category.HasValue)
            {
                filteredTests = filteredTests.Where(t => t.Category == request.Category.Value);
            }

            if (request.IsActive.HasValue)
            {
                filteredTests = filteredTests.Where(t => t.IsActive == request.IsActive.Value);
            }

            if (request.RequiresFasting.HasValue)
            {
                filteredTests = filteredTests.Where(t => t.RequiresFasting == request.RequiresFasting.Value);
            }

            if (request.ExternalLabId.HasValue)
            {
                filteredTests = filteredTests.Where(t => t.ExternalLabId == request.ExternalLabId.Value);
            }

            var totalCount = filteredTests.Count();

            filteredTests = request.SortBy?.ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? filteredTests.OrderByDescending(t => t.TestName) : filteredTests.OrderBy(t => t.TestName),
                "code" => request.SortDescending ? filteredTests.OrderByDescending(t => t.TestCode) : filteredTests.OrderBy(t => t.TestCode),
                "price" => request.SortDescending ? filteredTests.OrderByDescending(t => t.Price) : filteredTests.OrderBy(t => t.Price),
                "category" => request.SortDescending ? filteredTests.OrderByDescending(t => t.Category) : filteredTests.OrderBy(t => t.Category),
                _ => filteredTests.OrderBy(t => t.TestName)
            };

            var pagedTests = filteredTests
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(MapToLabTestDto)
                .ToList();

            return ApiPaginated(pagedTests, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab tests");
            return ApiServerError("Failed to retrieve lab tests");
        }
    }

    /// <summary>
    /// Get lab test by ID.
    /// </summary>
    [HttpGet("tests/{id}")]
    public async Task<IActionResult> GetTest(int id)
    {
        try
        {
            var test = await _labService.GetLabTestByIdAsync(id);
            if (test == null)
            {
                return ApiNotFound("Lab test not found");
            }

            if (!HasBranchAccess(test.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab tests");
            }

            return ApiOk(MapToLabTestDto(test));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab test {Id}", id);
            return ApiServerError("Failed to retrieve lab test");
        }
    }

    /// <summary>
    /// Get active lab tests.
    /// </summary>
    [HttpGet("tests/active")]
    public async Task<IActionResult>> GetActiveTests()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var tests = await _labService.GetActiveLabTestsAsync(branchId.Value);
            return ApiOk(tests.Select(MapToLabTestDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active lab tests");
            return ApiServerError("Failed to retrieve active lab tests");
        }
    }

    /// <summary>
    /// Get lab tests by category.
    /// </summary>
    [HttpGet("tests/category/{category}")]
    public async Task<IActionResult>> GetTestsByCategory(TestCategory category)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var tests = await _labService.GetLabTestsByCategoryAsync(branchId.Value, category);
            return ApiOk(tests.Select(MapToLabTestDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab tests by category {Category}", category);
            return ApiServerError("Failed to retrieve lab tests");
        }
    }

    /// <summary>
    /// Create a new lab test.
    /// </summary>
    [HttpPost("tests")]
    public async Task<IActionResult> CreateTest([FromBody] CreateLabTestDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var existingTest = await _labService.GetLabTestByCodeAsync(dto.TestCode, branchId.Value);
            if (existingTest != null)
            {
                return ApiConflict($"Test with code '{dto.TestCode}' already exists");
            }

            var test = new LabTest
            {
                TestCode = dto.TestCode,
                TestName = dto.TestName,
                Description = dto.Description,
                Category = dto.Category,
                SpecimenType = dto.SpecimenType,
                SpecimenVolume = dto.SpecimenVolume,
                TurnaroundTimeHours = dto.TurnaroundTimeHours,
                Price = dto.Price,
                Unit = dto.Unit,
                ReferenceRange = dto.ReferenceRange,
                Methodology = dto.Methodology,
                RequiresFasting = dto.RequiresFasting,
                PreparationInstructions = dto.PreparationInstructions,
                ExternalLabId = dto.ExternalLabId,
                BranchId = branchId.Value,
                IsActive = true,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var createdTest = await _labService.CreateLabTestAsync(test);

            _logger.LogInformation("Lab test created: {TestCode} by {UserId}",
                test.TestCode, _currentUserService.UserId);

            return ApiCreated(MapToLabTestDto(createdTest), message: "Lab test created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab test");
            return ApiServerError("Failed to create lab test");
        }
    }

    /// <summary>
    /// Update an existing lab test.
    /// </summary>
    [HttpPut("tests/{id}")]
    public async Task<IActionResult> UpdateTest(int id, [FromBody] UpdateLabTestDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var test = await _labService.GetLabTestByIdAsync(id);
            if (test == null)
            {
                return ApiNotFound("Lab test not found");
            }

            if (!HasBranchAccess(test.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab tests");
            }

            test.TestCode = dto.TestCode;
            test.TestName = dto.TestName;
            test.Description = dto.Description;
            test.Category = dto.Category;
            test.SpecimenType = dto.SpecimenType;
            test.SpecimenVolume = dto.SpecimenVolume;
            test.TurnaroundTimeHours = dto.TurnaroundTimeHours;
            test.Price = dto.Price;
            test.Unit = dto.Unit;
            test.ReferenceRange = dto.ReferenceRange;
            test.Methodology = dto.Methodology;
            test.IsActive = dto.IsActive;
            test.RequiresFasting = dto.RequiresFasting;
            test.PreparationInstructions = dto.PreparationInstructions;
            test.ExternalLabId = dto.ExternalLabId;
            test.UpdatedAt = DateTime.UtcNow;
            test.UpdatedBy = _currentUserService.UserId;

            await _labService.UpdateLabTestAsync(test);

            _logger.LogInformation("Lab test updated: {TestCode} by {UserId}",
                test.TestCode, _currentUserService.UserId);

            return ApiOk(MapToLabTestDto(test), "Lab test updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab test {Id}", id);
            return ApiServerError("Failed to update lab test");
        }
    }

    /// <summary>
    /// Delete a lab test.
    /// </summary>
    [HttpDelete("tests/{id}")]
    public async Task<IActionResult> DeleteTest(int id)
    {
        try
        {
            var test = await _labService.GetLabTestByIdAsync(id);
            if (test == null)
            {
                return ApiNotFound("Lab test not found");
            }

            if (!HasBranchAccess(test.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab tests");
            }

            await _labService.DeleteLabTestAsync(id);

            _logger.LogInformation("Lab test deleted: {TestCode} by {UserId}",
                test.TestCode, _currentUserService.UserId);

            return ApiOk("Lab test deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab test {Id}", id);
            return ApiServerError("Failed to delete lab test");
        }
    }

    #endregion

    #region Lab Orders

    /// <summary>
    /// Get all lab orders for the current branch.
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult>> GetOrders(
        [FromQuery] LabOrderListRequestDto request)
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orders = await _labService.GetLabOrdersByBranchIdAsync(branchId.Value);
            var filteredOrders = orders.AsQueryable();

            if (request.PatientId.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.PatientId == request.PatientId.Value);
            }

            if (request.Status.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.Status == request.Status.Value);
            }

            if (request.IsUrgent.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.IsUrgent == request.IsUrgent.Value);
            }

            if (request.IsPaid.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.IsPaid == request.IsPaid.Value);
            }

            if (request.ExternalLabId.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.ExternalLabId == request.ExternalLabId.Value);
            }

            if (request.DateFrom.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate >= request.DateFrom.Value);
            }

            if (request.DateTo.HasValue)
            {
                filteredOrders = filteredOrders.Where(o => o.OrderDate <= request.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm;
                filteredOrders = filteredOrders.Where(o =>
                    o.OrderNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (o.Patient != null && o.Patient.FullNameEn.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            var totalCount = filteredOrders.Count();

            filteredOrders = request.SortBy?.ToLowerInvariant() switch
            {
                "ordernumber" => request.SortDescending ? filteredOrders.OrderByDescending(o => o.OrderNumber) : filteredOrders.OrderBy(o => o.OrderNumber),
                "status" => request.SortDescending ? filteredOrders.OrderByDescending(o => o.Status) : filteredOrders.OrderBy(o => o.Status),
                "amount" => request.SortDescending ? filteredOrders.OrderByDescending(o => o.TotalAmount) : filteredOrders.OrderBy(o => o.TotalAmount),
                _ => request.SortDescending ? filteredOrders.OrderByDescending(o => o.OrderDate) : filteredOrders.OrderBy(o => o.OrderDate)
            };

            var pagedOrders = filteredOrders
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(MapToLabOrderDto)
                .ToList();

            return ApiPaginated(pagedOrders, totalCount, request.PageNumber, request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab orders");
            return ApiServerError("Failed to retrieve lab orders");
        }
    }

    /// <summary>
    /// Get lab order by ID.
    /// </summary>
    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            var order = await _labService.GetLabOrderByIdAsync(id);
            if (order == null)
            {
                return ApiNotFound("Lab order not found");
            }

            if (!HasBranchAccess(order.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab orders");
            }

            return ApiOk(MapToLabOrderDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab order {Id}", id);
            return ApiServerError("Failed to retrieve lab order");
        }
    }

    /// <summary>
    /// Get pending lab orders.
    /// </summary>
    [HttpGet("orders/pending")]
    public async Task<IActionResult>> GetPendingOrders()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orders = await _labService.GetPendingLabOrdersAsync(branchId.Value);
            return ApiOk(orders.Select(MapToLabOrderDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending lab orders");
            return ApiServerError("Failed to retrieve pending lab orders");
        }
    }

    /// <summary>
    /// Get urgent lab orders.
    /// </summary>
    [HttpGet("orders/urgent")]
    public async Task<IActionResult>> GetUrgentOrders()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orders = await _labService.GetUrgentLabOrdersAsync(branchId.Value);
            return ApiOk(orders.Select(MapToLabOrderDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting urgent lab orders");
            return ApiServerError("Failed to retrieve urgent lab orders");
        }
    }

    /// <summary>
    /// Get lab orders for a patient.
    /// </summary>
    [HttpGet("orders/patient/{patientId}")]
    public async Task<IActionResult>> GetPatientOrders(int patientId)
    {
        try
        {
            // SECURITY FIX: Require branch context for patient queries
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orders = await _labService.GetLabOrdersByPatientIdAsync(patientId);
            var filteredOrders = orders.Where(o => HasBranchAccess(o.BranchId)).ToList();

            // SECURITY FIX: Log if no accessible orders found (potential unauthorized access attempt)
            if (!filteredOrders.Any())
            {
                _logger.LogWarning("Patient lab orders query returned no accessible results: PatientId={PatientId}, UserId={UserId}",
                    patientId, _currentUserService.UserId);
            }

            return ApiOk(filteredOrders.Select(MapToLabOrderDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab orders for patient");
            return ApiServerError("Failed to retrieve patient lab orders");
        }
    }

    /// <summary>
    /// Create a new lab order.
    /// </summary>
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateLabOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orderNumber = await _labService.GenerateLabOrderNumberAsync(branchId.Value);
            decimal totalAmount = 0;

            var items = new List<LabOrderItem>();
            foreach (var itemDto in dto.Items)
            {
                var test = await _labService.GetLabTestByIdAsync(itemDto.LabTestId);
                if (test == null)
                {
                    return ApiBadRequest($"Lab test {itemDto.LabTestId} not found");
                }

                totalAmount += test.Price;
                items.Add(new LabOrderItem
                {
                    LabTestId = test.Id,
                    TestCode = test.TestCode,
                    TestName = test.TestName,
                    Price = test.Price,
                    Notes = itemDto.Notes,
                    CreatedBy = _currentUserService.UserId
                });
            }

            var order = new LabOrder
            {
                OrderNumber = orderNumber,
                OrderDate = DateTime.UtcNow,
                Status = LabOrderStatus.Pending,
                PatientId = dto.PatientId,
                BranchId = branchId.Value,
                ExternalLabId = dto.ExternalLabId,
                OrderedBy = _currentUserService.UserId,
                ExpectedCompletionDate = dto.ExpectedCompletionDate,
                TotalAmount = totalAmount,
                IsUrgent = dto.IsUrgent,
                ClinicalNotes = dto.ClinicalNotes,
                Notes = dto.Notes,
                Items = items,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var createdOrder = await _labService.CreateLabOrderAsync(order);

            _logger.LogInformation("Lab order created: {OrderNumber} by {UserId}",
                order.OrderNumber, _currentUserService.UserId);

            return ApiCreated(MapToLabOrderDto(createdOrder), message: "Lab order created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lab order");
            return ApiServerError("Failed to create lab order");
        }
    }

    /// <summary>
    /// Update lab order status.
    /// </summary>
    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateLabOrderStatusDto dto)
    {
        try
        {
            if (id != dto.LabOrderId)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var order = await _labService.GetLabOrderByIdAsync(id);
            if (order == null)
            {
                return ApiNotFound("Lab order not found");
            }

            if (!HasBranchAccess(order.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab orders");
            }

            await _labService.UpdateLabOrderStatusAsync(id, dto.Status, _currentUserService.UserId ?? string.Empty);

            _logger.LogInformation("Lab order status updated: {OrderNumber} to {Status} by {UserId}",
                order.OrderNumber, dto.Status, _currentUserService.UserId);

            return ApiOk("Lab order status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lab order status {Id}", id);
            return ApiServerError("Failed to update lab order status");
        }
    }

    /// <summary>
    /// Collect samples for a lab order.
    /// </summary>
    [HttpPost("orders/{id}/collect")]
    public async Task<IActionResult> CollectSamples(int id, [FromBody] CollectSamplesDto dto)
    {
        try
        {
            if (id != dto.LabOrderId)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var order = await _labService.GetLabOrderByIdAsync(id);
            if (order == null)
            {
                return ApiNotFound("Lab order not found");
            }

            if (!HasBranchAccess(order.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab orders");
            }

            if (order.Status != LabOrderStatus.Pending)
            {
                return ApiBadRequest("Samples can only be collected for pending orders");
            }

            order.CollectionDate = dto.CollectionDate;
            order.CollectedBy = _currentUserService.UserId;
            order.Status = LabOrderStatus.Collected;
            order.Notes = string.IsNullOrEmpty(order.Notes)
                ? dto.Notes
                : $"{order.Notes}\n{dto.Notes}";
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _currentUserService.UserId;

            await _labService.UpdateLabOrderAsync(order);

            _logger.LogInformation("Samples collected for order: {OrderNumber} by {UserId}",
                order.OrderNumber, _currentUserService.UserId);

            return ApiOk("Samples collected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting samples for order {Id}", id);
            return ApiServerError("Failed to collect samples");
        }
    }

    /// <summary>
    /// Delete a lab order.
    /// </summary>
    [HttpDelete("orders/{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        try
        {
            var order = await _labService.GetLabOrderByIdAsync(id);
            if (order == null)
            {
                return ApiNotFound("Lab order not found");
            }

            if (!HasBranchAccess(order.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab orders");
            }

            if (order.Status != LabOrderStatus.Pending)
            {
                return ApiBadRequest("Only pending orders can be deleted");
            }

            await _labService.DeleteLabOrderAsync(id);

            _logger.LogInformation("Lab order deleted: {OrderNumber} by {UserId}",
                order.OrderNumber, _currentUserService.UserId);

            return ApiOk("Lab order deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lab order {Id}", id);
            return ApiServerError("Failed to delete lab order");
        }
    }

    #endregion

    #region Lab Results

    /// <summary>
    /// Get lab results for an order.
    /// </summary>
    [HttpGet("orders/{orderId}/results")]
    public async Task<IActionResult>> GetOrderResults(int orderId)
    {
        try
        {
            var order = await _labService.GetLabOrderByIdAsync(orderId);
            if (order == null)
            {
                return ApiNotFound("Lab order not found");
            }

            if (!HasBranchAccess(order.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab orders");
            }

            var results = await _labService.GetLabResultsByOrderIdAsync(orderId);
            return ApiOk(results.Select(MapToLabResultDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab results for order {OrderId}", orderId);
            return ApiServerError("Failed to retrieve lab results");
        }
    }

    /// <summary>
    /// Get lab result by ID.
    /// </summary>
    [HttpGet("results/{id}")]
    public async Task<IActionResult> GetResult(int id)
    {
        try
        {
            var result = await _labService.GetLabResultByIdAsync(id);
            if (result == null)
            {
                return ApiNotFound("Lab result not found");
            }

            if (!HasBranchAccess(result.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab results");
            }

            return ApiOk(MapToLabResultDto(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lab result {Id}", id);
            return ApiServerError("Failed to retrieve lab result");
        }
    }

    /// <summary>
    /// Enter lab result.
    /// </summary>
    [HttpPost("results")]
    public async Task<IActionResult> EnterResult([FromBody] EnterLabResultDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            // SECURITY FIX: Validate the attachment path to prevent path traversal attacks
            if (!string.IsNullOrEmpty(dto.AttachmentPath))
            {
                // Reject paths containing traversal patterns
                if (dto.AttachmentPath.Contains("..") ||
                    dto.AttachmentPath.Contains("://") ||
                    Path.IsPathRooted(dto.AttachmentPath))
                {
                    _logger.LogWarning("Potential path traversal attempt in lab result attachment: Path={Path}, UserId={UserId}",
                        dto.AttachmentPath, _currentUserService.UserId);
                    return ApiBadRequest("Invalid attachment path");
                }

                // Only allow alphanumeric, dash, underscore, dot, and forward slash
                var invalidChars = dto.AttachmentPath.Where(c =>
                    !char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.' && c != '/').ToList();
                if (invalidChars.Any())
                {
                    return ApiBadRequest("Attachment path contains invalid characters");
                }
            }

            var result = new LabResult
            {
                BranchId = branchId.Value,
                LabOrderItemId = dto.LabOrderItemId,
                Status = LabResultStatus.Completed,
                ResultDate = DateTime.UtcNow,
                ResultValue = dto.ResultValue,
                Unit = dto.Unit,
                ReferenceRange = dto.ReferenceRange,
                IsAbnormal = dto.IsAbnormal,
                Interpretation = dto.Interpretation,
                Notes = dto.Notes,
                AttachmentPath = dto.AttachmentPath,
                PerformedBy = _currentUserService.UserId,
                PerformedDate = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var createdResult = await _labService.CreateLabResultAsync(result);

            _logger.LogInformation("Lab result entered for order item {OrderItemId} by {UserId}",
                dto.LabOrderItemId, _currentUserService.UserId);

            return ApiCreated(MapToLabResultDto(createdResult), message: "Lab result entered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error entering lab result");
            return ApiServerError("Failed to enter lab result");
        }
    }

    /// <summary>
    /// Review lab result.
    /// </summary>
    [HttpPost("results/{id}/review")]
    public async Task<IActionResult> ReviewResult(int id, [FromBody] ReviewLabResultDto dto)
    {
        try
        {
            if (id != dto.LabResultId)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var result = await _labService.GetLabResultByIdAsync(id);
            if (result == null)
            {
                return ApiNotFound("Lab result not found");
            }

            if (!HasBranchAccess(result.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab results");
            }

            result.Status = LabResultStatus.Reviewed;
            result.IsAbnormal = dto.IsAbnormal;
            result.Interpretation = dto.Interpretation;
            result.Notes = dto.Notes;
            result.ReviewedBy = _currentUserService.UserId;
            result.ReviewedDate = DateTime.UtcNow;
            result.UpdatedAt = DateTime.UtcNow;
            result.UpdatedBy = _currentUserService.UserId;

            await _labService.UpdateLabResultAsync(result);

            _logger.LogInformation("Lab result reviewed: {ResultId} by {UserId}",
                id, _currentUserService.UserId);

            return ApiOk("Lab result reviewed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing lab result {Id}", id);
            return ApiServerError("Failed to review lab result");
        }
    }

    /// <summary>
    /// Verify lab result.
    /// </summary>
    [HttpPost("results/{id}/verify")]
    public async Task<IActionResult> VerifyResult(int id, [FromBody] VerifyLabResultDto dto)
    {
        try
        {
            if (id != dto.LabResultId)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var result = await _labService.GetLabResultByIdAsync(id);
            if (result == null)
            {
                return ApiNotFound("Lab result not found");
            }

            if (!HasBranchAccess(result.BranchId))
            {
                return ApiForbidden("Access denied to this branch's lab results");
            }

            if (result.Status != LabResultStatus.Reviewed)
            {
                return ApiBadRequest("Only reviewed results can be verified");
            }

            result.Status = LabResultStatus.Verified;
            result.Notes = string.IsNullOrEmpty(result.Notes)
                ? dto.Notes
                : $"{result.Notes}\n{dto.Notes}";
            result.VerifiedBy = _currentUserService.UserId;
            result.VerifiedDate = DateTime.UtcNow;
            result.UpdatedAt = DateTime.UtcNow;
            result.UpdatedBy = _currentUserService.UserId;

            await _labService.UpdateLabResultAsync(result);

            _logger.LogInformation("Lab result verified: {ResultId} by {UserId}",
                id, _currentUserService.UserId);

            return ApiOk("Lab result verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying lab result {Id}", id);
            return ApiServerError("Failed to verify lab result");
        }
    }

    #endregion

    #region External Labs

    /// <summary>
    /// Get all external labs for the current branch.
    /// </summary>
    [HttpGet("external-labs")]
    public async Task<IActionResult>> GetExternalLabs()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var labs = await _labService.GetExternalLabsByBranchIdAsync(branchId.Value);
            return ApiOk(labs.Select(MapToExternalLabDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external labs");
            return ApiServerError("Failed to retrieve external labs");
        }
    }

    /// <summary>
    /// Get active external labs.
    /// </summary>
    [HttpGet("external-labs/active")]
    public async Task<IActionResult>> GetActiveExternalLabs()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var labs = await _labService.GetActiveExternalLabsAsync(branchId.Value);
            return ApiOk(labs.Select(MapToExternalLabDto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active external labs");
            return ApiServerError("Failed to retrieve active external labs");
        }
    }

    /// <summary>
    /// Get external lab by ID.
    /// </summary>
    [HttpGet("external-labs/{id}")]
    public async Task<IActionResult> GetExternalLab(int id)
    {
        try
        {
            var lab = await _labService.GetExternalLabByIdAsync(id);
            if (lab == null)
            {
                return ApiNotFound("External lab not found");
            }

            if (!HasBranchAccess(lab.BranchId))
            {
                return ApiForbidden("Access denied to this branch's external labs");
            }

            return ApiOk(MapToExternalLabDto(lab));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external lab {Id}", id);
            return ApiServerError("Failed to retrieve external lab");
        }
    }

    /// <summary>
    /// Create a new external lab.
    /// </summary>
    [HttpPost("external-labs")]
    public async Task<IActionResult> CreateExternalLab([FromBody] CreateExternalLabDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var lab = new ExternalLab
            {
                Name = dto.Name,
                Code = dto.Code,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                Website = dto.Website,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpiryDate = dto.LicenseExpiryDate,
                TurnaroundTimeDays = dto.TurnaroundTimeDays,
                Notes = dto.Notes,
                BranchId = branchId.Value,
                IsActive = true,
                CreatedBy = _currentUserService.UserId ?? string.Empty
            };

            var createdLab = await _labService.CreateExternalLabAsync(lab);

            _logger.LogInformation("External lab created: {Name} by {UserId}",
                lab.Name, _currentUserService.UserId);

            return ApiCreated(MapToExternalLabDto(createdLab), message: "External lab created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating external lab");
            return ApiServerError("Failed to create external lab");
        }
    }

    /// <summary>
    /// Update an existing external lab.
    /// </summary>
    [HttpPut("external-labs/{id}")]
    public async Task<IActionResult> UpdateExternalLab(int id, [FromBody] UpdateExternalLabDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return ApiBadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return ApiBadRequestFromModelState();
            }

            var lab = await _labService.GetExternalLabByIdAsync(id);
            if (lab == null)
            {
                return ApiNotFound("External lab not found");
            }

            if (!HasBranchAccess(lab.BranchId))
            {
                return ApiForbidden("Access denied to this branch's external labs");
            }

            lab.Name = dto.Name;
            lab.Code = dto.Code;
            lab.ContactPerson = dto.ContactPerson;
            lab.Email = dto.Email;
            lab.Phone = dto.Phone;
            lab.Mobile = dto.Mobile;
            lab.Address = dto.Address;
            lab.City = dto.City;
            lab.Country = dto.Country;
            lab.Website = dto.Website;
            lab.LicenseNumber = dto.LicenseNumber;
            lab.LicenseExpiryDate = dto.LicenseExpiryDate;
            lab.TurnaroundTimeDays = dto.TurnaroundTimeDays;
            lab.IsActive = dto.IsActive;
            lab.Notes = dto.Notes;
            lab.UpdatedAt = DateTime.UtcNow;
            lab.UpdatedBy = _currentUserService.UserId;

            await _labService.UpdateExternalLabAsync(lab);

            _logger.LogInformation("External lab updated: {Name} by {UserId}",
                lab.Name, _currentUserService.UserId);

            return ApiOk(MapToExternalLabDto(lab), "External lab updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating external lab {Id}", id);
            return ApiServerError("Failed to update external lab");
        }
    }

    /// <summary>
    /// Delete an external lab.
    /// </summary>
    [HttpDelete("external-labs/{id}")]
    public async Task<IActionResult> DeleteExternalLab(int id)
    {
        try
        {
            var lab = await _labService.GetExternalLabByIdAsync(id);
            if (lab == null)
            {
                return ApiNotFound("External lab not found");
            }

            if (!HasBranchAccess(lab.BranchId))
            {
                return ApiForbidden("Access denied to this branch's external labs");
            }

            await _labService.DeleteExternalLabAsync(id);

            _logger.LogInformation("External lab deleted: {Name} by {UserId}",
                lab.Name, _currentUserService.UserId);

            return ApiOk("External lab deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting external lab {Id}", id);
            return ApiServerError("Failed to delete external lab");
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get laboratory statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var branchId = _currentUserService.BranchId;
            if (branchId == null)
            {
                return ApiBadRequest("Branch context is required");
            }

            var orders = await _labService.GetLabOrdersByBranchIdAsync(branchId.Value);
            var ordersList = orders.ToList();

            var tests = await _labService.GetLabTestsByBranchIdAsync(branchId.Value);
            var testsList = tests.ToList();

            var externalLabs = await _labService.GetExternalLabsByBranchIdAsync(branchId.Value);
            var labsList = externalLabs.ToList();

            var pendingCount = await _labService.GetPendingOrdersCountAsync(branchId.Value);
            var urgentCount = await _labService.GetUrgentOrdersCountAsync(branchId.Value);
            var orderStatusDistribution = await _labService.GetOrderStatusDistributionAsync(branchId.Value);

            var statistics = new LabStatisticsDto
            {
                TotalOrders = ordersList.Count,
                PendingOrders = pendingCount,
                InProgressOrders = ordersList.Count(o => o.Status == LabOrderStatus.InProgress),
                CompletedOrders = ordersList.Count(o => o.Status == LabOrderStatus.Completed),
                CancelledOrders = ordersList.Count(o => o.Status == LabOrderStatus.Cancelled),
                UrgentOrders = urgentCount,
                PaidOrders = ordersList.Count(o => o.IsPaid),
                UnpaidOrders = ordersList.Count(o => !o.IsPaid),
                TotalRevenue = ordersList.Where(o => o.IsPaid).Sum(o => o.TotalAmount),
                PendingRevenue = ordersList.Where(o => !o.IsPaid).Sum(o => o.TotalAmount),
                AverageOrderValue = ordersList.Count > 0 ? ordersList.Average(o => o.TotalAmount) : 0,
                TotalTests = testsList.Count,
                ActiveTests = testsList.Count(t => t.IsActive),
                TotalExternalLabs = labsList.Count,
                ActiveExternalLabs = labsList.Count(l => l.IsActive),
                OrdersByStatus = orderStatusDistribution,
                TestsByCategory = testsList.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count())
            };

            return ApiOk(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting laboratory statistics");
            return ApiServerError("Failed to retrieve laboratory statistics");
        }
    }

    #endregion

    #region Helper Methods

    private bool HasBranchAccess(int branchId)
    {
        return _currentUserService.BranchId == branchId ||
               _currentUserService.HasRole("SuperAdmin");
    }

    private static LabTestDto MapToLabTestDto(LabTest test)
    {
        return new LabTestDto
        {
            Id = test.Id,
            TestCode = test.TestCode,
            TestName = test.TestName,
            Description = test.Description,
            Category = test.Category,
            SpecimenType = test.SpecimenType,
            SpecimenVolume = test.SpecimenVolume,
            TurnaroundTimeHours = test.TurnaroundTimeHours,
            Price = test.Price,
            Unit = test.Unit,
            ReferenceRange = test.ReferenceRange,
            Methodology = test.Methodology,
            IsActive = test.IsActive,
            RequiresFasting = test.RequiresFasting,
            PreparationInstructions = test.PreparationInstructions,
            ExternalLabId = test.ExternalLabId,
            ExternalLabName = test.ExternalLab?.Name,
            BranchId = test.BranchId,
            CreatedAt = test.CreatedAt,
            CreatedBy = test.CreatedBy,
            UpdatedAt = test.UpdatedAt,
            UpdatedBy = test.UpdatedBy
        };
    }

    private static LabOrderDto MapToLabOrderDto(LabOrder order)
    {
        return new LabOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PatientId = order.PatientId,
            PatientName = order.Patient?.FullNameEn,
            PatientMRN = order.Patient?.MRN,
            BranchId = order.BranchId,
            BranchName = order.Branch?.Name,
            ExternalLabId = order.ExternalLabId,
            ExternalLabName = order.ExternalLab?.Name,
            OrderedBy = order.OrderedBy,
            CollectionDate = order.CollectionDate,
            CollectedBy = order.CollectedBy,
            ExpectedCompletionDate = order.ExpectedCompletionDate,
            CompletedDate = order.CompletedDate,
            TotalAmount = order.TotalAmount,
            IsPaid = order.IsPaid,
            IsUrgent = order.IsUrgent,
            ClinicalNotes = order.ClinicalNotes,
            Notes = order.Notes,
            ItemCount = order.Items.Count,
            Items = order.Items.Select(MapToLabOrderItemDto).ToList(),
            Results = order.Results.Select(MapToLabResultDto).ToList(),
            CreatedAt = order.CreatedAt,
            CreatedBy = order.CreatedBy,
            UpdatedAt = order.UpdatedAt,
            UpdatedBy = order.UpdatedBy
        };
    }

    private static LabOrderItemDto MapToLabOrderItemDto(LabOrderItem item)
    {
        return new LabOrderItemDto
        {
            Id = item.Id,
            LabOrderId = item.LabOrderId,
            LabTestId = item.LabTestId,
            TestCode = item.TestCode,
            TestName = item.TestName,
            Price = item.Price,
            Notes = item.Notes
        };
    }

    private static LabResultDto MapToLabResultDto(LabResult result)
    {
        return new LabResultDto
        {
            Id = result.Id,
            BranchId = result.BranchId,
            LabOrderId = result.LabOrderId,
            OrderNumber = result.LabOrder?.OrderNumber,
            LabOrderItemId = result.LabOrderItemId,
            LabTestId = result.LabTestId,
            TestCode = result.LabTest?.TestCode,
            TestName = result.LabTest?.TestName,
            Status = result.Status,
            ResultDate = result.ResultDate,
            ResultValue = result.ResultValue,
            Unit = result.Unit,
            ReferenceRange = result.ReferenceRange,
            IsAbnormal = result.IsAbnormal,
            Interpretation = result.Interpretation,
            Notes = result.Notes,
            AttachmentPath = result.AttachmentPath,
            PerformedBy = result.PerformedBy,
            PerformedDate = result.PerformedDate,
            ReviewedBy = result.ReviewedBy,
            ReviewedDate = result.ReviewedDate,
            VerifiedBy = result.VerifiedBy,
            VerifiedDate = result.VerifiedDate,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    private static ExternalLabDto MapToExternalLabDto(ExternalLab lab)
    {
        return new ExternalLabDto
        {
            Id = lab.Id,
            Name = lab.Name,
            Code = lab.Code,
            ContactPerson = lab.ContactPerson,
            Email = lab.Email,
            Phone = lab.Phone,
            Mobile = lab.Mobile,
            Address = lab.Address,
            City = lab.City,
            Country = lab.Country,
            Website = lab.Website,
            LicenseNumber = lab.LicenseNumber,
            LicenseExpiryDate = lab.LicenseExpiryDate,
            TurnaroundTimeDays = lab.TurnaroundTimeDays,
            IsActive = lab.IsActive,
            Notes = lab.Notes,
            BranchId = lab.BranchId,
            CreatedAt = lab.CreatedAt,
            CreatedBy = lab.CreatedBy,
            UpdatedAt = lab.UpdatedAt,
            UpdatedBy = lab.UpdatedBy
        };
    }

    #endregion
}
