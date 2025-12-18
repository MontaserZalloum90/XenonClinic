using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using XenonClinic.Api.Middleware;
using Xunit;

namespace XenonClinic.Tests.Api;

/// <summary>
/// Tests for the CorrelationId middleware and accessor.
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    #region Middleware Tests

    [Fact]
    public async Task InvokeAsync_NoExistingCorrelationId_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(accessor.CorrelationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ExistingCorrelationId_UsesExisting()
    {
        // Arrange
        var existingId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = existingId;

        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_InvalidCorrelationId_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "invalid<script>chars";

        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        var resultId = accessor.CorrelationId;
        resultId.Should().NotBe("invalid<script>chars");
        Guid.TryParse(resultId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SetsAccessorCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var nextCalled = false;

        var middleware = new CorrelationIdMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SetsRequestId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        accessor.RequestId.Should().NotBeNullOrEmpty();
        accessor.RequestId.Should().HaveLength(12);
    }

    #endregion

    #region Accessor Tests

    [Fact]
    public void CorrelationIdAccessor_DefaultValue_IsEmpty()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act & Assert
        accessor.CorrelationId.Should().BeEmpty();
    }

    [Fact]
    public void CorrelationIdAccessor_SetAndGet_WorksCorrectly()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();
        var expectedCorrelationId = Guid.NewGuid().ToString();
        var expectedRequestId = "test123456";

        // Act
        accessor.Set(expectedCorrelationId, expectedRequestId);

        // Assert
        accessor.CorrelationId.Should().Be(expectedCorrelationId);
        accessor.RequestId.Should().Be(expectedRequestId);
    }

    [Fact]
    public async Task CorrelationIdAccessor_IsScoped_DifferentPerRequest()
    {
        // This test verifies the accessor maintains correlation ID per request
        var accessor1 = new CorrelationIdAccessor();
        var accessor2 = new CorrelationIdAccessor();

        accessor1.Set("request-1", "req1");
        accessor2.Set("request-2", "req2");

        accessor1.CorrelationId.Should().Be("request-1");
        accessor2.CorrelationId.Should().Be("request-2");
        await Task.CompletedTask;
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task InvokeAsync_EmptyCorrelationIdHeader_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "";

        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        var resultId = accessor.CorrelationId;
        resultId.Should().NotBeEmpty();
        Guid.TryParse(resultId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_OversizedCorrelationId_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = new string('a', 100);

        var accessor = new CorrelationIdAccessor();
        var logger = Mock.Of<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            logger: logger);

        // Act
        await middleware.InvokeAsync(context, accessor);

        // Assert
        var resultId = accessor.CorrelationId;
        Guid.TryParse(resultId, out _).Should().BeTrue();
    }

    #endregion
}
