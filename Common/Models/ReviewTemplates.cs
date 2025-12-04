namespace Common.Models;

public class ReviewTemplates
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual Users? CreatedByUser { get; set; }

    public virtual ICollection<ReviewStages> ReviewStages { get; set; } = new List<ReviewStages>();

    public virtual ICollection<TodoLists> TodoLists { get; set; } = new List<TodoLists>();
}