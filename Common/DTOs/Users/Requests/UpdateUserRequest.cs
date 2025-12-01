namespace Common.DTOs.Users.Requests;

public class UpdateUserRequest
{
    public int UserId { get; set; }
    public string UserCode { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
