using FluentAssertions;
using Xunit;

namespace XenonClinic.Tests.Validation;

/// <summary>
/// Tests for null validation and guard clauses across the application.
/// </summary>
public class NullValidationTests
{
    [Fact]
    public void ApiResponse_WithNullData_ShouldHandleGracefully()
    {
        // Arrange & Act
        var response = new Api.Controllers.ApiResponse<string?>
        {
            Success = true,
            Data = null,
            Message = "No data available"
        };

        // Assert
        response.Data.Should().BeNull();
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void PaginatedResponse_WithEmptyItems_ShouldReturnDefaults()
    {
        // Arrange & Act
        var response = new Api.Controllers.PaginatedResponse<string>
        {
            Items = Array.Empty<string>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        // Assert
        response.Items.Should().BeEmpty();
        response.TotalPages.Should().Be(0);
        response.HasPreviousPage.Should().BeFalse();
        response.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PaginatedResponse_WithZeroPageSize_ShouldNotThrow()
    {
        // Arrange & Act
        var response = new Api.Controllers.PaginatedResponse<string>
        {
            Items = new[] { "item1" },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 0
        };

        // Assert - TotalPages uses division, should handle zero gracefully
        response.TotalPages.Should().Be(0);
    }

    [Fact]
    public void PaginationRequest_WithNegativePageNumber_ShouldResetToOne()
    {
        // Arrange & Act
        var request = new Api.Controllers.PaginationRequest
        {
            PageNumber = -5
        };

        // Assert
        request.PageNumber.Should().Be(1);
    }

    [Fact]
    public void PaginationRequest_WithZeroPageNumber_ShouldResetToOne()
    {
        // Arrange & Act
        var request = new Api.Controllers.PaginationRequest
        {
            PageNumber = 0
        };

        // Assert
        request.PageNumber.Should().Be(1);
    }

    [Fact]
    public void PaginationRequest_WithNegativePageSize_ShouldResetToDefault()
    {
        // Arrange & Act
        var request = new Api.Controllers.PaginationRequest
        {
            PageSize = -10
        };

        // Assert
        request.PageSize.Should().Be(20); // Default page size
    }

    [Fact]
    public void PaginationRequest_WithExcessivePageSize_ShouldCapAt100()
    {
        // Arrange & Act
        var request = new Api.Controllers.PaginationRequest
        {
            PageSize = 1000
        };

        // Assert
        request.PageSize.Should().Be(100); // Max page size
    }

    [Fact]
    public void DiagnosticsMetrics_Uptime_WithDefaultValues_ShouldNotThrow()
    {
        // Arrange & Act
        var uptime = new Api.Controllers.UptimeInfo();

        // Assert - default values should be safe
        uptime.Days.Should().Be(0);
        uptime.Hours.Should().Be(0);
        uptime.TotalSeconds.Should().Be(0);
    }

    [Fact]
    public void DiagnosticsMetrics_Memory_WithDefaultValues_ShouldNotThrow()
    {
        // Arrange & Act
        var memory = new Api.Controllers.MemoryMetrics();

        // Assert
        memory.WorkingSetMB.Should().Be(0);
        memory.GcGen0Collections.Should().Be(0);
    }

    [Fact]
    public void DiagnosticsMetrics_Database_WithError_ShouldContainMessage()
    {
        // Arrange & Act
        var metrics = new Api.Controllers.DatabaseMetrics
        {
            IsConnected = false,
            Error = "Connection timeout"
        };

        // Assert
        metrics.IsConnected.Should().BeFalse();
        metrics.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DiagnosticsMetrics_Cache_WithError_ShouldContainMessage()
    {
        // Arrange & Act
        var metrics = new Api.Controllers.CacheMetrics
        {
            IsOperational = false,
            Error = "Redis unavailable"
        };

        // Assert
        metrics.IsOperational.Should().BeFalse();
        metrics.Error.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ApiResponse_Error_WithEmptyOrNullMessage_ShouldBeHandled(string? errorMessage)
    {
        // Arrange & Act
        var response = Api.Controllers.ApiResponse.Failure(errorMessage ?? "");

        // Assert
        response.Success.Should().BeFalse();
    }

    [Fact]
    public void RuntimeInfo_WithDefaultValues_ShouldHaveEmptyStrings()
    {
        // Arrange & Act
        var runtime = new Api.Controllers.RuntimeInfo();

        // Assert - empty strings are better than null
        runtime.FrameworkDescription.Should().NotBeNull();
        runtime.OsDescription.Should().NotBeNull();
        runtime.ProcessArchitecture.Should().NotBeNull();
        runtime.MachineName.Should().NotBeNull();
    }

    [Fact]
    public void ApplicationMetrics_WithDefaultValues_ShouldNotContainNulls()
    {
        // Arrange & Act
        var metrics = new Api.Controllers.ApplicationMetrics();

        // Assert
        metrics.ApplicationName.Should().NotBeNull();
        metrics.Version.Should().NotBeNull();
        metrics.Environment.Should().NotBeNull();
        metrics.Uptime.Should().NotBeNull();
        metrics.Memory.Should().NotBeNull();
        metrics.Threads.Should().NotBeNull();
        metrics.Runtime.Should().NotBeNull();
        metrics.Requests.Should().NotBeNull();
    }
}
