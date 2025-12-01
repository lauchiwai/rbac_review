namespace Common.DTOs.Users.Responses;

public class RoleUsersResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
}