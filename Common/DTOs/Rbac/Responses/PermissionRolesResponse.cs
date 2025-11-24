namespace Common.DTOs.Rbac.Responses;

public class PermissionRolesResponse
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();
}

public class RoleViewModel
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;
}