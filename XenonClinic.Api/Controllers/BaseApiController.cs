using Microsoft.AspNetCore.Mvc;

namespace XenonClinic.Api.Controllers;

/// <summary>
/// Base controller providing standardized API response methods.
/// All API controllers should inherit from this class.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns a successful response with data.
    /// </summary>
    protected ActionResult<ApiResponse<T>> ApiOk<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    /// <summary>
    /// Returns a successful response without data.
    /// </summary>
    protected ActionResult<ApiResponse> ApiOk(string? message = null)
    {
        return Ok(ApiResponse.Ok(message));
    }

    /// <summary>
    /// Returns a created response with data.
    /// </summary>
    protected ActionResult<ApiResponse<T>> ApiCreated<T>(T data, string? location = null, string? message = null)
    {
        var response = ApiResponse<T>.Success(data, message ?? "Resource created successfully");
        if (!string.IsNullOrEmpty(location))
        {
            return Created(location, response);
        }
        return StatusCode(201, response);
    }

    /// <summary>
    /// Returns a no content response.
    /// </summary>
    protected new ActionResult NoContent()
    {
        return StatusCode(204);
    }

    /// <summary>
    /// Returns a bad request response with error details.
    /// </summary>
    protected ActionResult<ApiResponse> ApiBadRequest(string error, IDictionary<string, string[]>? validationErrors = null)
    {
        return BadRequest(ApiResponse.Failure(error, validationErrors));
    }

    /// <summary>
    /// Returns a bad request response from ModelState.
    /// </summary>
    protected ActionResult<ApiResponse> ApiBadRequestFromModelState()
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return BadRequest(ApiResponse.Failure("Validation failed", errors));
    }

    /// <summary>
    /// Returns a not found response.
    /// </summary>
    protected ActionResult<ApiResponse> ApiNotFound(string error = "Resource not found")
    {
        return NotFound(ApiResponse.Failure(error));
    }

    /// <summary>
    /// Returns a not found response (generic version for typed responses).
    /// </summary>
    protected ActionResult<ApiResponse<T>> ApiNotFound<T>(string error = "Resource not found")
    {
        return NotFound(ApiResponse<T>.Failure(error));
    }

    /// <summary>
    /// Returns an unauthorized response.
    /// </summary>
    protected ActionResult<ApiResponse> ApiUnauthorized(string error = "Unauthorized access")
    {
        return Unauthorized(ApiResponse.Failure(error));
    }

    /// <summary>
    /// Returns an unauthorized response (generic version for typed responses).
    /// </summary>
    protected ActionResult<ApiResponse<T>> ApiUnauthorized<T>(string error = "Unauthorized access")
    {
        return Unauthorized(ApiResponse<T>.Failure(error));
    }

    /// <summary>
    /// Returns a forbidden response.
    /// </summary>
    protected ActionResult<ApiResponse> ApiForbidden(string error = "Access denied")
    {
        return StatusCode(403, ApiResponse.Failure(error));
    }

    /// <summary>
    /// Returns a forbidden response (generic version for typed responses).
    /// </summary>
    protected ActionResult<ApiResponse<T>> ApiForbidden<T>(string error = "Access denied")
    {
        return StatusCode(403, ApiResponse<T>.Failure(error));
    }

    /// <summary>
    /// Returns a conflict response.
    /// </summary>
    protected ActionResult<ApiResponse> ApiConflict(string error)
    {
        return Conflict(ApiResponse.Failure(error));
    }

    /// <summary>
    /// Returns an internal server error response.
    /// </summary>
    protected ActionResult<ApiResponse> ApiServerError(string error = "An unexpected error occurred")
    {
        return StatusCode(500, ApiResponse.Failure(error));
    }

    /// <summary>
    /// Returns a paginated response.
    /// </summary>
    protected ActionResult<ApiResponse<PaginatedResponse<T>>> ApiPaginated<T>(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string? message = null)
    {
        var paginatedResponse = new PaginatedResponse<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(ApiResponse<PaginatedResponse<T>>.Success(paginatedResponse, message));
    }
}

/// <summary>
/// Standard API response wrapper.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Failure(string error, IDictionary<string, string[]>? validationErrors = null) => new()
    {
        Success = false,
        Error = error,
        ValidationErrors = validationErrors
    };
}

/// <summary>
/// Standard API response wrapper with data.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public new static ApiResponse<T> Failure(string error, IDictionary<string, string[]>? validationErrors = null) => new()
    {
        Success = false,
        Error = error,
        ValidationErrors = validationErrors
    };
}

/// <summary>
/// Standard paginated response.
/// </summary>
public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Standard pagination request parameters.
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : (value > 100 ? 100 : value);
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? Search { get; set; }
}
