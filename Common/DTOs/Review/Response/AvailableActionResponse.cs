namespace Common.DTOs.Review.Response;

public class AvailableActionResponse
{
    public string ActionName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string ResultStatus { get; set; } = null!;
    public string? NextStageName { get; set; }
}