namespace Common.Enums;

public static class ReviewAction
{
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Returned = "returned";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    // 擴展方法來提供額外功能
    public static bool IsValidStatus(string status)
    {
        return status switch
        {
            Pending => true,
            InProgress => true,
            Approved => true,
            Rejected => true,
            Returned => true,
            Completed => true,
            Cancelled => true,
            _ => false
        };
    }

    public static string GetDisplayName(string status)
    {
        return status switch
        {
            Pending => "待審核",
            InProgress => "審核中",
            Approved => "已批准",
            Rejected => "已拒絕",
            Returned => "已退回",
            Completed => "已完成",
            Cancelled => "已取消",
            _ => "未知狀態"
        };
    }

    public static List<string> GetAllStatuses()
    {
        return new List<string>
        {
            Pending,
            InProgress,
            Approved,
            Rejected,
            Returned,
            Completed,
            Cancelled
        };
    }

    // 狀態轉換規則
    public static bool CanTransitionFromTo(string fromStatus, string toStatus)
    {
        return (fromStatus, toStatus) switch
        {
            (Pending, InProgress) => true,
            (Pending, Approved) => true,
            (Pending, Rejected) => true,
            (Pending, Cancelled) => true,
            (InProgress, Approved) => true,
            (InProgress, Rejected) => true,
            (InProgress, Returned) => true,
            (Approved, Completed) => true,
            (Returned, InProgress) => true,
            (Returned, Pending) => true,
            _ => false
        };
    }
}
