namespace Common.DTOs.Template.Response;

public class TemplateInitResponse
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int StageCount { get; set; }
}
