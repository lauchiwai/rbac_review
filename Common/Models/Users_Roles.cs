namespace Common.Models;

public class Users_Roles
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public virtual Users User { get; set; } = null!;

    public virtual Roles Role { get; set; } = null!;
}