namespace Common.Enums;
using System.ComponentModel;

public enum Permission
{
    [Description("Allow creating to-do items (corresponds to initial status)")]
    TodoCreate = 1,

    [Description("Allow performing first-level review (e.g., updating status to \"approved\" or \"returned\")")]
    TodoReviewLevel1 = 2,

    [Description("Allow performing second-level review (e.g., updating status to \"approved\" or \"returned\")")]
    TodoReviewLevel2 = 3,

    [Description("Allow managing roles and permissions configuration (admin only)")]
    AdminManage = 4
}

public static class PermissionExtensions
{
    public static string GetPermissionName(this Permission permission)
    {
        return permission switch
        {
            Permission.TodoCreate => "todo_create",
            Permission.TodoReviewLevel1 => "todo_review_level1",
            Permission.TodoReviewLevel2 => "todo_review_level2",
            Permission.AdminManage => "admin_manage",
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
        };
    }

    public static string GetDescription(this Permission permission)
    {
        var field = permission.GetType().GetField(permission.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? permission.ToString();
    }
}
