namespace Common.Models;

public class Roles_Permissions
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public virtual Roles Role { get; set; } = null!;

    public virtual Permissions Permission { get; set; } = null!;
}
