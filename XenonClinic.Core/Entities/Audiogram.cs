namespace XenonClinic.Core.Entities;

public class Audiogram
{
    public int Id { get; set; }
    public int AudiologyVisitId { get; set; }
    public string RawDataJson { get; set; } = "{}";
    public string? Notes { get; set; }

    public AudiologyVisit? Visit { get; set; }
}
