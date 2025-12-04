namespace Common.Models;

public class StageTransitions
{
    public int TransitionId { get; set; }

    public int StageId { get; set; }

    public string ActionName { get; set; } = null!;

    public int? NextStageId { get; set; } 

    public string ResultStatus { get; set; } = null!;

    public virtual ReviewStages FromStage { get; set; } = null!;

    public virtual ReviewStages? ToStage { get; set; }
}