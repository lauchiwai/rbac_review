namespace Common.Models;

public class Permissions
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; } = new List<Roles_Permissions>();
}