namespace Common.DTOs.Users.Responses;

public class UserRolesResponse
{
    public int UserId { get; set; }
    public List<UserRoleViewModel> Roles { get; set; } = new List<UserRoleViewModel>();
}
