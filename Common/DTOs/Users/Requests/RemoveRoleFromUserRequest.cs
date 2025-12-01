namespace Common.DTOs.Users.Requests;

public class RemoveRoleFromUserRequest
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
