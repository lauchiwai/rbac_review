namespace Common.DTOs.TodoLists.Requests;

public class GetTodosRequest
{
    public int CurrentUserRoleId { get; set; }

    public string? Status { get; set; }
}