namespace Common.DTOs.Users.Requests;

public class AssignRoleToUserRequest
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
