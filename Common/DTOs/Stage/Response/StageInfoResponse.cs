namespace Common.DTOs.Stage.Response;

public class StageInfoResponse
{
    public int StageId { get; set; }
    public string StageName { get; set; } = null!;
    public int StageOrder { get; set; }
    public string RequiredRoleName { get; set; } = null!;
    public string SpecificReviewerUserName { get; set; } = null!;
    public bool IsCurrentStage { get; set; }
}