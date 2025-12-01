namespace Common.DTOs.Users.Responses;

public class UserResponse
{
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UserRoleViewModel> Roles { get; set; } = new List<UserRoleViewModel>();
}