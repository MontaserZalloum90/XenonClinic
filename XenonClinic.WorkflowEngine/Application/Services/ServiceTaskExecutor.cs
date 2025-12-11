namespace XenonClinic.WorkflowEngine.Application.Services;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service implementation for executing service tasks.
/// </summary>
public class ServiceTaskExecutor : IServiceTaskExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExpressionEvaluator _expressionEvaluator;
    private readonly ILogger<ServiceTaskExecutor> _logger;
    private readonly ConcurrentDictionary<string, IServiceHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly JsonSerializerOptions _jsonOptions;

    public ServiceTaskExecutor(
        IHttpClientFactory httpClientFactory,
        IExpressionEvaluator expressionEvaluator,
        ILogger<ServiceTaskExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _expressionEvaluator = expressionEvaluator;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public void RegisterHandler(string serviceName, IServiceHandler handler)
    {
        _handlers[serviceName] = handler;
        _logger.LogInformation("Registered service handler: {ServiceName}", serviceName);
    }

    public async Task<ServiceTaskResult> ExecuteAsync(
        Guid? processInstanceId,
        string? activityInstanceId,
        Dictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check for registered service handler
            if (parameters.TryGetValue("serviceName", out var serviceNameObj) &&
                serviceNameObj is string serviceName &&
                _handlers.TryGetValue(serviceName, out var handler))
            {
                _logger.LogInformation(
                    "Executing service handler {ServiceName} for process {ProcessInstanceId}",
                    serviceName, processInstanceId);

                var result = await handler.ExecuteAsync(parameters, cancellationToken);
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Check for HTTP endpoint
            if (parameters.TryGetValue("httpEndpoint", out var endpointObj) ||
                parameters.TryGetValue("url", out endpointObj))
            {
                var request = BuildHttpRequest(parameters);
                return await ExecuteHttpAsync(request, cancellationToken);
            }

            _logger.LogWarning(
                "No handler or endpoint found for service task in process {ProcessInstanceId}",
                processInstanceId);

            return ServiceTaskResult.Fail("No service handler or HTTP endpoint configured");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error executing service task for process {ProcessInstanceId}",
                processInstanceId);

            return new ServiceTaskResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
    }

    public async Task<ServiceTaskResult> ExecuteHttpAsync(
        HttpServiceTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient("WorkflowServiceTask");
            client.Timeout = request.Timeout;

            // Build URL with query parameters
            var url = request.Url;
            if (request.QueryParameters?.Count > 0)
            {
                var query = string.Join("&",
                    request.QueryParameters.Select(kvp =>
                        $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url = url.Contains('?') ? $"{url}&{query}" : $"{url}?{query}";
            }

            // Create request message
            var httpRequest = new HttpRequestMessage(
                new HttpMethod(request.Method.ToUpperInvariant()),
                url);

            // Add headers
            if (request.Headers?.Count > 0)
            {
                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Add body for non-GET requests
            if (request.Body != null && request.Method.ToUpperInvariant() != "GET")
            {
                var json = request.Body is string s ? s : JsonSerializer.Serialize(request.Body, _jsonOptions);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, request.ContentType ?? "application/json");
            }

            _logger.LogInformation(
                "Executing HTTP {Method} to {Url}",
                request.Method, request.Url);

            // Execute request
            var response = await client.SendAsync(httpRequest, cancellationToken);
            stopwatch.Stop();

            // Extract response headers
            var responseHeaders = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            // Check success
            var successCodes = request.SuccessStatusCodes ?? Enumerable.Range(200, 100).ToList();
            var isSuccess = successCodes.Contains((int)response.StatusCode);

            // Read response body
            object? responseData = null;
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrEmpty(responseContent))
            {
                try
                {
                    responseData = JsonSerializer.Deserialize<JsonElement>(responseContent, _jsonOptions);

                    // Extract result path if specified
                    if (!string.IsNullOrEmpty(request.ResultPath) && responseData is JsonElement jsonElement)
                    {
                        responseData = ExtractJsonPath(jsonElement, request.ResultPath);
                    }
                }
                catch (JsonException)
                {
                    // Response is not valid JSON, keep as string
                    responseData = responseContent;
                }
            }

            if (!isSuccess && request.ThrowOnError)
            {
                var errorMessage = responseData?.ToString() ?? $"HTTP {(int)response.StatusCode}";
                throw new HttpRequestException($"Service call failed: {errorMessage}", null, response.StatusCode);
            }

            _logger.LogInformation(
                "HTTP {Method} to {Url} completed with status {StatusCode} in {Duration}ms",
                request.Method, request.Url, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            return new ServiceTaskResult
            {
                Success = isSuccess,
                StatusCode = (int)response.StatusCode,
                Data = responseData,
                ResponseHeaders = responseHeaders,
                Duration = stopwatch.Elapsed,
                ErrorMessage = isSuccess ? null : responseData?.ToString()
            };
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("HTTP request to {Url} timed out", request.Url);
            return new ServiceTaskResult
            {
                Success = false,
                ErrorMessage = "Request timed out",
                Duration = stopwatch.Elapsed
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to {Url} failed", request.Url);
            return new ServiceTaskResult
            {
                Success = false,
                StatusCode = ex.StatusCode.HasValue ? (int)ex.StatusCode : null,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing HTTP request to {Url}", request.Url);
            return new ServiceTaskResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Duration = stopwatch.Elapsed
            };
        }
    }

    private HttpServiceTaskRequest BuildHttpRequest(Dictionary<string, object> parameters)
    {
        var request = new HttpServiceTaskRequest();

        if (parameters.TryGetValue("httpEndpoint", out var endpoint) ||
            parameters.TryGetValue("url", out endpoint))
        {
            request.Url = endpoint?.ToString() ?? "";
        }

        if (parameters.TryGetValue("httpMethod", out var method) ||
            parameters.TryGetValue("method", out method))
        {
            request.Method = method?.ToString() ?? "GET";
        }

        if (parameters.TryGetValue("headers", out var headers))
        {
            request.Headers = ConvertToDictionary(headers);
        }

        if (parameters.TryGetValue("queryParameters", out var queryParams) ||
            parameters.TryGetValue("query", out queryParams))
        {
            request.QueryParameters = ConvertToDictionary(queryParams);
        }

        if (parameters.TryGetValue("body", out var body) ||
            parameters.TryGetValue("payload", out body))
        {
            request.Body = body;
        }

        if (parameters.TryGetValue("contentType", out var contentType))
        {
            request.ContentType = contentType?.ToString();
        }

        if (parameters.TryGetValue("timeout", out var timeout))
        {
            if (timeout is int timeoutSeconds)
            {
                request.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }
            else if (timeout is TimeSpan ts)
            {
                request.Timeout = ts;
            }
        }

        if (parameters.TryGetValue("resultPath", out var resultPath))
        {
            request.ResultPath = resultPath?.ToString();
        }

        if (parameters.TryGetValue("throwOnError", out var throwOnError))
        {
            request.ThrowOnError = throwOnError is true or "true";
        }

        return request;
    }

    private static Dictionary<string, string>? ConvertToDictionary(object? value)
    {
        if (value == null)
            return null;

        if (value is Dictionary<string, string> dict)
            return dict;

        if (value is Dictionary<string, object> objDict)
            return objDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "");

        if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
        {
            var result = new Dictionary<string, string>();
            foreach (var prop in jsonElement.EnumerateObject())
            {
                result[prop.Name] = prop.Value.ToString();
            }
            return result;
        }

        return null;
    }

    private static object? ExtractJsonPath(JsonElement element, string path)
    {
        var parts = path.Split('.');
        var current = element;

        foreach (var part in parts)
        {
            if (current.ValueKind != JsonValueKind.Object)
                return null;

            if (!current.TryGetProperty(part, out current))
                return null;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => current
        };
    }
}
