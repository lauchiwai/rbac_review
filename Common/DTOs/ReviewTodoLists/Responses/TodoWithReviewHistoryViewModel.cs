namespace Common.DTOs.ReviewTodoLists.Responses;

public class TodoWithReviewHistoryViewModel
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Status { get; set; }

    public int CreatedByRoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<ReviewHistoryViewModel> ReviewHistories { get; set; }
}

public class ReviewHistoryViewModel
{
    public int ReviewId { get; set; }

    public int ReviewerRoleId { get; set; }

    public string ReviewerRoleName { get; set; }

    public string Action { get; set; }

    public string Comment { get; set; }

    public string PreviousStatus { get; set; }

    public string NewStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}