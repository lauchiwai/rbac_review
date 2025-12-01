namespace Common.Models;

public class Users
{
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Users_Roles> Users_Roles { get; set; } = new List<Users_Roles>();
}