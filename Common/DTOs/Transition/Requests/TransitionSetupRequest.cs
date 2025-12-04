using Common.DTOs.Todo.Requests;

namespace Common.DTOs.Transition.Requests;

public class TransitionSetupRequest
{
    public int UserId { get; set; }
    public int TemplateId { get; set; }
    public List<TransitionRuleRequest> TransitionRules { get; set; } = new();
}