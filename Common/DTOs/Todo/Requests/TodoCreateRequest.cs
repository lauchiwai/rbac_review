namespace Common.DTOs.Todo.Requests;

public class TodoCreateRequest
{
    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public int TemplateId { get; set; }

    public int? CurrentReviewerUserId { get; set; }
}
