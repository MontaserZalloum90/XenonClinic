using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Infrastructure.Data;
using Xunit;

namespace XenonClinic.Tests.Services;

/// <summary>
/// Tests for generic Repository pattern.
/// </summary>
public class RepositoryTests
{
    private readonly DbContextOptions<ClinicDbContext> _options;

    public RepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var repository = new Repository<Branch>(context);
        var branch = new Branch
        {
            Name = "Test Branch",
            Code = "TST",
            IsActive = true,
            CompanyId = 1
        };

        // Act
        var result = await repository.AddAsync(branch);
        await repository.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var branch = new Branch
        {
            Name = "Test Branch",
            Code = "TST",
            IsActive = true,
            CompanyId = 1
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.GetByIdAsync(branch.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Branch", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        context.Branches.AddRange(
            new Branch { Name = "Branch 1", Code = "BR1", IsActive = true, CompanyId = 1 },
            new Branch { Name = "Branch 2", Code = "BR2", IsActive = true, CompanyId = 1 },
            new Branch { Name = "Branch 3", Code = "BR3", IsActive = true, CompanyId = 1 }
        );
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        context.Branches.AddRange(
            new Branch { Name = "Active Branch", Code = "ACT", IsActive = true, CompanyId = 1 },
            new Branch { Name = "Inactive Branch", Code = "INA", IsActive = false, CompanyId = 1 }
        );
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.FindAsync(b => b.IsActive);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Branch", result.First().Name);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnTrue_WhenMatchExists()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        context.Branches.Add(new Branch { Name = "Test", Code = "TST", IsActive = true, CompanyId = 1 });
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.AnyAsync(b => b.IsActive);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalse_WhenNoMatch()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var repository = new Repository<Branch>(context);

        // Act
        var result = await repository.AnyAsync(b => b.Name == "NonExistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        context.Branches.AddRange(
            new Branch { Name = "Branch 1", Code = "BR1", IsActive = true, CompanyId = 1 },
            new Branch { Name = "Branch 2", Code = "BR2", IsActive = true, CompanyId = 1 }
        );
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        var totalCount = await repository.CountAsync();
        var activeCount = await repository.CountAsync(b => b.IsActive);

        // Assert
        Assert.Equal(2, totalCount);
        Assert.Equal(2, activeCount);
    }

    [Fact]
    public async Task Update_ShouldModifyEntity()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var branch = new Branch { Name = "Original", Code = "ORG", IsActive = true, CompanyId = 1 };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        branch.Name = "Updated";
        repository.Update(branch);
        await repository.SaveChangesAsync();

        // Assert
        var updated = await repository.GetByIdAsync(branch.Id);
        Assert.Equal("Updated", updated?.Name);
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        // Arrange
        using var context = new ClinicDbContext(_options);
        var branch = new Branch { Name = "ToDelete", Code = "DEL", IsActive = true, CompanyId = 1 };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var repository = new Repository<Branch>(context);

        // Act
        repository.Remove(branch);
        await repository.SaveChangesAsync();

        // Assert
        var deleted = await repository.GetByIdAsync(branch.Id);
        Assert.Null(deleted);
    }
}
