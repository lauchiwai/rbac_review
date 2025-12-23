using Common.DTOs.Review.Response;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.MyRepository;
using System.Collections.Concurrent;

namespace Services.Helpers;

public interface IReviewHelper
{
    Task<Dictionary<int, ReviewStages>> GetFirstStagesByTemplateIdsAsync(List<int> templateIds);

    Task<ReviewStages?> GetStageByTemplateAndOrderAsync(int templateId, int stageOrder);

    Task<Reviews?> FindPreviousReview(int todoId, int? currentStageId, int currentUserId);

    Task<List<AvailableActionResponse>> GetAvailableActionsForTodoAsync(
        TodoLists todo,
        int userId,
        Dictionary<int, ReviewStages>? stagesDict = null,
        Dictionary<int, List<StageTransitions>>? transitionsDict = null);

    Task HandleReturnAction(TodoLists todo, StageTransitions transition, int currentUserId);

    Task HandleApproveAction(TodoLists todo, StageTransitions transition, int? nextReviewerId = null);

    Task HandleRejectAction(TodoLists todo);

    Task ReturnToCreator(TodoLists todo);

    ReviewSummaryResponse CalculateReviewSummary(TodoLists todo, List<ReviewTimelineItemResponse> timeline);

    Task<int?> FindReviewerByRoleAsync(int requiredRoleId, int todoListId, int excludeUserId);

    Task<Dictionary<int, ReviewStages>> GetNextStagesByIdsAsync(List<int> stageIds);

    Task<Dictionary<int, bool>> CheckUsersHaveRoleAsync(List<int> userIds, int requiredRoleId);

    Task<Dictionary<int, int>> GetReviewersWorkloadAsync(List<int> reviewerIds);
}

public class ReviewHelper : IReviewHelper
{
    private readonly IRepository<ReviewStages> _stageRepository;
    private readonly IRepository<StageTransitions> _transitionRepository;
    private readonly IRepository<Reviews> _reviewRepository;
    private readonly IRepository<Users_Roles> _userRoleRepository;
    private readonly IRepository<TodoLists> _todoRepository;
    private readonly IReviewQueryHelper _queryHelper;

    private static readonly ConcurrentDictionary<int, int> _workloadCache = new();
    private static readonly ConcurrentDictionary<string, Dictionary<int, bool>> _userRoleCache = new();
    private static readonly DateTime _lastCacheClear = DateTime.UtcNow;

    public ReviewHelper(
        IRepository<ReviewStages> stageRepository,
        IRepository<StageTransitions> transitionRepository,
        IRepository<Reviews> reviewRepository,
        IRepository<Users_Roles> userRoleRepository,
        IRepository<TodoLists> todoRepository,
        IReviewQueryHelper queryHelper)
    {
        _stageRepository = stageRepository;
        _transitionRepository = transitionRepository;
        _reviewRepository = reviewRepository;
        _userRoleRepository = userRoleRepository;
        _todoRepository = todoRepository;
        _queryHelper = queryHelper;
    }

    public async Task<Dictionary<int, ReviewStages>> GetFirstStagesByTemplateIdsAsync(List<int> templateIds)
    {
        if (!templateIds.Any())
            return new Dictionary<int, ReviewStages>();

        var query = _stageRepository.GetQueryable()
            .Where(s => templateIds.Contains(s.TemplateId) && s.StageOrder == 1);

        var firstStages = await query.ToListAsync();
        return firstStages.ToDictionary(s => s.TemplateId, s => s);
    }

    public async Task<ReviewStages?> GetStageByTemplateAndOrderAsync(int templateId, int stageOrder)
    {
        var stages = await _stageRepository.FindAsync(s =>
            s.TemplateId == templateId && s.StageOrder == stageOrder);
        return stages.FirstOrDefault();
    }

    public async Task<Reviews?> FindPreviousReview(int todoId, int? currentStageId, int currentUserId)
    {
        if (!currentStageId.HasValue)
            return null;
        var query = _reviewRepository.GetQueryable()
            .Where(r => r.TodoId == todoId &&
                       r.StageId != currentStageId.Value &&
                       r.ReviewerUserId != currentUserId &&
                       r.Action == "approve")
            .OrderByDescending(r => r.ReviewedAt);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<List<AvailableActionResponse>> GetAvailableActionsForTodoAsync(
        TodoLists todo,
        int userId,
        Dictionary<int, ReviewStages>? stagesDict = null,
        Dictionary<int, List<StageTransitions>>? transitionsDict = null)
    {
        var actions = new List<AvailableActionResponse>();

        // If in returned status
        if (ReviewConstantsHelper.IsReturnedStatus(todo.Status))
        {
            // Check user permissions
            if (todo.Status == ReviewConstantsHelper.ReturnedToCreator && todo.CreatedByUserId == userId)
            {
                // Returned to creator, can resubmit to first stage
                var firstStage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 1);
                if (firstStage != null)
                {
                    actions.Add(new AvailableActionResponse
                    {
                        ActionName = ReviewConstantsHelper.ActionResubmit,
                        DisplayName = ReviewConstantsHelper.GetActionDisplayName(ReviewConstantsHelper.ActionResubmit),
                        ResultStatus = ReviewConstantsHelper.PendingReviewLevel1,
                        NextStageName = firstStage.StageName
                    });
                }
            }
            else if (todo.Status == ReviewConstantsHelper.ReturnedToReviewer && todo.CurrentReviewerUserId == userId)
            {
                // Returned to reviewer, can resubmit
                if (todo.CurrentStageId.HasValue)
                {
                    var currentStage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
                    if (currentStage != null)
                    {
                        actions.Add(new AvailableActionResponse
                        {
                            ActionName = ReviewConstantsHelper.ActionResubmit,
                            DisplayName = ReviewConstantsHelper.GetActionDisplayName(ReviewConstantsHelper.ActionResubmit),
                            ResultStatus = $"pending_review_stage{todo.CurrentStageId}",
                            NextStageName = currentStage.StageName
                        });
                    }
                }
            }
            return actions;
        }

        // Normal pending todo item available actions
        if (todo.CurrentStageId == null || todo.CurrentReviewerUserId != userId)
            return actions;

        // Check if user has review permission for this stage
        ReviewStages? stage = null;
        if (stagesDict != null && todo.CurrentStageId.HasValue)
        {
            stagesDict.TryGetValue(todo.CurrentStageId.Value, out stage);
        }
        else if (todo.CurrentStageId.HasValue)
        {
            stage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
        }

        if (stage == null) return actions;

        var userRoles = await _queryHelper.GetUserRolesAsync(userId);
        if (!userRoles.Contains(stage.RequiredRoleId))
            return actions;

        // If there's a specific reviewer, check if it's the current reviewer
        if (stage.SpecificReviewerUserId.HasValue && stage.SpecificReviewerUserId.Value != userId)
            return actions;

        // Get transition rules
        List<StageTransitions> stageTransitions;
        if (transitionsDict != null && todo.CurrentStageId.HasValue &&
            transitionsDict.TryGetValue(todo.CurrentStageId.Value, out var transitions))
        {
            stageTransitions = transitions;
        }
        else
        {
            stageTransitions = (await _transitionRepository.FindAsync(
                t => t.StageId == todo.CurrentStageId)).ToList();
        }

        if (!stageTransitions.Any())
            return actions;

        // Collect next stage IDs
        var nextStageIds = stageTransitions.Where(t => t.NextStageId.HasValue)
                                         .Select(t => t.NextStageId.Value)
                                         .Distinct()
                                         .ToList();

        // Batch query next stage information
        var nextStagesDict = await GetNextStagesByIdsAsync(nextStageIds);

        foreach (var transition in stageTransitions)
        {
            string? nextStageName = null;
            if (transition.NextStageId.HasValue)
            {
                nextStagesDict.TryGetValue(transition.NextStageId.Value, out var nextStage);
                nextStageName = nextStage?.StageName;
            }

            actions.Add(new AvailableActionResponse
            {
                ActionName = transition.ActionName,
                DisplayName = ReviewConstantsHelper.GetActionDisplayName(transition.ActionName),
                ResultStatus = transition.ResultStatus,
                NextStageName = nextStageName
            });
        }

        return actions;
    }

    public async Task HandleReturnAction(TodoLists todo, StageTransitions transition, int currentUserId)
    {
        var previousReview = await FindPreviousReview(todo.TodoListId, todo.CurrentStageId, currentUserId);

        if (previousReview != null)
        {
            todo.CurrentStageId = previousReview.StageId;
            todo.CurrentReviewerUserId = previousReview.ReviewerUserId;
            todo.Status = ReviewConstantsHelper.ReturnedToReviewer;
        }
        else
        {
            await ReturnToCreator(todo);
        }
    }

    public async Task HandleApproveAction(TodoLists todo, StageTransitions transition, int? nextReviewerId = null)
    {
        if (transition.NextStageId.HasValue)
        {
            var nextStage = await _stageRepository.GetByIdAsync(transition.NextStageId.Value);
            if (nextStage != null)
            {
                todo.CurrentStageId = nextStage.StageId;
                todo.Status = transition.ResultStatus;

                // Set reviewer according to priority order
                todo.CurrentReviewerUserId = await DetermineNextReviewerAsync(
                    nextStage,
                    nextReviewerId,
                    todo.TodoListId);
            }
            else
            {
                todo.CurrentStageId = null;
                todo.CurrentReviewerUserId = null;
                todo.Status = ReviewConstantsHelper.Approved;
            }
        }
        else
        {
            todo.CurrentStageId = null;
            todo.CurrentReviewerUserId = null;
            todo.Status = ReviewConstantsHelper.Approved;
        }
    }

    public async Task HandleRejectAction(TodoLists todo)
    {
        todo.CurrentStageId = null;
        todo.CurrentReviewerUserId = null;
        todo.Status = ReviewConstantsHelper.Rejected;
    }

    public async Task ReturnToCreator(TodoLists todo)
    {
        todo.CurrentStageId = null;
        todo.CurrentReviewerUserId = todo.CreatedByUserId;
        todo.Status = ReviewConstantsHelper.ReturnedToCreator;
    }

    public ReviewSummaryResponse CalculateReviewSummary(TodoLists todo, List<ReviewTimelineItemResponse> timeline)
    {
        var summary = new ReviewSummaryResponse
        {
            CurrentStatus = ReviewConstantsHelper.GetStatusDisplayName(todo.Status)
        };

        if (timeline.Any())
        {
            var reviewItems = timeline.Where(t => t.Action != ReviewConstantsHelper.ActionCreated).ToList();

            if (reviewItems.Any())
            {
                var firstReview = reviewItems.Min(t => t.Time);
                var lastReview = reviewItems.Max(t => t.Time);
                var duration = lastReview - firstReview;

                summary.TotalReviews = reviewItems.Count;
                summary.FirstReviewDate = firstReview.ToString("yyyy-MM-dd HH:mm:ss");
                summary.LastReviewDate = lastReview.ToString("yyyy-MM-dd HH:mm:ss");
                summary.TotalDuration = $"{duration.Days} days {duration.Hours} hours {duration.Minutes} minutes";
                summary.ApprovalCount = reviewItems.Count(r => r.Action == ReviewConstantsHelper.ActionApprove);
                summary.ReturnCount = reviewItems.Count(r => r.Action == ReviewConstantsHelper.ActionReturn);
                summary.RejectCount = reviewItems.Count(r => r.Action == ReviewConstantsHelper.ActionReject);
            }
            else
            {
                summary.TotalReviews = 0;
                summary.FirstReviewDate = "Review has not started yet";
                summary.LastReviewDate = "Review has not started yet";
                summary.TotalDuration = "0 days 0 hours 0 minutes";
                summary.ApprovalCount = 0;
                summary.ReturnCount = 0;
                summary.RejectCount = 0;
            }
        }

        return summary;
    }

    public async Task<int?> FindReviewerByRoleAsync(int requiredRoleId, int todoListId, int excludeUserId)
    {
        try
        {
            // Find all users with this role
            var userRoles = await _userRoleRepository.FindAsync(
                ur => ur.RoleId == requiredRoleId && ur.UserId != excludeUserId);

            if (!userRoles.Any())
                return null;

            var userIds = userRoles.Select(ur => ur.UserId).Distinct().ToList();

            // Batch get workload
            var workloads = await GetReviewersWorkloadAsync(userIds);

            // Select reviewer with lowest workload
            var selectedReviewer = workloads
                .OrderBy(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key)
                .FirstOrDefault();

            return selectedReviewer.Key != 0 ? selectedReviewer.Key : userIds.FirstOrDefault();
        }
        catch (Exception)
        {
            // If error, return first user with this role
            var userRoles = await _userRoleRepository.FindAsync(
                ur => ur.RoleId == requiredRoleId && ur.UserId != excludeUserId);

            return userRoles.FirstOrDefault()?.UserId;
        }
    }

    public async Task<Dictionary<int, ReviewStages>> GetNextStagesByIdsAsync(List<int> stageIds)
    {
        if (!stageIds.Any())
            return new Dictionary<int, ReviewStages>();

        // Batch query to avoid N+1 problem
        var stages = await _stageRepository.FindAsync(s => stageIds.Contains(s.StageId));
        return stages.ToDictionary(s => s.StageId, s => s);
    }

    public async Task<Dictionary<int, bool>> CheckUsersHaveRoleAsync(List<int> userIds, int requiredRoleId)
    {
        if (!userIds.Any())
            return new Dictionary<int, bool>();

        // Check cache
        var cacheKey = $"{requiredRoleId}_{string.Join(",", userIds.OrderBy(id => id))}";
        if (_userRoleCache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        // Batch query user roles
        var userRoles = await _userRoleRepository.FindAsync(
            ur => userIds.Contains(ur.UserId) && ur.RoleId == requiredRoleId);

        var result = userRoles
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => true);

        // Fill in users who don't have this role
        foreach (var userId in userIds)
        {
            if (!result.ContainsKey(userId))
            {
                result[userId] = false;
            }
        }

        // Add to cache (simple cache strategy)
        _userRoleCache[cacheKey] = result;

        // Regular cache clearing
        if ((DateTime.UtcNow - _lastCacheClear).TotalMinutes > 10)
        {
            _userRoleCache.Clear();
        }

        return result;
    }

    public async Task<Dictionary<int, int>> GetReviewersWorkloadAsync(List<int> reviewerIds)
    {
        if (!reviewerIds.Any())
            return new Dictionary<int, int>();

        var result = new Dictionary<int, int>();

        // First get partial data from cache
        var uncachedIds = new List<int>();
        foreach (var reviewerId in reviewerIds)
        {
            if (_workloadCache.TryGetValue(reviewerId, out var workload))
            {
                result[reviewerId] = workload;
            }
            else
            {
                uncachedIds.Add(reviewerId);
            }
        }

        // If there are uncached reviewers, batch query
        if (uncachedIds.Any())
        {
            // Batch query pending todo items count
            var todos = await _todoRepository.FindAsync(t =>
                uncachedIds.Contains(t.CurrentReviewerUserId.Value) &&
                t.Status.StartsWith("pending"));

            var workloadDict = todos
                .GroupBy(t => t.CurrentReviewerUserId.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Update results and cache
            foreach (var reviewerId in uncachedIds)
            {
                if (workloadDict.TryGetValue(reviewerId, out var workload))
                {
                    result[reviewerId] = workload;
                    _workloadCache[reviewerId] = workload;
                }
                else
                {
                    result[reviewerId] = 0;
                    _workloadCache[reviewerId] = 0;
                }
            }
        }

        return result;
    }

    private async Task<int?> DetermineNextReviewerAsync(
        ReviewStages nextStage,
        int? requestedReviewerId,
        int todoListId)
    {
        // 1. Priority: use requested reviewer
        if (requestedReviewerId.HasValue)
        {
            // Batch verify if specified reviewer has required role
            var checkResults = await CheckUsersHaveRoleAsync(
                new List<int> { requestedReviewerId.Value },
                nextStage.RequiredRoleId);

            if (checkResults.TryGetValue(requestedReviewerId.Value, out var hasRole) && hasRole)
            {
                return requestedReviewerId.Value;
            }
            // If doesn't have required role, ignore and continue to next rule
        }

        // 2. Use stage-specific reviewer
        if (nextStage.SpecificReviewerUserId.HasValue)
        {
            return nextStage.SpecificReviewerUserId.Value;
        }

        // 3. Dynamic allocation based on role
        return await FindReviewerByRoleAsync(nextStage.RequiredRoleId, todoListId, -1);
    }
}