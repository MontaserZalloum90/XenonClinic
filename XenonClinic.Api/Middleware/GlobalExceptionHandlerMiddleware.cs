using System.Net;
using System.Text.Json;
using XenonClinic.Api.Controllers;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Api.Middleware;

/// <summary>
/// Middleware that handles all unhandled exceptions and returns standardized error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? "unknown";

        // Map exception to response
        var (statusCode, errorResponse) = MapException(exception, correlationId);

        // Log the exception
        LogException(exception, statusCode, correlationId, context);

        // Write response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private (HttpStatusCode statusCode, ApiResponse response) MapException(Exception exception, string correlationId)
    {
        return exception switch
        {
            // Business logic exceptions
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Failure(validationEx.Message, validationEx.Errors)
            ),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateResponse(notFoundEx.Message, correlationId)
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                CreateResponse("Unauthorized access", correlationId)
            ),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                CreateResponse(forbiddenEx.Message, correlationId)
            ),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateResponse(conflictEx.Message, correlationId)
            ),

            // Tenant isolation violations
            TenantIsolationViolationException tenantEx => (
                HttpStatusCode.Forbidden,
                CreateResponse("Access denied to this resource", correlationId)
            ),

            // Rate limiting
            RateLimitExceededException rateLimitEx => (
                HttpStatusCode.TooManyRequests,
                CreateResponse(rateLimitEx.Message ?? "Too many requests. Please try again later.", correlationId)
            ),

            // Operation cancelled (client disconnected)
            OperationCanceledException => (
                HttpStatusCode.BadRequest,
                CreateResponse("Operation was cancelled", correlationId)
            ),

            // Timeout
            TimeoutException => (
                HttpStatusCode.GatewayTimeout,
                CreateResponse("The operation timed out", correlationId)
            ),

            // Database errors
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx => (
                HttpStatusCode.Conflict,
                CreateResponse(GetDbErrorMessage(dbEx), correlationId)
            ),

            // Default - internal server error
            _ => (
                HttpStatusCode.InternalServerError,
                CreateResponse(GetErrorMessage(exception), correlationId)
            )
        };
    }

    private ApiResponse CreateResponse(string error, string correlationId)
    {
        var response = ApiResponse.Failure(error);
        response.TraceId = correlationId;
        return response;
    }

    private string GetErrorMessage(Exception exception)
    {
        if (_environment.IsDevelopment())
        {
            return exception.Message;
        }

        return "An unexpected error occurred. Please try again later.";
    }

    private string GetDbErrorMessage(Microsoft.EntityFrameworkCore.DbUpdateException exception)
    {
        // Check for specific database constraints
        var message = exception.InnerException?.Message ?? exception.Message;

        if (message.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
        {
            return "A record with this information already exists.";
        }

        if (message.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase))
        {
            return "This operation references data that doesn't exist or cannot be modified.";
        }

        if (_environment.IsDevelopment())
        {
            return message;
        }

        return "A database error occurred. Please try again later.";
    }

    private void LogException(Exception exception, HttpStatusCode statusCode, string correlationId, HttpContext context)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Information,
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => LogLevel.Warning,
            _ => LogLevel.Warning
        };

        _logger.Log(logLevel, exception,
            "Exception occurred - CorrelationId: {CorrelationId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}",
            correlationId, context.Request.Method, context.Request.Path, (int)statusCode);
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]>? Errors { get; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
}

/// <summary>
/// Exception thrown when a resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string resourceType, object id)
        : base($"{resourceType} with id '{id}' was not found.")
    {
    }
}

/// <summary>
/// Exception thrown when access is forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when there is a conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when rate limit is exceeded.
/// </summary>
public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string? message = null)
        : base(message ?? "Rate limit exceeded")
    {
    }
}

/// <summary>
/// Extension methods for global exception handler middleware.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
