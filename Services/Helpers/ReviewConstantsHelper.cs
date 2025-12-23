namespace Services.Helpers;

public static class ReviewConstantsHelper
{
    // Status constants
    public const string ReturnedToCreator = "returned_to_creator";
    public const string ReturnedToReviewer = "returned_to_reviewer";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string PendingReviewLevel1 = "pending_review_level1";
    public const string PendingReviewLevel2 = "pending_review_level2";
    public const string PendingReviewLevel3 = "pending_review_level3";
    public const string PendingReviewStage1 = "pending_review_stage1";
    public const string PendingReviewStage2 = "pending_review_stage2";
    public const string PendingReviewStage3 = "pending_review_stage3";

    // Action constants
    public const string ActionApprove = "approve";
    public const string ActionReturn = "return";
    public const string ActionReject = "reject";
    public const string ActionResubmit = "resubmit";
    public const string ActionCreated = "created";

    // Permission constants
    public const string PermissionAdminManage = "admin_manage";

    // Display name mappings
    private static readonly Dictionary<string, string> _actionDisplayNames = new()
    {
        [ActionApprove] = "Approve",
        [ActionReturn] = "Return",
        [ActionReject] = "Reject",
        [ActionResubmit] = "Resubmit",
        [ActionCreated] = "Created"
    };

    private static readonly Dictionary<string, string> _statusDisplayNames = new()
    {
        [ReturnedToCreator] = "Returned to Creator",
        [ReturnedToReviewer] = "Returned to Reviewer",
        [PendingReviewLevel1] = "Pending Level 1 Review",
        [PendingReviewLevel2] = "Pending Level 2 Review",
        [PendingReviewLevel3] = "Pending Level 3 Review",
        [PendingReviewStage1] = "Pending Stage 1 Review",
        [PendingReviewStage2] = "Pending Stage 2 Review",
        [PendingReviewStage3] = "Pending Stage 3 Review",
        [Approved] = "Approved",
        [Rejected] = "Rejected"
    };

    public static string GetActionDisplayName(string action)
    {
        return _actionDisplayNames.TryGetValue(action, out var displayName)
            ? displayName
            : action;
    }

    public static string GetStatusDisplayName(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return "Unknown";

        return _statusDisplayNames.TryGetValue(status, out var displayName)
            ? displayName
            : status;
    }

    public static bool IsPendingStatus(string status)
    {
        return status.StartsWith("pending_review");
    }

    public static bool IsReturnedStatus(string status)
    {
        return status == ReturnedToCreator || status == ReturnedToReviewer;
    }

    public static bool IsFinalStatus(string status)
    {
        return status == Approved || status == Rejected;
    }
}