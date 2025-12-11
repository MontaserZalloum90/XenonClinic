using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.Should().ContainKey(CorrelationIdHeader);
        var correlationId = context.Response.Headers[CorrelationIdHeader].ToString();
        correlationId.Should().NotBeNullOrEmpty();
        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ExistingCorrelationId_UsesExisting()
    {
        // Arrange
        var existingId = Guid.NewGuid().ToString();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = existingId;

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers[CorrelationIdHeader].ToString().Should().Be(existingId);
        accessor.CorrelationId.Should().Be(existingId);
    }

    [Fact]
    public async Task InvokeAsync_InvalidCorrelationId_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "not-a-guid";

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var resultId = context.Response.Headers[CorrelationIdHeader].ToString();
        resultId.Should().NotBe("not-a-guid");
        Guid.TryParse(resultId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SetsAccessorCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        accessor.CorrelationId.Should().NotBeNullOrEmpty();
        accessor.CorrelationId.Should().Be(context.Response.Headers[CorrelationIdHeader].ToString());
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var nextCalled = false;

        var middleware = new CorrelationIdMiddleware(
            next: _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AddsToRequestItems()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Items.Should().ContainKey("CorrelationId");
        context.Items["CorrelationId"].Should().Be(accessor.CorrelationId);
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
        var expectedId = Guid.NewGuid().ToString();

        // Act
        accessor.CorrelationId = expectedId;

        // Assert
        accessor.CorrelationId.Should().Be(expectedId);
    }

    [Fact]
    public void CorrelationIdAccessor_SetNull_ReturnsEmpty()
    {
        // Arrange
        var accessor = new CorrelationIdAccessor();

        // Act
        accessor.CorrelationId = null!;

        // Assert
        accessor.CorrelationId.Should().BeEmpty();
    }

    [Fact]
    public async Task CorrelationIdAccessor_IsScoped_DifferentPerRequest()
    {
        // This test verifies the accessor maintains correlation ID per request
        var accessor1 = new CorrelationIdAccessor();
        var accessor2 = new CorrelationIdAccessor();

        accessor1.CorrelationId = "request-1";
        accessor2.CorrelationId = "request-2";

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
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var resultId = context.Response.Headers[CorrelationIdHeader].ToString();
        resultId.Should().NotBeEmpty();
        Guid.TryParse(resultId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitespaceCorrelationIdHeader_GeneratesNew()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdHeader] = "   ";

        var accessor = new CorrelationIdAccessor();
        var middleware = new CorrelationIdMiddleware(
            next: _ => Task.CompletedTask,
            accessor: accessor);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var resultId = context.Response.Headers[CorrelationIdHeader].ToString();
        Guid.TryParse(resultId.Trim(), out _).Should().BeTrue();
    }

    #endregion
}
