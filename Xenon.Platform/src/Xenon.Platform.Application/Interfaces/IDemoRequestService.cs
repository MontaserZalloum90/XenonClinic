using Xenon.Platform.Application.DTOs;

namespace Xenon.Platform.Application.Interfaces;

/// <summary>
/// Service for handling demo requests and contact inquiries
/// </summary>
public interface IDemoRequestService
{
    /// <summary>
    /// Submit a new demo request
    /// </summary>
    /// <param name="request">Demo request details</param>
    /// <param name="clientIpAddress">Client IP for tracking</param>
    /// <param name="userAgent">User agent for tracking</param>
    /// <returns>Result indicating success or honeypot detection</returns>
    Task<Result<DemoRequestResponse>> SubmitAsync(
        DemoRequestSubmission request,
        string? clientIpAddress = null,
        string? userAgent = null);
}
