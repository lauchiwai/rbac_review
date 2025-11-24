namespace Common.Enums;
using System.ComponentModel;

public enum Permission
{
    [Description("Allow creating to-do items (corresponds to initial status)")]
    TodoCreate = 8,

    [Description("Allow performing first-level review (e.g., updating status to \"approved\" or \"returned\")")]
    TodoReviewLevel1 = 9,

    [Description("Allow performing second-level review (e.g., updating status to \"approved\" or \"returned\")")]
    TodoReviewLevel2 = 10,

    [Description("Allow viewing own created to-do items")]
    TodoViewOwn = 11,

    [Description("Allow viewing first-level review related items (status: pending, returned)")]
    TodoViewLevel1 = 12,

    [Description("Allow viewing second-level review related items (status: in_progress)")]
    TodoViewLevel2 = 13,

    [Description("Allow managing roles and permissions configuration (admin only)")]
    AdminManage = 14
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
            Permission.TodoViewOwn => "todo_view_own",
            Permission.TodoViewLevel1 => "todo_view_level1",
            Permission.TodoViewLevel2 => "todo_view_level2",
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
