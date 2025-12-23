namespace Common.DTOs.Template.Response;

public class TemplateResponse
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; }

    public string Description { get; set; }

    public int StageCount { get; set; }

    public DateTime CreatedAt { get; set; }
}
