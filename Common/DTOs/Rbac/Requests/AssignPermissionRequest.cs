namespace Common.DTOs.Rbac.Requests;

public class AssignPermissionRequest
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }
}