namespace Common.DTOs.Rbac.Responses;

public class PermissionCheckResponse
{
    public int RoleId { get; set; }

    public string PermissionName { get; set; } = null!;

    public bool HasPermission { get; set; }

    public string Message { get; set; } = null!;
}
