namespace Common.Enums;
using System.ComponentModel;

public enum Permission
{
    [Description("允許創建待辦事項（對應狀態初始化）")]
    TodoCreate = 8,

    [Description("允許執行一級審核（例如更新狀態為 \"approved\" 或 \"returned\"）")]
    TodoReviewLevel1 = 9,

    [Description("允許執行二級審核（例如更新狀態為 \"approved\" 或 \"returned\"）")]
    TodoReviewLevel2 = 10,

    [Description("允許查看所有待辦事項（用於監控和追蹤）")]
    TodoView = 11,

    [Description("允許管理角色和權限配置（僅管理員使用）")]
    AdminManage = 12
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
            Permission.TodoView => "todo_view",
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

