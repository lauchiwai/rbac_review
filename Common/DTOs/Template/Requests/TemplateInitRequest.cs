using Common.DTOs.Stage.Requests;
using Common.DTOs.Todo.Requests;

namespace Common.DTOs.Template.Requests;

public class TemplateInitRequest
{
    public int UserId { get; set; }
    public string TemplateName { get; set; } = null!;
    public string? Description { get; set; }
    public List<StageInitRequest> Stages { get; set; } = new();
}
