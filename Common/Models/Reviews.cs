namespace Common.Models;

public class Reviews
{
    public int ReviewId { get; set; }

    public int TodoId { get; set; }

    public int ReviewerUserId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime ReviewedAt { get; set; }

    public string? Comment { get; set; }

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public int? StageId { get; set; }

    public virtual ReviewStages? ReviewStage { get; set; }

    public virtual TodoLists Todo { get; set; } = null!;

    public virtual Users ReviewerUser { get; set; } = null!;
}