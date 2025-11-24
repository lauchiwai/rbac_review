namespace Common.DTOs.TodoLists.Requests;

public class UpdateTodoRequest
{
    public int TodoId { get; set; }

    public string Title { get; set; } = null!;

    public int CurrentUserRoleId { get; set; }
}

