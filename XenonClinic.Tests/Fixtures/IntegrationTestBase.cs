using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using XenonClinic.Api.Controllers;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Tests.Fixtures;

/// <summary>
/// Base class for integration tests providing common utilities and setup.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<XenonClinicWebApplicationFactory>, IDisposable
{
    protected readonly XenonClinicWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(XenonClinicWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateApiClient();
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Gets a scoped service from the test service provider.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Executes an action within a database scope.
    /// </summary>
    protected async Task ExecuteWithDbContextAsync(Func<ClinicDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        await action(context);
    }

    /// <summary>
    /// Gets data from an API endpoint and deserializes the response.
    /// </summary>
    protected async Task<ApiResponse<T>?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
    }

    /// <summary>
    /// Posts data to an API endpoint and deserializes the response.
    /// </summary>
    protected async Task<(HttpResponseMessage Response, ApiResponse<T>? Data)> PostAsync<T>(
        string url,
        object content)
    {
        var response = await Client.PostAsJsonAsync(url, content, JsonOptions);
        var data = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions)
            : null;
        return (response, data);
    }

    /// <summary>
    /// Puts data to an API endpoint and deserializes the response.
    /// </summary>
    protected async Task<(HttpResponseMessage Response, ApiResponse<T>? Data)> PutAsync<T>(
        string url,
        object content)
    {
        var response = await Client.PutAsJsonAsync(url, content, JsonOptions);
        var data = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions)
            : null;
        return (response, data);
    }

    /// <summary>
    /// Deletes a resource at the specified URL.
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    /// <summary>
    /// Asserts that the API response was successful.
    /// </summary>
    protected static void AssertSuccess<T>(ApiResponse<T>? response)
    {
        Assert.NotNull(response);
        Assert.True(response.Success, response.Error ?? "Request failed");
        Assert.NotNull(response.Data);
    }

    /// <summary>
    /// Asserts that the API response was a failure.
    /// </summary>
    protected static void AssertFailure<T>(ApiResponse<T>? response, string? expectedError = null)
    {
        Assert.NotNull(response);
        Assert.False(response.Success);
        if (expectedError != null)
        {
            Assert.Contains(expectedError, response.Error ?? "");
        }
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
