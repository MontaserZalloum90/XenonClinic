using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using Xunit;

namespace XenonClinic.Tests.Infrastructure;

/// <summary>
/// Tests for the enhanced Repository implementation with pagination and read-only queries.
/// </summary>
public class RepositoryTests : IDisposable
{
    private readonly ClinicDbContext _context;
    private readonly Repository<Branch> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _repository = new Repository<Branch>(_context);

        SeedTestData();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private void SeedTestData()
    {
        var company = new Company
        {
            Id = 1,
            TenantId = 1,
            Name = "Test Company",
            Code = "TEST-COMP",
            IsActive = true
        };
        _context.Companies.Add(company);

        var branches = new List<Branch>();
        for (int i = 1; i <= 25; i++)
        {
            branches.Add(new Branch
            {
                Id = i,
                CompanyId = 1,
                Code = $"BR-{i:D3}",
                Name = $"Branch {i}",
                IsActive = i <= 20 // First 20 are active
            });
        }
        _context.Branches.AddRange(branches);
        _context.SaveChanges();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Code.Should().Be("BR-001");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEntity_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_TracksChanges()
    {
        // Act
        var branch = await _repository.GetByIdAsync(1);
        branch!.Name = "Modified Name";

        // Assert - entity should be tracked
        var entry = _context.Entry(branch);
        entry.State.Should().Be(EntityState.Modified);
    }

    #endregion

    #region GetByIdReadOnlyAsync Tests

    [Fact]
    public async Task GetByIdReadOnlyAsync_ExistingEntity_ReturnsEntity()
    {
        // Act
        var result = await _repository.GetByIdReadOnlyAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdReadOnlyAsync_DoesNotTrackChanges()
    {
        // Act
        var branch = await _repository.GetByIdReadOnlyAsync(1);
        branch!.Name = "Modified Name";

        // Assert - entity should not be tracked
        var entry = _context.Entry(branch);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_FirstPage_ReturnsCorrectItems()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ReturnsCorrectItems()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 2,
            pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_LastPage_ReturnsRemainingItems()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 3,
            pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithPredicate_FiltersResults()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            predicate: b => b.IsActive);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(20); // Only active branches
        result.Items.Should().OnlyContain(b => b.IsActive);
    }

    [Fact]
    public async Task GetPagedAsync_WithOrderBy_OrdersResults()
    {
        // Act
        var resultAsc = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 5,
            orderBy: b => b.Name,
            ascending: true);

        var resultDesc = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 5,
            orderBy: b => b.Name,
            ascending: false);

        // Assert
        resultAsc.Items.First().Name.Should().Be("Branch 1");
        resultDesc.Items.First().Name.Should().Be("Branch 9"); // String ordering
    }

    [Fact]
    public async Task GetPagedAsync_InvalidPageNumber_DefaultsToOne()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: -1,
            pageSize: 10);

        // Assert
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ExcessivePageSize_LimitedTo1000()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 5000);

        // Assert
        result.PageSize.Should().Be(1000);
    }

    [Fact]
    public async Task GetPagedAsync_EmptyResult_ReturnsEmptyResult()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            predicate: b => b.Code == "NONEXISTENT");

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    #endregion

    #region FindReadOnlyAsync Tests

    [Fact]
    public async Task FindReadOnlyAsync_MatchingEntities_ReturnsEntities()
    {
        // Act
        var result = await _repository.FindReadOnlyAsync(b => b.IsActive);

        // Assert
        result.Should().HaveCount(20);
        result.Should().OnlyContain(b => b.IsActive);
    }

    [Fact]
    public async Task FindReadOnlyAsync_EntitiesNotTracked()
    {
        // Act
        var results = (await _repository.FindReadOnlyAsync(b => b.IsActive)).ToList();
        results.First().Name = "Modified";

        // Assert - entity should not be tracked
        var entry = _context.Entry(results.First());
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region QueryReadOnly Tests

    [Fact]
    public void QueryReadOnly_ReturnsQueryableWithNoTracking()
    {
        // Act
        var query = _repository.QueryReadOnly();
        var result = query.First();
        result.Name = "Modified";

        // Assert - entity should not be tracked
        var entry = _context.Entry(result);
        entry.State.Should().Be(EntityState.Detached);
    }

    [Fact]
    public void Query_ReturnsQueryableWithTracking()
    {
        // Act
        var query = _repository.Query();
        var result = query.First();
        result.Name = "Modified";

        // Assert - entity should be tracked
        var entry = _context.Entry(result);
        entry.State.Should().Be(EntityState.Modified);
    }

    #endregion
}
