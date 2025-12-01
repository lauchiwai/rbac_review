namespace Common.Models;

public class Roles
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<Roles_Permissions> Roles_Permissions { get; set; } = new List<Roles_Permissions>();

    public virtual ICollection<Users_Roles> Users_Roles { get; set; } = new List<Users_Roles>();
}