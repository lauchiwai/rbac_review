namespace Common.DTOs.Rbac.Requests;

public class RemovePermissionRequest
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }
}
