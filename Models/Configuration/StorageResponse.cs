namespace CloudflareSuperSites.Models.Configuration;

public class StorageResponse
{
    public string GenerateResponseCode { get; set; }

    public string GlobalCode { get; set; }

    public string PreloadCode { get; set; }

    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
}