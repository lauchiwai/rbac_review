namespace Common.DTOs.Todo.Response;

public class TodoCreateResponse
{
    public int TodoListId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? CurrentStageId { get; set; }
    public int? CurrentReviewerUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CurrentStageName { get; set; } = null!;
    public string CurrentReviewerName { get; set; } = null!;
}
