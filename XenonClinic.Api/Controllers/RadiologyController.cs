using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Controller for Radiology/Imaging management operations.
/// Provides endpoints for imaging studies, radiology orders, and imaging results.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RadiologyController : BaseApiController
{
    private readonly IRadiologyService _radiologyService;
    private readonly ICurrentUserService _currentUserService;

    public RadiologyController(
        IRadiologyService radiologyService,
        ICurrentUserService currentUserService)
    {
        _radiologyService = radiologyService;
        _currentUserService = currentUserService;
    }

    #region Imaging Study Endpoints

    /// <summary>
    /// Gets all imaging studies for the current branch.
    /// </summary>
    [HttpGet("studies")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ImagingStudyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImagingStudies()
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var studies = await _radiologyService.GetImagingStudiesByBranchIdAsync(branchId.Value);

        var studyDtos = studies.Select(MapToImagingStudyDto).ToList();
        return ApiOk(studyDtos);
    }

    /// <summary>
    /// Gets active imaging studies for the current branch.
    /// </summary>
    [HttpGet("studies/active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ImagingStudyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveImagingStudies()
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var studies = await _radiologyService.GetActiveImagingStudiesAsync(branchId.Value);

        var studyDtos = studies.Select(MapToImagingStudyDto).ToList();
        return ApiOk(studyDtos);
    }

    /// <summary>
    /// Gets imaging studies by category/modality.
    /// </summary>
    [HttpGet("studies/by-modality/{modality}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ImagingStudyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImagingStudiesByModality(string modality)
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var studies = await _radiologyService.GetImagingStudiesByCategoryAsync(branchId.Value, modality);

        var studyDtos = studies.Select(MapToImagingStudyDto).ToList();
        return ApiOk(studyDtos);
    }

    /// <summary>
    /// Gets a specific imaging study by ID.
    /// </summary>
    [HttpGet("studies/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ImagingStudyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImagingStudyById(int id)
    {
        var study = await _radiologyService.GetImagingStudyByIdAsync(id);
        if (study == null)
            return ApiNotFound("Imaging study not found");

        return ApiOk(MapToImagingStudyDto(study));
    }

    /// <summary>
    /// Gets an imaging study by code.
    /// </summary>
    [HttpGet("studies/by-code/{studyCode}")]
    [ProducesResponseType(typeof(ApiResponse<ImagingStudyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImagingStudyByCode(string studyCode)
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var study = await _radiologyService.GetImagingStudyByCodeAsync(studyCode, branchId.Value);
        if (study == null)
            return ApiNotFound("Imaging study not found");

        return ApiOk(MapToImagingStudyDto(study));
    }

    /// <summary>
    /// Creates a new imaging study.
    /// </summary>
    [HttpPost("studies")]
    [ProducesResponseType(typeof(ApiResponse<ImagingStudyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateImagingStudy([FromBody] CreateImagingStudyDto dto)
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var study = new LabTest
        {
            TestCode = dto.StudyCode,
            TestName = dto.StudyName,
            Description = $"{dto.Modality}: {dto.Description}",
            Category = TestCategory.Imaging,
            Price = dto.Price,
            TurnaroundTimeHours = dto.EstimatedDurationMinutes / 60,
            RequiresFasting = dto.RequiresFasting,
            PreparationInstructions = dto.PatientPreparation,
            BranchId = branchId.Value,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId ?? "system"
        };

        var created = await _radiologyService.CreateImagingStudyAsync(study);
        return ApiCreated(MapToImagingStudyDto(created), "Imaging study created successfully");
    }

    /// <summary>
    /// Updates an existing imaging study.
    /// </summary>
    [HttpPut("studies/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ImagingStudyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateImagingStudy(int id, [FromBody] UpdateImagingStudyDto dto)
    {
        if (id != dto.Id)
            return ApiBadRequest("ID mismatch");

        var study = await _radiologyService.GetImagingStudyByIdAsync(id);
        if (study == null)
            return ApiNotFound("Imaging study not found");

        study.TestCode = dto.StudyCode;
        study.TestName = dto.StudyName;
        study.Description = $"{dto.Modality}: {dto.Description}";
        study.Category = TestCategory.Imaging;
        study.Price = dto.Price;
        study.TurnaroundTimeHours = dto.EstimatedDurationMinutes / 60;
        study.RequiresFasting = dto.RequiresFasting;
        study.PreparationInstructions = dto.PatientPreparation;
        study.IsActive = dto.IsActive;
        study.UpdatedAt = DateTime.UtcNow;
        study.UpdatedBy = _currentUserService.UserId;

        await _radiologyService.UpdateImagingStudyAsync(study);
        return ApiOk(MapToImagingStudyDto(study), "Imaging study updated successfully");
    }

    /// <summary>
    /// Deletes an imaging study.
    /// </summary>
    [HttpDelete("studies/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImagingStudy(int id)
    {
        var study = await _radiologyService.GetImagingStudyByIdAsync(id);
        if (study == null)
            return ApiNotFound("Imaging study not found");

        await _radiologyService.DeleteImagingStudyAsync(id);
        return ApiOk(true, "Imaging study deleted successfully");
    }

    #endregion

    #region Radiology Order Endpoints

    /// <summary>
    /// Gets all radiology orders for the current branch.
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RadiologyOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRadiologyOrders()
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var orders = await _radiologyService.GetRadiologyOrdersByBranchIdAsync(branchId.Value);

        var orderDtos = orders.Select(MapToRadiologyOrderDto).ToList();
        return ApiOk(orderDtos);
    }

    /// <summary>
    /// Gets pending radiology orders for the current branch.
    /// </summary>
    [HttpGet("orders/pending")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RadiologyOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingRadiologyOrders()
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var orders = await _radiologyService.GetPendingRadiologyOrdersAsync(branchId.Value);

        var orderDtos = orders.Select(MapToRadiologyOrderDto).ToList();
        return ApiOk(orderDtos);
    }

    /// <summary>
    /// Gets completed radiology orders for the current branch.
    /// </summary>
    [HttpGet("orders/completed")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RadiologyOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCompletedRadiologyOrders()
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var orders = await _radiologyService.GetCompletedRadiologyOrdersAsync(branchId.Value);

        var orderDtos = orders.Select(MapToRadiologyOrderDto).ToList();
        return ApiOk(orderDtos);
    }

    /// <summary>
    /// Gets radiology orders for a specific patient.
    /// </summary>
    [HttpGet("orders/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RadiologyOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRadiologyOrdersByPatient(int patientId)
    {
        var orders = await _radiologyService.GetRadiologyOrdersByPatientIdAsync(patientId);

        var orderDtos = orders.Select(MapToRadiologyOrderDto).ToList();
        return ApiOk(orderDtos);
    }

    /// <summary>
    /// Gets radiology orders within a date range.
    /// </summary>
    [HttpGet("orders/date-range")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RadiologyOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRadiologyOrdersByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var branchId = _currentUserService.BranchId;
        if (branchId == null)
            return ApiBadRequest("Branch context is required");

        var orders = await _radiologyService.GetRadiologyOrdersByDateRangeAsync(branchId.Value, startDate, endDate);

        var orderDtos = orders.Select(MapToRadiologyOrderDto).ToList();
        return ApiOk(orderDtos);
    }

    /// <summary>
    /// Gets a specific radiology order by ID.
    /// </summary>
    [HttpGet("orders/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RadiologyOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRadiologyOrderById(int id)
    {
        var order = await _radiologyService.GetRadiologyOrderByIdAsync(id);
        if (order == null)
            return ApiNotFound("Radiology order not found");

        return ApiOk(MapToRadiologyOrderDto(order));
    }

    /// <summary>
    /// Gets a radiology order by order number.
    /// </summary>
    [HttpGet("orders/by-number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<RadiologyOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRadiologyOrderByNumber(string orderNumber)
    {
        var order = await _radiologyService.GetRadiologyOrderByOrderNumberAsync(orderNumber);
        if (order == null)
            return ApiNotFound("Radiology order not found");

        return ApiOk(MapToRadiologyOrderDto(order));
    }

    /// <summary>
    /// Creates a new radiology order.
    /// </summary>
    [HttpPost("orders")]
    [ProducesResponseType(typeof(ApiResponse<RadiologyOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRadiologyOrder([FromBody] CreateRadiologyOrderDto dto)
    {
        var branchId = _currentUserService.BranchId;
        var orderNumber = await _radiologyService.GenerateRadiologyOrderNumberAsync(branchId);

        var order = new LabOrder
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            Status = LabOrderStatus.Pending,
            PatientId = dto.PatientId,
            VisitId = dto.AppointmentId, // Using VisitId as appointment reference
            OrderingDoctorId = dto.ReferringDoctorId,
            IsUrgent = dto.IsUrgent || dto.IsStat,
            ClinicalNotes = dto.ClinicalIndication,
            Notes = dto.Notes,
            BranchId = branchId ?? 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId ?? string.Empty
        };

        // Add order items
        decimal total = 0;
        decimal discountAmount = 0;
        foreach (var itemDto in dto.Items)
        {
            var study = await _radiologyService.GetImagingStudyByIdAsync(itemDto.ImagingStudyId);
            if (study != null)
            {
                var price = study.Price - (itemDto.DiscountAmount ?? 0);
                total += price;

                order.Items.Add(new LabOrderItem
                {
                    LabTestId = itemDto.ImagingStudyId,
                    TestCode = study.TestCode,
                    TestName = study.TestName,
                    Price = price,
                    Notes = itemDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.UserId ?? string.Empty
                });
            }
        }

        // Apply order-level discount
        if (dto.DiscountPercentage.HasValue && dto.DiscountPercentage.Value > 0)
        {
            discountAmount = total * (dto.DiscountPercentage.Value / 100);
            total -= discountAmount;
        }

        order.TotalAmount = total;

        var created = await _radiologyService.CreateRadiologyOrderAsync(order);
        return ApiCreated(MapToRadiologyOrderDto(created), "Radiology order created successfully");
    }

    /// <summary>
    /// Updates an existing radiology order.
    /// </summary>
    [HttpPut("orders/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RadiologyOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRadiologyOrder(int id, [FromBody] UpdateRadiologyOrderDto dto)
    {
        if (id != dto.Id)
            return ApiBadRequest("ID mismatch");

        var order = await _radiologyService.GetRadiologyOrderByIdAsync(id);
        if (order == null)
            return ApiNotFound("Radiology order not found");

        order.OrderingDoctorId = dto.ReferringDoctorId;
        order.IsUrgent = dto.IsUrgent || dto.IsStat;
        order.ClinicalNotes = dto.ClinicalIndication;
        order.Notes = dto.Notes;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = _currentUserService.UserId;

        await _radiologyService.UpdateRadiologyOrderAsync(order);
        return ApiOk(MapToRadiologyOrderDto(order), "Radiology order updated successfully");
    }

    /// <summary>
    /// Deletes a radiology order.
    /// </summary>
    [HttpDelete("orders/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRadiologyOrder(int id)
    {
        var order = await _radiologyService.GetRadiologyOrderByIdAsync(id);
        if (order == null)
            return ApiNotFound("Radiology order not found");

        await _radiologyService.DeleteRadiologyOrderAsync(id);
        return ApiOk(true, "Radiology order deleted successfully");
    }

    #endregion

    #region Order Workflow Endpoints

    /// <summary>
    /// Receives a radiology order (patient checked in).
    /// </summary>
    [HttpPost("orders/{id:int}/receive")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReceiveRadiologyOrder(int id, [FromBody] ReceiveRadiologyOrderDto dto)
    {
        try
        {
            await _radiologyService.ReceiveRadiologyOrderAsync(id, _currentUserService.UserId);
            return ApiOk(true, "Radiology order received successfully");
        }
        catch (KeyNotFoundException)
        {
            return ApiNotFound("Radiology order not found");
        }
    }

    /// <summary>
    /// Starts imaging for a radiology order.
    /// </summary>
    [HttpPost("orders/{id:int}/start-imaging")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartImaging(int id, [FromBody] StartImagingDto dto)
    {
        try
        {
            var technician = dto.Technician ?? _currentUserService.UserId;
            await _radiologyService.StartImagingAsync(id, technician);
            return ApiOk(true, "Imaging started successfully");
        }
        catch (KeyNotFoundException)
        {
            return ApiNotFound("Radiology order not found");
        }
    }

    /// <summary>
    /// Completes a radiology order.
    /// </summary>
    [HttpPost("orders/{id:int}/complete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteRadiologyOrder(int id, [FromBody] CompleteRadiologyOrderDto dto)
    {
        try
        {
            var completedBy = dto.CompletedBy ?? _currentUserService.UserId;
            await _radiologyService.CompleteRadiologyOrderAsync(id, completedBy);
            return ApiOk(true, "Radiology order completed successfully");
        }
        catch (KeyNotFoundException)
        {
            return ApiNotFound("Radiology order not found");
        }
    }

    /// <summary>
    /// Approves a radiology order/report.
    /// </summary>
    [HttpPost("orders/{id:int}/approve")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRadiologyOrder(int id, [FromBody] ApproveRadiologyOrderDto dto)
    {
        try
        {
            var approvedBy = dto.ApprovedBy ?? _currentUserService.UserId;
            await _radiologyService.ApproveRadiologyOrderAsync(id, approvedBy);
            return ApiOk(true, "Radiology order approved successfully");
        }
        catch (KeyNotFoundException)
        {
            return ApiNotFound("Radiology order not found");
        }
    }

    /// <summary>
    /// Rejects a radiology order.
    /// </summary>
    [HttpPost("orders/{id:int}/reject")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRadiologyOrder(int id, [FromBody] RejectRadiologyOrderDto dto)
    {
        try
        {
            await _radiologyService.RejectRadiologyOrderAsync(id, _currentUserService.UserId, dto.RejectionReason);
            return ApiOk(true, "Radiology order rejected");
        }
        catch (KeyNotFoundException)
        {
            return ApiNotFound("Radiology order not found");
        }
    }

    #endregion

    #region Imaging Result Endpoints

    /// <summary>
    /// Gets imaging results for a specific order.
    /// </summary>
    [HttpGet("results/order/{orderId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ImagingResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImagingResultsByOrder(int orderId)
    {
        var results = await _radiologyService.GetImagingResultsByOrderIdAsync(orderId);
        var resultDtos = results.Select(MapToImagingResultDto).ToList();
        return ApiOk(resultDtos);
    }

    /// <summary>
    /// Gets imaging results for a specific patient.
    /// </summary>
    [HttpGet("results/patient/{patientId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ImagingResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImagingResultsByPatient(int patientId)
    {
        var results = await _radiologyService.GetImagingResultsByPatientIdAsync(patientId);
        var resultDtos = results.Select(MapToImagingResultDto).ToList();
        return ApiOk(resultDtos);
    }

    /// <summary>
    /// Gets a specific imaging result by ID.
    /// </summary>
    [HttpGet("results/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ImagingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImagingResultById(int id)
    {
        var result = await _radiologyService.GetImagingResultByIdAsync(id);
        if (result == null)
            return ApiNotFound("Imaging result not found");

        return ApiOk(MapToImagingResultDto(result));
    }

    /// <summary>
    /// Creates a new imaging result.
    /// </summary>
    [HttpPost("results")]
    [ProducesResponseType(typeof(ApiResponse<ImagingResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateImagingResult([FromBody] CreateImagingResultDto dto)
    {
        var result = new LabResult
        {
            LabOrderId = dto.RadiologyOrderId,
            LabOrderItemId = dto.RadiologyOrderItemId,
            LabTestId = dto.ImagingStudyId,
            Status = LabResultStatus.Pending,
            Interpretation = dto.Findings,
            Notes = dto.Notes,
            AttachmentPath = dto.ImagePath,
            BranchId = _currentUserService.BranchId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        var created = await _radiologyService.CreateImagingResultAsync(result);
        return ApiCreated(MapToImagingResultDto(created), "Imaging result created successfully");
    }

    /// <summary>
    /// Updates an existing imaging result.
    /// </summary>
    [HttpPut("results/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ImagingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateImagingResult(int id, [FromBody] UpdateImagingResultDto dto)
    {
        if (id != dto.Id)
            return ApiBadRequest("ID mismatch");

        var result = await _radiologyService.GetImagingResultByIdAsync(id);
        if (result == null)
            return ApiNotFound("Imaging result not found");

        result.Interpretation = dto.Findings;
        result.Notes = dto.Notes;
        result.AttachmentPath = dto.ImagePath;
        result.UpdatedAt = DateTime.UtcNow;
        result.UpdatedBy = _currentUserService.UserId;

        await _radiologyService.UpdateImagingResultAsync(result);
        return ApiOk(MapToImagingResultDto(result), "Imaging result updated successfully");
    }

    /// <summary>
    /// Adds a report to an imaging result.
    /// </summary>
    [HttpPost("results/{id:int}/add-report")]
    [ProducesResponseType(typeof(ApiResponse<ImagingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddImagingReport(int id, [FromBody] AddImagingReportDto dto)
    {
        var result = await _radiologyService.GetImagingResultByIdAsync(id);
        if (result == null)
            return ApiNotFound("Imaging result not found");

        result.Interpretation = $"FINDINGS:\n{dto.Findings}\n\nIMPRESSION:\n{dto.Impression}";
        if (!string.IsNullOrEmpty(dto.Recommendation))
            result.Interpretation += $"\n\nRECOMMENDATION:\n{dto.Recommendation}";
        if (!string.IsNullOrEmpty(dto.Technique))
            result.Notes = $"Technique: {dto.Technique}";

        result.IsAbnormal = dto.IsCritical;
        result.Status = LabResultStatus.Completed;
        result.ResultDate = DateTime.UtcNow;
        result.PerformedBy = _currentUserService.UserId;
        result.PerformedDate = DateTime.UtcNow;
        result.UpdatedAt = DateTime.UtcNow;
        result.UpdatedBy = _currentUserService.UserId;

        await _radiologyService.UpdateImagingResultAsync(result);
        return ApiOk(MapToImagingResultDto(result), "Imaging report added successfully");
    }

    /// <summary>
    /// Verifies an imaging result.
    /// </summary>
    [HttpPost("results/{id:int}/verify")]
    [ProducesResponseType(typeof(ApiResponse<ImagingResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyImagingResult(int id, [FromBody] VerifyImagingResultDto dto)
    {
        var result = await _radiologyService.GetImagingResultByIdAsync(id);
        if (result == null)
            return ApiNotFound("Imaging result not found");

        result.VerifiedBy = dto.VerifiedBy ?? _currentUserService.UserId;
        result.VerifiedDate = DateTime.UtcNow;
        result.UpdatedAt = DateTime.UtcNow;
        result.UpdatedBy = _currentUserService.UserId;

        await _radiologyService.UpdateImagingResultAsync(result);
        return ApiOk(MapToImagingResultDto(result), "Imaging result verified successfully");
    }

    /// <summary>
    /// Deletes an imaging result.
    /// </summary>
    [HttpDelete("results/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImagingResult(int id)
    {
        var result = await _radiologyService.GetImagingResultByIdAsync(id);
        if (result == null)
            return ApiNotFound("Imaging result not found");

        await _radiologyService.DeleteImagingResultAsync(id);
        return ApiOk(true, "Imaging result deleted successfully");
    }

    #endregion

    #region Statistics Endpoints

    /// <summary>
    /// Gets radiology statistics for the current branch.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<RadiologyStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRadiologyStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var branchId = _currentUserService.BranchId;
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var totalOrders = await _radiologyService.GetTotalRadiologyOrdersCountAsync(branchId);
        var pendingOrders = await _radiologyService.GetPendingRadiologyOrdersCountAsync(branchId);
        var completedOrders = await _radiologyService.GetCompletedRadiologyOrdersCountAsync(branchId, start, end);
        var totalRevenue = await _radiologyService.GetTotalRadiologyRevenueAsync(branchId, start, end);
        var topStudies = await _radiologyService.GetMostOrderedStudiesAsync(branchId, 10);
        var modalityDistribution = await _radiologyService.GetOrdersByModalityDistributionAsync(branchId);

        var statistics = new RadiologyStatisticsDto
        {
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            InProgressOrders = 0, // Would need additional query
            CompletedOrders = completedOrders,
            TotalRevenue = totalRevenue,
            AverageOrderValue = completedOrders > 0 ? totalRevenue / completedOrders : 0,
            OrdersByModality = modalityDistribution,
            TopStudies = topStudies.Select(t => new TopImagingStudyDto
            {
                StudyName = t.StudyName,
                OrderCount = t.Count,
                Revenue = t.Revenue
            }).ToList()
        };

        return ApiOk(statistics);
    }

    /// <summary>
    /// Gets order count by modality distribution.
    /// </summary>
    [HttpGet("statistics/modality-distribution")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModalityDistribution()
    {
        var branchId = _currentUserService.BranchId;
        var distribution = await _radiologyService.GetOrdersByModalityDistributionAsync(branchId);
        return ApiOk(distribution);
    }

    /// <summary>
    /// Gets most ordered imaging studies.
    /// </summary>
    [HttpGet("statistics/top-studies")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopImagingStudyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopStudies([FromQuery] int count = 10)
    {
        var branchId = _currentUserService.BranchId;
        var topStudies = await _radiologyService.GetMostOrderedStudiesAsync(branchId, count);

        var topStudyDtos = topStudies.Select(t => new TopImagingStudyDto
        {
            StudyName = t.StudyName,
            OrderCount = t.Count,
            Revenue = t.Revenue
        }).ToList();

        return ApiOk(topStudyDtos);
    }

    #endregion

    #region Private Mapping Methods

    private static ImagingStudyDto MapToImagingStudyDto(LabTest study)
    {
        return new ImagingStudyDto
        {
            Id = study.Id,
            StudyCode = study.TestCode,
            StudyName = study.TestName,
            Description = study.Description,
            Modality = Enum.TryParse<ImagingModality>(study.Category.ToString(), out var modality) ? modality : ImagingModality.Other,
            EstimatedDurationMinutes = study.TurnaroundTimeHours,
            Price = study.Price,
            PatientPreparation = study.PreparationInstructions,
            IsActive = study.IsActive,
            RequiresFasting = study.RequiresFasting,
            BranchId = study.BranchId,
            BranchName = study.Branch?.Name,
            CreatedAt = study.CreatedAt,
            CreatedBy = study.CreatedBy,
            UpdatedAt = study.UpdatedAt,
            UpdatedBy = study.UpdatedBy
        };
    }

    private static RadiologyOrderDto MapToRadiologyOrderDto(LabOrder order)
    {
        return new RadiologyOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = MapToRadiologyOrderStatus(order.Status),
            PatientId = order.PatientId,
            PatientName = order.Patient?.FullNameEn,
            AppointmentId = order.VisitId, // Using VisitId as appointment reference
            ReferringDoctorId = order.OrderingDoctorId,
            IsUrgent = order.IsUrgent,
            IsStat = order.IsUrgent, // Using IsUrgent for STAT as well
            ClinicalIndication = order.ClinicalNotes,
            Notes = order.Notes,
            SubTotal = order.Items.Sum(oi => oi.Price),
            DiscountAmount = 0, // LabOrder doesn't have DiscountAmount
            TotalPrice = order.TotalAmount,
            BranchId = order.BranchId,
            BranchName = order.Branch?.Name,
            ReceivedDate = order.ReceivedDate,
            ReceivedBy = order.ReceivedBy,
            PerformedDate = order.PerformedDate,
            PerformedBy = order.PerformedBy,
            CompletedDate = order.CompletedDate,
            ApprovedDate = order.ApprovedDate,
            ApprovedBy = order.ApprovedBy,
            ItemCount = order.Items.Count,
            Items = order.Items.Select(MapToRadiologyOrderItemDto).ToList(),
            Results = order.Results.Select(MapToImagingResultDto).ToList(),
            CreatedAt = order.CreatedAt,
            CreatedBy = order.CreatedBy,
            UpdatedAt = order.UpdatedAt,
            UpdatedBy = order.UpdatedBy
        };
    }

    private static RadiologyOrderItemDto MapToRadiologyOrderItemDto(LabOrderItem item)
    {
        return new RadiologyOrderItemDto
        {
            Id = item.Id,
            RadiologyOrderId = item.LabOrderId,
            ImagingStudyId = item.LabTestId,
            StudyCode = item.LabTest?.TestCode,
            StudyName = item.LabTest?.TestName,
            Modality = Enum.TryParse<ImagingModality>(item.LabTest?.Category.ToString(), out var modality) ? modality : ImagingModality.Other,
            Price = item.Price,
            FinalPrice = item.Price,
            Notes = item.Notes
        };
    }

    private static ImagingResultDto MapToImagingResultDto(LabResult result)
    {
        return new ImagingResultDto
        {
            Id = result.Id,
            RadiologyOrderId = result.LabOrderId,
            OrderNumber = result.LabOrder?.OrderNumber,
            RadiologyOrderItemId = result.LabOrderItemId,
            ImagingStudyId = result.LabTestId,
            StudyName = result.LabTest?.TestName,
            Modality = result.LabTest != null && Enum.TryParse<ImagingModality>(result.LabTest.Category.ToString(), out var modality)
                ? modality : ImagingModality.Other,
            Status = MapToImagingResultStatus(result.Status),
            ResultDate = result.ResultDate,
            Findings = result.Interpretation,
            IsCritical = result.IsAbnormal,
            ImagePath = result.AttachmentPath,
            PerformedBy = result.PerformedBy,
            PerformedDate = result.PerformedDate,
            ReviewedBy = result.ReviewedBy,
            ReviewedDate = result.ReviewedDate,
            VerifiedBy = result.VerifiedBy,
            VerifiedDate = result.VerifiedDate,
            Notes = result.Notes,
            BranchId = result.BranchId,
            CreatedAt = result.CreatedAt,
            CreatedBy = result.CreatedBy,
            UpdatedAt = result.UpdatedAt,
            UpdatedBy = result.UpdatedBy
        };
    }

    private static RadiologyOrderStatus MapToRadiologyOrderStatus(LabOrderStatus status)
    {
        return status switch
        {
            LabOrderStatus.Pending => RadiologyOrderStatus.Pending,
            LabOrderStatus.Collected => RadiologyOrderStatus.Received,
            LabOrderStatus.InProgress => RadiologyOrderStatus.InProgress,
            LabOrderStatus.Completed => RadiologyOrderStatus.Completed,
            LabOrderStatus.Cancelled => RadiologyOrderStatus.Cancelled,
            _ => RadiologyOrderStatus.Pending
        };
    }

    private static ImagingResultStatus MapToImagingResultStatus(LabResultStatus status)
    {
        return status switch
        {
            LabResultStatus.Pending => ImagingResultStatus.Pending,
            LabResultStatus.InProgress => ImagingResultStatus.InProgress,
            LabResultStatus.Completed => ImagingResultStatus.Final,
            LabResultStatus.Reviewed => ImagingResultStatus.Final,
            LabResultStatus.Verified => ImagingResultStatus.Verified,
            _ => ImagingResultStatus.Pending
        };
    }

    #endregion
}
