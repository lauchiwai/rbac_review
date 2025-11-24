namespace Common.DTOs.Rbac.Requests;

public class CheckPermissionRequest
{
    public int RoleId { get; set; }

    public string PermissionName { get; set; } = null!;
}
