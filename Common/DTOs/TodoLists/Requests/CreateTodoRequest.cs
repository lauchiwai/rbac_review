namespace Common.DTOs.TodoLists.Requests;

public class CreateTodoRequest
{
    public string Title { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public int ReviewerUserId { get; set; }
}
