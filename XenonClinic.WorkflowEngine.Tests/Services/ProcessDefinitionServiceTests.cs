using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using XenonClinic.WorkflowEngine.Application.DTOs;
using XenonClinic.WorkflowEngine.Application.Services;
using XenonClinic.WorkflowEngine.Domain.Entities;
using XenonClinic.WorkflowEngine.Domain.Models;
using XenonClinic.WorkflowEngine.Tests.Testing;
using static XenonClinic.WorkflowEngine.Tests.Testing.WorkflowAssertions;

namespace XenonClinic.WorkflowEngine.Tests.Services;

/// <summary>
/// Unit tests for the ProcessDefinitionService.
/// </summary>
public class ProcessDefinitionServiceTests : IDisposable
{
    private readonly WorkflowTestFixture _fixture;

    public ProcessDefinitionServiceTests()
    {
        _fixture = new WorkflowTestFixture();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateProcessDefinition()
    {
        // Arrange
        var request = CreateValidRequest("create-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.CreateAsync(request);

        // Assert
        AssertThat(result)
            .HasKey("create-test")
            .HasVersion(1)
            .IsNotDeployed();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateKey_ShouldCreateNewVersion()
    {
        // Arrange
        var request1 = CreateValidRequest("duplicate-test");
        var request2 = CreateValidRequest("duplicate-test");

        // Act
        var result1 = await _fixture.ProcessDefinitionService.CreateAsync(request1);
        var deployed1 = await _fixture.ProcessDefinitionService.DeployAsync(result1.Id);
        var result2 = await _fixture.ProcessDefinitionService.CreateAsync(request2);

        // Assert
        AssertThat(result1).HasVersion(1);
        AssertThat(result2).HasVersion(2);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingDefinition_ShouldReturnDefinition()
    {
        // Arrange
        var created = await CreateAndDeployDefinition("get-by-id-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        AssertThat(result)
            .HasKey("get-by-id-test")
            .IsDeployed();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _fixture.ProcessDefinitionService.GetByIdAsync("non-existing-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByKeyAsync_WithLatestVersion_ShouldReturnLatest()
    {
        // Arrange
        var v1 = await CreateAndDeployDefinition("key-test");
        var v2 = await CreateAndDeployDefinition("key-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.GetByKeyAsync("key-test");

        // Assert
        Assert.NotNull(result);
        AssertThat(result).HasVersion(2);
    }

    [Fact]
    public async Task GetByKeyAsync_WithSpecificVersion_ShouldReturnThatVersion()
    {
        // Arrange
        var v1 = await CreateAndDeployDefinition("version-test");
        var v2 = await CreateAndDeployDefinition("version-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.GetByKeyAsync("version-test", version: 1);

        // Assert
        Assert.NotNull(result);
        AssertThat(result).HasVersion(1);
    }

    [Fact]
    public async Task DeployAsync_ShouldMarkAsDeployed()
    {
        // Arrange
        var request = CreateValidRequest("deploy-test");
        var created = await _fixture.ProcessDefinitionService.CreateAsync(request);

        // Act
        var result = await _fixture.ProcessDefinitionService.DeployAsync(created.Id);

        // Assert
        AssertThat(result)
            .IsDeployed()
            .IsActive();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDefinition()
    {
        // Arrange
        var created = await CreateDefinition("update-test");
        var updateRequest = new UpdateProcessDefinitionRequest
        {
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Act
        var result = await _fixture.ProcessDefinitionService.UpdateAsync(created.Id, updateRequest);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDefinition()
    {
        // Arrange
        var created = await CreateAndDeployDefinition("delete-test");

        // Act
        await _fixture.ProcessDefinitionService.DeleteAsync(created.Id);

        // Assert
        var result = await _fixture.ProcessDefinitionService.GetByIdAsync(created.Id);
        // Soft delete - may still be retrievable but inactive
        Assert.True(result == null || !result.IsActive);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPagedResults()
    {
        // Arrange
        await CreateAndDeployDefinition("list-test-1");
        await CreateAndDeployDefinition("list-test-2");
        await CreateAndDeployDefinition("list-test-3");

        var query = new ProcessDefinitionListQuery
        {
            PageNumber = 1,
            PageSize = 2
        };

        // Act
        var result = await _fixture.ProcessDefinitionService.ListAsync(query);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.TotalCount >= 3);
    }

    [Fact]
    public async Task ListAsync_WithCategoryFilter_ShouldFilterByCategory()
    {
        // Arrange
        var request = CreateValidRequest("category-test");
        request.Category = "SpecialCategory";
        var created = await _fixture.ProcessDefinitionService.CreateAsync(request);
        await _fixture.ProcessDefinitionService.DeployAsync(created.Id);

        var query = new ProcessDefinitionListQuery
        {
            Category = "SpecialCategory"
        };

        // Act
        var result = await _fixture.ProcessDefinitionService.ListAsync(query);

        // Assert
        Assert.True(result.TotalCount >= 1);
        Assert.All(result.Items, item => Assert.Equal("SpecialCategory", item.Category));
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        await CreateAndDeployDefinition("exists-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.ExistsAsync("exists-test");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingKey_ShouldReturnFalse()
    {
        // Act
        var result = await _fixture.ProcessDefinitionService.ExistsAsync("non-existing-key");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_WithProcessModel_ShouldValidateModel()
    {
        // Arrange
        var request = CreateValidRequest("model-validation-test");

        // Act
        var result = await _fixture.ProcessDefinitionService.CreateAsync(request);

        // Assert
        AssertThat(result)
            .HasStartEvent()
            .HasEndEvent();
    }

    #region Helper Methods

    private CreateProcessDefinitionRequest CreateValidRequest(string key)
    {
        return new CreateProcessDefinitionRequest
        {
            TenantId = "test-tenant",
            Key = key,
            Name = $"Test Process - {key}",
            Description = "A test process definition",
            Category = "Test",
            ProcessModel = new ProcessModel
            {
                Id = key,
                Name = $"Test Process - {key}",
                Elements = new List<ProcessElement>
                {
                    new ProcessElement { Id = "start", Type = "startEvent", Name = "Start" },
                    new ProcessElement { Id = "task1", Type = "userTask", Name = "Task 1" },
                    new ProcessElement { Id = "end", Type = "endEvent", Name = "End" }
                },
                Flows = new List<SequenceFlow>
                {
                    new SequenceFlow { Id = "flow1", SourceRef = "start", TargetRef = "task1" },
                    new SequenceFlow { Id = "flow2", SourceRef = "task1", TargetRef = "end" }
                }
            }
        };
    }

    private async Task<ProcessDefinition> CreateDefinition(string key)
    {
        var request = CreateValidRequest(key);
        return await _fixture.ProcessDefinitionService.CreateAsync(request);
    }

    private async Task<ProcessDefinition> CreateAndDeployDefinition(string key)
    {
        var created = await CreateDefinition(key);
        return await _fixture.ProcessDefinitionService.DeployAsync(created.Id);
    }

    #endregion
}
