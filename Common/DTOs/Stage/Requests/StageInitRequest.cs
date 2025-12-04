namespace Common.DTOs.Stage.Requests;

public class StageInitRequest
{
    public string StageName { get; set; } = null!;
    public int StageOrder { get; set; }
    public int RequiredRoleId { get; set; }
    public int? SpecificReviewerUserId { get; set; }
}
