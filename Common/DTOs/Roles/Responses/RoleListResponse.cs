namespace Common.DTOs.Roles.Responses;

public class RoleListResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public int PermissionCount { get; set; }
}