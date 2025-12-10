using Microsoft.AspNetCore.Mvc;
using Xenon.Platform.Application.DTOs;
using Xenon.Platform.Application.Interfaces;

namespace Xenon.Platform.Api.Controllers.Public;

[ApiController]
[Route("api/public/demo-request")]
public class DemoRequestController : ControllerBase
{
    private readonly IDemoRequestService _demoRequestService;

    public DemoRequestController(IDemoRequestService demoRequestService)
    {
        _demoRequestService = demoRequestService;
    }

    /// <summary>
    /// Submit a demo request or contact inquiry
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitDemoRequest([FromBody] DemoRequestSubmission request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _demoRequestService.SubmitAsync(request, clientIp, userAgent);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, error = result.Error });
        }

        return Ok(new
        {
            success = true,
            message = "Thank you for your interest! Our team will contact you shortly.",
            data = result.Value
        });
    }
}
