namespace Common.DTOs.Transition.Requests;

public class TransitionRuleRequest
{
    public int StageId { get; set; }
    public string ActionName { get; set; } = null!;
    public string ResultStatus { get; set; } = null!;
    public int? NextStageId { get; set; }
}