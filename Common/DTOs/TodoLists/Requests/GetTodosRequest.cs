namespace Common.DTOs.TodoLists.Requests;

public class GetTodosRequest
{
    public int CurrentUserId { get; set; }

    public string? Status { get; set; }
}