namespace Common.DTOs.Roles.Responses;

public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public List<RolePermissionResponse> RolePermissions { get; set; } = new List<RolePermissionResponse>();
}


public class RolePermissionResponse
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; } = null!;
}
