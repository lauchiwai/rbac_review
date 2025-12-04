namespace Common.Models;

public class ReviewStages
{
    public int StageId { get; set; }

    public int TemplateId { get; set; }

    public string StageName { get; set; } = null!;

    public int StageOrder { get; set; }

    public int RequiredRoleId { get; set; } 

    public int? SpecificReviewerUserId { get; set; } 

    public virtual ReviewTemplates ReviewTemplate { get; set; } = null!;

    public virtual Roles RequiredRole { get; set; } = null!;

    public virtual Users? SpecificReviewerUser { get; set; } 

    public virtual ICollection<TodoLists> TodoLists { get; set; } = new List<TodoLists>();

    public virtual ICollection<StageTransitions> FromStageTransitions { get; set; } = new List<StageTransitions>();

    public virtual ICollection<StageTransitions> ToStageTransitions { get; set; } = new List<StageTransitions>();

    public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
}