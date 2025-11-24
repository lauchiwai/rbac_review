namespace Common.DTOs.Rbac.Responses;

public class RolePermissionsResponse
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public List<PermissionViewModel> Permissions { get; set; } = new List<PermissionViewModel>();
}

public class PermissionViewModel
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;
}

