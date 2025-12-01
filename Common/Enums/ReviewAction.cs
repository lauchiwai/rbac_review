namespace Common.Enums;

public static class ReviewAction
{
    public const string Pending = "pending";                   // Waiting for Level 1 review
    public const string InProgress = "in_progress";            // Level 1 approved, waiting for Level 2 review
    public const string Approved = "approved";                 // Level 2 approved
    public const string Rejected = "rejected";                 // Rejected at any level
    public const string Returned = "returned";                 // Returned to creator
    public const string ReturnedToLevel1 = "returned_to_level1"; // Level 2 returned to Level 1
    public const string Completed = "completed";               // Completed
    public const string Cancelled = "cancelled";               // Cancelled

    public static bool IsValidStatus(string status)
    {
        return status switch
        {
            Pending => true,
            InProgress => true,
            Approved => true,
            Rejected => true,
            Returned => true,
            ReturnedToLevel1 => true,
            Completed => true,
            Cancelled => true,
            _ => false
        };
    }

    public static string GetDisplayName(string status)
    {
        return status switch
        {
            Pending => "Pending (Level 1)",
            InProgress => "In Progress (Level 2)",
            Approved => "Approved (Level 2)",
            Rejected => "Rejected",
            Returned => "Returned to Creator",
            ReturnedToLevel1 => "Returned to Level 1",
            Completed => "Completed",
            Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    public static bool CanTransitionFromTo(string fromStatus, string toStatus)
    {
        return (fromStatus, toStatus) switch
        {
            // After creator submits, enter Level 1 review
            (Pending, InProgress) => true,           // Level 1 approved
            (Pending, Returned) => true,             // Level 1 returned to creator
            (Pending, Rejected) => true,             // Level 1 rejected

            // After entering Level 2 review
            (InProgress, Approved) => true,          // Level 2 approved
            (InProgress, Rejected) => true,          // Level 2 rejected
            (InProgress, ReturnedToLevel1) => true,  // Level 2 returned to Level 1

            // After Level 2 returns to Level 1
            (ReturnedToLevel1, InProgress) => true,  // Level 1 re-approved
            (ReturnedToLevel1, Returned) => true,    // Level 1 returned to creator
            (ReturnedToLevel1, Rejected) => true,    // Level 1 rejected

            // After Level 2 approval
            (Approved, Completed) => true,           // Final processing completed

            // Return-related transitions
            (Returned, Pending) => true,             // Creator resubmits

            _ => false
        };
    }

    // Get review level
    public static int GetReviewLevel(string status)
    {
        return status switch
        {
            Pending => 1,              // Level 1 review
            InProgress => 2,           // Level 2 review
            ReturnedToLevel1 => 1,     // Returned to Level 1, requires Level 1 review
            Returned => 0,             // Returned to creator
            _ => 0                     // Other statuses
        };
    }

    // Check if Level 1 review is required
    public static bool RequiresLevel1Review(string status)
    {
        return status == Pending || status == ReturnedToLevel1;
    }

    // Check if Level 2 review is required
    public static bool RequiresLevel2Review(string status)
    {
        return status == InProgress;
    }

    // Check if it's a returned status
    public static bool IsReturnedStatus(string status)
    {
        return status == Returned || status == ReturnedToLevel1;
    }
}