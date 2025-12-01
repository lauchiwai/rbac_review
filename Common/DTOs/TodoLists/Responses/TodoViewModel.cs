namespace Common.DTOs.TodoLists.Responses;

public class TodoViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public int? CurrentReviewerUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
