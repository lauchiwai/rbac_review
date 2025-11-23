namespace Common.Models;

public class TodoLists
{
    public int TodoListId { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int CreatedByRole { get; set; }

    public DateTime CreatedAt { get; set; }
}
