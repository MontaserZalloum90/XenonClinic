namespace Xenon.Platform.Domain.Entities;

public class ApiCallLog : BaseEntity
{
    public Guid? TenantId { get; set; }

    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;

    public int StatusCode { get; set; }
    public long DurationMs { get; set; }

    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public long? RequestSize { get; set; }
    public long? ResponseSize { get; set; }

    public string? ErrorMessage { get; set; }
}
