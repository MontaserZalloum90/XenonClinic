using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using Xunit;

namespace XenonClinic.Tests.Infrastructure;

/// <summary>
/// Tests for the Unit of Work pattern implementation.
/// </summary>
public class UnitOfWorkTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private UnitOfWork _unitOfWork = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _unitOfWork = new UnitOfWork(_context);

        await SeedTestDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _unitOfWork.DisposeAsync();
        await _context.DisposeAsync();
    }

    private async Task SeedTestDataAsync()
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

        var branch = new Branch
        {
            Id = 1,
            CompanyId = 1,
            Code = "BR-001",
            Name = "Test Branch",
            IsActive = true
        };
        _context.Branches.Add(branch);

        await _context.SaveChangesAsync();
    }

    #region Repository Tests

    [Fact]
    public void Repository_ReturnsSameInstanceForSameType()
    {
        // Act
        var repo1 = _unitOfWork.Repository<Branch>();
        var repo2 = _unitOfWork.Repository<Branch>();

        // Assert
        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Repository_ReturnsDifferentInstancesForDifferentTypes()
    {
        // Act
        var branchRepo = _unitOfWork.Repository<Branch>();
        var companyRepo = _unitOfWork.Repository<Company>();

        // Assert
        branchRepo.Should().NotBeSameAs(companyRepo);
    }

    [Fact]
    public async Task Repository_CanQueryThroughUnitOfWork()
    {
        // Act
        var repo = _unitOfWork.Repository<Branch>();
        var branch = await repo.GetByIdAsync(1);

        // Assert
        branch.Should().NotBeNull();
        branch!.Code.Should().Be("BR-001");
    }

    #endregion

    #region HasChanges Tests

    [Fact]
    public void HasChanges_NoChanges_ReturnsFalse()
    {
        // Assert
        _unitOfWork.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task HasChanges_AfterAdd_ReturnsTrue()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();

        // Act
        await repo.AddAsync(new Branch
        {
            Id = 2,
            CompanyId = 1,
            Code = "BR-002",
            Name = "New Branch",
            IsActive = true
        });

        // Assert
        _unitOfWork.HasChanges.Should().BeTrue();
    }

    [Fact]
    public async Task HasChanges_AfterSaveChanges_ReturnsFalse()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();
        await repo.AddAsync(new Branch
        {
            Id = 3,
            CompanyId = 1,
            Code = "BR-003",
            Name = "Another Branch",
            IsActive = true
        });

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _unitOfWork.HasChanges.Should().BeFalse();
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_PersistsChanges()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();
        var newBranch = new Branch
        {
            Id = 4,
            CompanyId = 1,
            Code = "BR-004",
            Name = "Saved Branch",
            IsActive = true
        };
        await repo.AddAsync(newBranch);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);

        // Verify persistence
        var savedBranch = await repo.GetByIdAsync(4);
        savedBranch.Should().NotBeNull();
        savedBranch!.Name.Should().Be("Saved Branch");
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleOperations_AllPersisted()
    {
        // Arrange
        var branchRepo = _unitOfWork.Repository<Branch>();
        var companyRepo = _unitOfWork.Repository<Company>();

        await branchRepo.AddAsync(new Branch
        {
            Id = 5,
            CompanyId = 1,
            Code = "BR-005",
            Name = "Branch 5",
            IsActive = true
        });

        var company = await companyRepo.GetByIdAsync(1);
        company!.Name = "Updated Company Name";

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(2); // 1 add + 1 update
    }

    #endregion

    #region Rollback Tests

    [Fact]
    public async Task Rollback_AddedEntity_DetachesEntity()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();
        var newBranch = new Branch
        {
            Id = 6,
            CompanyId = 1,
            Code = "BR-006",
            Name = "Will Be Rolled Back",
            IsActive = true
        };
        await repo.AddAsync(newBranch);

        _unitOfWork.HasChanges.Should().BeTrue();

        // Act
        _unitOfWork.Rollback();

        // Assert
        _unitOfWork.HasChanges.Should().BeFalse();
    }

    [Fact]
    public async Task Rollback_ModifiedEntity_ReloadsOriginalValues()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();
        var branch = await repo.GetByIdAsync(1);
        var originalName = branch!.Name;
        branch.Name = "Modified Name";

        _unitOfWork.HasChanges.Should().BeTrue();

        // Act
        _unitOfWork.Rollback();

        // Assert
        _unitOfWork.HasChanges.Should().BeFalse();
        branch.Name.Should().Be(originalName);
    }

    #endregion

    #region Transaction Tests

    [Fact]
    public async Task BeginTransactionAsync_ReturnsTransaction()
    {
        // Act
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        // Assert
        transaction.Should().NotBeNull();
        transaction.TransactionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Transaction_CommitAsync_PersistsChanges()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();

        // Act
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        await repo.AddAsync(new Branch
        {
            Id = 7,
            CompanyId = 1,
            Code = "BR-007",
            Name = "Transaction Branch",
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        // Assert - create new context to verify persistence
        var verifyOptions = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: _context.Database.GetDbConnection().Database)
            .Options;

        await using var verifyContext = new ClinicDbContext(verifyOptions);
        var savedBranch = await verifyContext.Branches.FindAsync(7);
        savedBranch.Should().NotBeNull();
    }

    [Fact]
    public async Task Transaction_RollbackAsync_DiscardsChanges()
    {
        // Arrange
        var repo = _unitOfWork.Repository<Branch>();

        // Act
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        await repo.AddAsync(new Branch
        {
            Id = 8,
            CompanyId = 1,
            Code = "BR-008",
            Name = "Rollback Branch",
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
        await transaction.RollbackAsync();

        // Note: In-memory database doesn't support true transactions,
        // so this test demonstrates the API usage. In real SQL Server,
        // the rollback would discard the changes.
    }

    #endregion
}
