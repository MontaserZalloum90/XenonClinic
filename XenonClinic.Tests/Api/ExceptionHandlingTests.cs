using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using XenonClinic.Api.Controllers;
using XenonClinic.Api.Middleware;
using XenonClinic.Infrastructure.Data;
using Xunit;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Tests for the global exception handler middleware.
/// </summary>
public class ExceptionHandlingTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;

    public ExceptionHandlingTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
    }

    #region Exception Type Mapping Tests

    [Fact]
    public async Task HandleException_ValidationException_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ValidationException("Validation failed");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var response = await ReadResponse(context);
        response.Success.Should().BeFalse();
        response.Error.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task HandleException_ValidationExceptionWithErrors_ReturnsErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Email", new[] { "Email is invalid" } }
        };
        var exception = new ValidationException("Validation failed", errors);
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponse(context);
        response.Errors.Should().NotBeNull();
        response.Errors.Should().ContainKey("Name");
        response.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task HandleException_NotFoundException_Returns404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new NotFoundException("Patient", 123);
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        var response = await ReadResponse(context);
        response.Error.Should().Contain("Patient");
        response.Error.Should().Contain("123");
    }

    [Fact]
    public async Task HandleException_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException();
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HandleException_ForbiddenException_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ForbiddenException("Access denied to this resource");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        var response = await ReadResponse(context);
        response.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task HandleException_ConflictException_Returns409()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new ConflictException("Resource already exists");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task HandleException_TenantIsolationViolation_Returns403()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new TenantIsolationViolationException("Tenant 1", "Tenant 2");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        var response = await ReadResponse(context);
        response.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task HandleException_RateLimitExceeded_Returns429()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new RateLimitExceededException("Too many requests");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task HandleException_OperationCancelled_Returns400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new OperationCanceledException();
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task HandleException_Timeout_Returns504()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new TimeoutException();
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.GatewayTimeout);
    }

    [Fact]
    public async Task HandleException_UnknownException_Returns500()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");
        var middleware = CreateMiddleware(_ => throw exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Response Format Tests

    [Fact]
    public async Task HandleException_SetsJsonContentType()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Test"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task HandleException_IncludesCorrelationId()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "test-correlation-id";
        var middleware = CreateMiddleware(_ => throw new Exception("Test"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponse(context);
        response.TraceId.Should().Be("test-correlation-id");
    }

    [Fact]
    public async Task HandleException_NoCorrelationId_UsesUnknown()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Test"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponse(context);
        response.TraceId.Should().Be("unknown");
    }

    #endregion

    #region Environment-Specific Behavior Tests

    [Fact]
    public async Task HandleException_Development_ShowsDetailedError()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Detailed error message"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponse(context);
        response.Error.Should().Contain("Detailed error message");
    }

    [Fact]
    public async Task HandleException_Production_HidesDetailedError()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Sensitive error details"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var response = await ReadResponse(context);
        response.Error.Should().NotContain("Sensitive error details");
        response.Error.Should().Contain("unexpected error");
    }

    #endregion

    #region Logging Tests

    [Fact]
    public async Task HandleException_InternalError_LogsError()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new Exception("Test error"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleException_BadRequest_LogsWarning()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new ValidationException("Bad input"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleException_NotFound_LogsInformation()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = CreateMiddleware(_ => throw new NotFoundException("Resource not found"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region No Exception Tests

    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        // Arrange
        var context = CreateHttpContext();
        var nextCalled = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200); // Default unchanged
    }

    #endregion

    #region Custom Exception Tests

    [Fact]
    public void ValidationException_WithMessage_SetsMessage()
    {
        var ex = new ValidationException("Test message");
        ex.Message.Should().Be("Test message");
        ex.Errors.Should().BeNull();
    }

    [Fact]
    public void ValidationException_WithErrors_SetsErrors()
    {
        var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error" } } };
        var ex = new ValidationException("Test", errors);
        ex.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public void NotFoundException_WithMessage_SetsMessage()
    {
        var ex = new NotFoundException("Custom message");
        ex.Message.Should().Be("Custom message");
    }

    [Fact]
    public void NotFoundException_WithTypeAndId_FormatsMessage()
    {
        var ex = new NotFoundException("Patient", 123);
        ex.Message.Should().Contain("Patient");
        ex.Message.Should().Contain("123");
    }

    [Fact]
    public void RateLimitExceededException_DefaultMessage_IsSet()
    {
        var ex = new RateLimitExceededException();
        ex.Message.Should().Be("Rate limit exceeded");
    }

    [Fact]
    public void RateLimitExceededException_CustomMessage_IsUsed()
    {
        var ex = new RateLimitExceededException("Custom rate limit message");
        ex.Message.Should().Be("Custom rate limit message");
    }

    #endregion

    #region Helper Methods

    private GlobalExceptionHandlerMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, _environmentMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiResponse> ReadResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })!;
    }

    #endregion
}
