namespace Common.Models;

public class TodoLists
{
    public int TodoListId { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public int? CurrentReviewerUserId { get; set; }  

    public DateTime CreatedAt { get; set; }

    public virtual Users CreatedByUser { get; set; } = null!;

    public virtual Users? CurrentReviewerUser { get; set; }  
}