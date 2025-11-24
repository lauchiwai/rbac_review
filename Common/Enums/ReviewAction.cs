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
            Pending => "Pending",
            InProgress => "InProgress",
            Approved => "Approved",
            Rejected => "Rejected",
            Returned => "Returned",
            Completed => "Completed",
            Cancelled => "Cancelled",
            _ => "unknow"
        };
    }

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
