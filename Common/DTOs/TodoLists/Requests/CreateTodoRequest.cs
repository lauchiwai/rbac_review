namespace Common.DTOs.TodoLists.Requests;

public class CreateTodoRequest
{
    public string Title { get; set; } = null!;

    public int CreatedByRoleId { get; set; }
}
