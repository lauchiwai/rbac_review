using Common.DTOs;
using Common.DTOs.Review.Requests;
using Common.DTOs.Review.Response;
using Common.DTOs.Stage.Response;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IRepository<TodoLists> _todoRepository;
    private readonly IRepository<Users> _userRepository;
    private readonly IRepository<ReviewStages> _stageRepository;
    private readonly IRepository<StageTransitions> _transitionRepository;
    private readonly IRepository<Reviews> _reviewRepository;
    private readonly ReviewQueryHelper _queryHelper;

    public ReviewService(
        IRepository<TodoLists> todoRepository,
        IRepository<Users> userRepository,
        IRepository<Roles> roleRepository,
        IRepository<Permissions> permissionRepository,
        IRepository<ReviewStages> stageRepository,
        IRepository<StageTransitions> transitionRepository,
        IRepository<Reviews> reviewRepository,
        IRepository<Users_Roles> userRoleRepository,
        IRepository<Roles_Permissions> rolePermissionRepository)
    {
        _todoRepository = todoRepository;
        _userRepository = userRepository;
        _stageRepository = stageRepository;
        _transitionRepository = transitionRepository;
        _reviewRepository = reviewRepository;

        _queryHelper = new ReviewQueryHelper(
            userRoleRepository,
            rolePermissionRepository,
            stageRepository,
            userRepository,
            roleRepository,
            transitionRepository,
            permissionRepository);
    }

    public async Task<ResultDto<List<PendingReviewResponse>>> GetPendingReviewsAsync(int userId)
    {
        try
        {
            // 1. Get user information
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ResultDto<List<PendingReviewResponse>>.Failure($"User ID {userId} does not exist");
            }

            // 2. Query all pending todo items
            var pendingTodos = await _todoRepository.FindWithIncludesAsync(
                t =>
                    // Condition 1: Current reviewer is self
                    (t.CurrentReviewerUserId == userId) ||
                    // Condition 2: Self is creator and status is returned to creator
                    (t.CreatedByUserId == userId && t.Status == "returned_to_creator") ||
                    // Condition 3: Self is reviewer for returned to level 1
                    (t.CurrentReviewerUserId == userId && t.CurrentStageId == 4),
                t => t.CreatedByUser,
                t => t.CurrentReviewerUser,
                t => t.CurrentStage,
                t => t.ReviewTemplate);

            if (!pendingTodos.Any())
            {
                return ResultDto<List<PendingReviewResponse>>.Success(new List<PendingReviewResponse>(), "No pending reviews");
            }

            // 3. Batch processing: collect all required IDs
            var stageIds = pendingTodos
                .Where(t => t.CurrentStageId.HasValue)
                .Select(t => t.CurrentStageId.Value)
                .Distinct()
                .ToList();

            // 4. Batch query related data
            var userRoles = await _queryHelper.GetUserRolesAsync(userId);
            var stagesDict = await _queryHelper.GetStagesByIdsAsync(stageIds);
            var transitionsDict = await _queryHelper.GetTransitionsByStageIdsAsync(stageIds);

            var result = new List<PendingReviewResponse>();

            foreach (var todo in pendingTodos)
            {
                // Handle todo items returned to level 1 review
                if (todo.CurrentStageId == 4) // StageId 4: Returned to level 1 review
                {
                    // Check if it's the current reviewer
                    if (todo.CurrentReviewerUserId != userId)
                    {
                        continue; // Not the reviewer, skip
                    }

                    // Provide action to resubmit to level 2 review
                    var secondStage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 2);
                    if (secondStage != null)
                    {
                        result.Add(new PendingReviewResponse
                        {
                            TodoListId = todo.TodoListId,
                            Title = todo.Title,
                            Status = todo.Status,
                            CreatedByUserName = $"User{todo.CreatedByUserId}",
                            CreatedAt = todo.CreatedAt,
                            CurrentStageName = "Returned to level 1 review",
                            CurrentReviewerUserName = $"User{userId}",
                            AvailableActions = new List<AvailableActionResponse>
                    {
                        new AvailableActionResponse
                        {
                            ActionName = "resubmit",
                            DisplayName = "Resubmit",
                            ResultStatus = "pending_review_level2",
                            NextStageName = secondStage.StageName
                        }
                    }
                        });
                    }
                    continue;
                }

                // Handle todo items returned to creator
                if (todo.Status == "returned_to_creator")
                {
                    // Check if it's the creator
                    if (todo.CreatedByUserId != userId)
                    {
                        continue; // Not the creator, skip
                    }

                    // Provide action to resubmit to level 1 review
                    var firstStage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 1);
                    if (firstStage != null)
                    {
                        result.Add(new PendingReviewResponse
                        {
                            TodoListId = todo.TodoListId,
                            Title = todo.Title,
                            Status = todo.Status,
                            CreatedByUserName = $"User{todo.CreatedByUserId}",
                            CreatedAt = todo.CreatedAt,
                            CurrentStageName = "Returned to creator",
                            CurrentReviewerUserName = $"User{userId}",
                            AvailableActions = new List<AvailableActionResponse>
                    {
                        new AvailableActionResponse
                        {
                            ActionName = "resubmit",
                            DisplayName = "Resubmit",
                            ResultStatus = "pending_review_level1",
                            NextStageName = firstStage.StageName
                        }
                    }
                        });
                    }
                    continue;
                }

                // Normal pending review item processing
                if (todo.CurrentStageId == null) continue;

                // Check if user has review permission for this stage
                if (stagesDict.TryGetValue(todo.CurrentStageId.Value, out var stage))
                {
                    // Check if user has required role
                    if (!userRoles.Contains(stage.RequiredRoleId))
                    {
                        continue; // No permission, skip
                    }

                    // If specific reviewer, check if it's the current reviewer
                    if (stage.SpecificReviewerUserId.HasValue &&
                        stage.SpecificReviewerUserId.Value != userId)
                    {
                        continue; // Not the specific reviewer, skip
                    }
                }

                // Get available actions
                var availableActions = new List<AvailableActionResponse>();
                if (transitionsDict.TryGetValue(todo.CurrentStageId.Value, out var transitions))
                {
                    foreach (var transition in transitions)
                    {
                        string? nextStageName = null;
                        if (transition.NextStageId.HasValue)
                        {
                            stagesDict.TryGetValue(transition.NextStageId.Value, out var nextStage);
                            nextStageName = nextStage?.StageName;
                        }

                        availableActions.Add(new AvailableActionResponse
                        {
                            ActionName = transition.ActionName,
                            DisplayName = GetActionDisplayName(transition.ActionName),
                            ResultStatus = transition.ResultStatus,
                            NextStageName = nextStageName
                        });
                    }
                }

                // Convert to response DTO
                result.Add(new PendingReviewResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserName = $"User{todo.CreatedByUserId}",
                    CreatedAt = todo.CreatedAt,
                    CurrentStageName = stage?.StageName ?? "Unknown",
                    CurrentReviewerUserName = $"User{userId}",
                    AvailableActions = availableActions
                });
            }

            return ResultDto<List<PendingReviewResponse>>.Success(result, "Pending reviews retrieved successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<List<PendingReviewResponse>>.Failure($"Error retrieving pending reviews: {ex.Message}");
        }
    }

    public async Task<ResultDto<TodoDetailResponse>> GetTodoDetailAsync(int userId, int todoId)
    {
        try
        {
            // 1. Get todo item
            var todos = await _todoRepository.FindWithIncludesAsync(
                t => t.TodoListId == todoId,
                t => t.CreatedByUser,
                t => t.CurrentReviewerUser,
                t => t.CurrentStage,
                t => t.ReviewTemplate);

            var todo = todos.FirstOrDefault();
            if (todo == null)
            {
                return ResultDto<TodoDetailResponse>.Failure($"Todo ID {todoId} does not exist");
            }

            // 2. Check permissions
            var canView = todo.CreatedByUserId == userId ||
                         todo.CurrentReviewerUserId == userId ||
                         await _queryHelper.UserHasPermissionAsync(userId, "admin_manage");

            if (!canView)
            {
                return ResultDto<TodoDetailResponse>.Failure("You do not have permission to view this todo");
            }

            // 3. Batch query all required data
            // Get review history
            var reviews = await _reviewRepository.FindWithIncludesAsync(
                r => r.TodoId == todoId,
                r => r.ReviewerUser,
                r => r.ReviewStage);

            // 4. Get all stages of the template
            var allStages = new List<ReviewStages>();
            if (todo.TemplateId.HasValue)
            {
                allStages = (await _stageRepository.FindAsync(s => s.TemplateId == todo.TemplateId.Value)).ToList();
            }

            // Collect role and user IDs
            var roleIds = allStages.Select(s => s.RequiredRoleId).Distinct().ToList();
            var stageUserIds = allStages.Select(s => s.SpecificReviewerUserId).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

            // 5. Batch query role information
            var rolesDict = await _queryHelper.GetRolesByIdsAsync(roleIds);
            var usersDict = await _queryHelper.GetUsersByIdsAsync(stageUserIds);

            var stageResponses = new List<StageInfoResponse>();
            foreach (var stage in allStages)
            {
                rolesDict.TryGetValue(stage.RequiredRoleId, out var role);

                string specificReviewerName = "No specific";
                if (stage.SpecificReviewerUserId.HasValue)
                {
                    usersDict.TryGetValue(stage.SpecificReviewerUserId.Value, out var specificUser);
                    specificReviewerName = specificUser != null ? $"User{specificUser.UserId}" : "Unknown";
                }

                stageResponses.Add(new StageInfoResponse
                {
                    StageId = stage.StageId,
                    StageName = stage.StageName,
                    StageOrder = stage.StageOrder,
                    RequiredRoleName = role?.RoleName ?? "Unknown",
                    SpecificReviewerUserName = specificReviewerName,
                    IsCurrentStage = stage.StageId == todo.CurrentStageId
                });
            }

            // 6. Get available actions
            var availableActions = await GetAvailableActionsForTodo(todo, userId);

            // 7. Convert to response DTO
            var response = new TodoDetailResponse
            {
                TodoListId = todo.TodoListId,
                Title = todo.Title,
                Status = todo.Status,
                CreatedByUserName = $"User{todo.CreatedByUserId}",
                CreatedAt = todo.CreatedAt,
                CurrentStageName = todo.CurrentStage?.StageName ?? "None",
                CurrentReviewerUserName = todo.CurrentReviewerUserId.HasValue
                    ? $"User{todo.CurrentReviewerUserId.Value}"
                    : "None",
                ReviewHistory = reviews.Select(r => new TodoReviewHistoryResponse
                {
                    ReviewedAt = r.ReviewedAt,
                    ReviewerUserName = $"User{r.ReviewerUserId}",
                    Action = r.Action,
                    Comment = r.Comment,
                    PreviousStatus = r.PreviousStatus,
                    NewStatus = r.NewStatus,
                    StageName = r.ReviewStage?.StageName ?? "Unknown"
                }).OrderByDescending(r => r.ReviewedAt).ToList(),
                AvailableActions = availableActions,
                TemplateName = todo.ReviewTemplate?.TemplateName ?? "Unknown",
                AllStages = stageResponses.OrderBy(s => s.StageOrder).ToList()
            };

            return ResultDto<TodoDetailResponse>.Success(response, "Todo detail retrieved successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<TodoDetailResponse>.Failure($"Error retrieving todo detail: {ex.Message}");
        }
    }

    private async Task<List<AvailableActionResponse>> GetAvailableActionsForTodo(TodoLists todo, int userId)
    {
        var actions = new List<AvailableActionResponse>();

        // If it's return status
        if (todo.Status == "returned_to_creator" || todo.Status == "returned_to_level1")
        {
            // Check user permissions
            if (todo.CreatedByUserId == userId)
            {
                // Get corresponding transition rules based on return status
                var transitionKey = todo.Status switch
                {
                    "returned_to_creator" => 3, // StageId 3
                    "returned_to_level1" => 4,  // StageId 4
                    _ => 0
                };

                if (transitionKey > 0)
                {
                    var transitions = await _transitionRepository.FindAsync(t =>
                        t.StageId == transitionKey && t.ActionName == "resubmit");

                    var transition = transitions.FirstOrDefault();
                    if (transition != null && transition.NextStageId.HasValue)
                    {
                        var nextStage = await _stageRepository.GetByIdAsync(transition.NextStageId.Value);

                        actions.Add(new AvailableActionResponse
                        {
                            ActionName = "resubmit",
                            DisplayName = "Resubmit",
                            ResultStatus = transition.ResultStatus,
                            NextStageName = nextStage?.StageName
                        });
                    }
                }
            }
            return actions;
        }

        // Normal pending review item available actions
        if (todo.CurrentStageId == null || todo.CurrentReviewerUserId != userId)
            return actions;

        // Check if user has review permission for this stage
        var stage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
        if (stage == null) return actions;

        var userRoles = await _queryHelper.GetUserRolesAsync(userId);
        if (!userRoles.Contains(stage.RequiredRoleId))
            return actions;

        // If specific reviewer, check if it's the current reviewer
        if (stage.SpecificReviewerUserId.HasValue && stage.SpecificReviewerUserId.Value != userId)
            return actions;

        // Get transition rules
        var stageTransitions = await _transitionRepository.FindAsync(
            t => t.StageId == todo.CurrentStageId);

        if (!stageTransitions.Any())
            return actions;

        // Collect next stage IDs
        var nextStageIds = stageTransitions.Where(t => t.NextStageId.HasValue)
                                         .Select(t => t.NextStageId.Value)
                                         .Distinct()
                                         .ToList();

        var nextStagesDict = await _queryHelper.GetStagesByIdsAsync(nextStageIds);

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
                DisplayName = GetActionDisplayName(transition.ActionName),
                ResultStatus = transition.ResultStatus,
                NextStageName = nextStageName
            });
        }

        return actions;
    }

    private string GetActionDisplayName(string action)
    {
        return action switch
        {
            "approve" => "Approve",
            "return" => "Return",
            "reject" => "Reject",
            "resubmit" => "Resubmit",
            _ => action
        };
    }

    public async Task<ResultDto<ReviewActionResponse>> ExecuteReviewActionAsync(ReviewActionRequest request)
    {
        try
        {
            // 1. If it's resubmit, special handling
            if (request.Action == "resubmit")
            {
                return await ExecuteResubmitActionAsync(request);
            }

            // 2. Get todo item
            var todos = await _todoRepository.FindWithIncludesAsync(
                t => t.TodoListId == request.TodoId,
                t => t.CurrentStage,
                t => t.ReviewTemplate);

            var todo = todos.FirstOrDefault();
            if (todo == null)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Todo ID {request.TodoId} does not exist");
            }

            // 3. Check if user is the current reviewer
            if (todo.CurrentReviewerUserId != request.UserId)
            {
                return ResultDto<ReviewActionResponse>.Failure("You are not the current reviewer");
            }

            // 4. Check if todo is in a reviewable state
            if (todo.CurrentStageId == null)
            {
                return ResultDto<ReviewActionResponse>.Failure("This todo is not in a reviewable state");
            }

            // 5. Check if user has review permission for this stage
            var userRoles = await _queryHelper.GetUserRolesAsync(request.UserId);
            if (todo.CurrentStage != null && !userRoles.Contains(todo.CurrentStage.RequiredRoleId))
            {
                return ResultDto<ReviewActionResponse>.Failure("You do not have the required role to review this stage");
            }

            // 6. Query transition rules
            var transitions = await _transitionRepository.FindAsync(t =>
                t.StageId == todo.CurrentStageId && t.ActionName == request.Action);

            var transition = transitions.FirstOrDefault();
            if (transition == null)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Action '{request.Action}' is not valid for this stage");
            }

            // 7. Create review record
            var review = new Reviews
            {
                TodoId = request.TodoId,
                ReviewerUserId = request.UserId,
                Action = request.Action,
                Comment = request.Comment,
                ReviewedAt = DateTime.UtcNow,
                PreviousStatus = todo.Status,
                NewStatus = transition.ResultStatus,
                StageId = todo.CurrentStageId
            };

            await _reviewRepository.AddAsync(review);

            // 8. Update todo status
            var previousStatus = todo.Status;
            todo.Status = transition.ResultStatus;

            // 9. Handle return action - Find previous level reviewer from review history
            if (request.Action == "return")
            {
                await HandleReturnAction(todo, transition, request.UserId);
            }
            else if (request.Action == "approve")
            {
                await HandleApproveAction(todo, transition);
            }
            else if (request.Action == "reject")
            {
                await HandleRejectAction(todo);
            }

            // 10. Save changes
            await _todoRepository.UpdateAsync(todo);

            // 11. Get next stage information
            string? nextStageName = null;
            string? currentReviewerUserName = null;
            bool isCompleted = false;

            if (todo.CurrentStageId.HasValue)
            {
                var stage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
                nextStageName = stage?.StageName;
                currentReviewerUserName = todo.CurrentReviewerUserId.HasValue
                    ? $"User{todo.CurrentReviewerUserId.Value}"
                    : "None";

                // Check if completed
                if (todo.CurrentStageId == 3 || todo.CurrentStageId == 4 ||
                    todo.Status == "approved" || todo.Status == "rejected")
                {
                    isCompleted = true;
                }
            }

            // 12. Return review result
            var response = new ReviewActionResponse
            {
                TodoListId = todo.TodoListId,
                Title = todo.Title,
                Status = todo.Status,
                PreviousStatus = previousStatus,
                Action = request.Action,
                ReviewId = review.ReviewId,
                ReviewedAt = review.ReviewedAt,
                Comment = review.Comment,
                CurrentStageName = todo.CurrentStageId.HasValue
                    ? (await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value))?.StageName ?? "Unknown"
                    : "Completed",
                CurrentReviewerUserName = currentReviewerUserName ?? "None",
                NextStageName = nextStageName ?? "None",
                IsCompleted = isCompleted
            };

            return ResultDto<ReviewActionResponse>.Success(response, "Review action executed successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<ReviewActionResponse>.Failure($"Error executing review action: {ex.Message}");
        }
    }

    private async Task HandleReturnAction(TodoLists todo, StageTransitions transition, int currentUserId)
    {
        // Process based on return target
        switch (transition.ResultStatus)
        {
            case "returned_to_creator":
                // Return to creator, clear current stage
                todo.CurrentStageId = 3; // StageId 3: Returned to creator
                todo.CurrentReviewerUserId = null;
                break;

            case "returned_to_level1":
                // Return to level 1 review, find previous level reviewer from review history
                await HandleReturnToLevel1(todo, currentUserId);
                break;

            default:
                // Other return statuses
                if (transition.NextStageId.HasValue)
                {
                    var returnStage = await _stageRepository.GetByIdAsync(transition.NextStageId.Value);
                    if (returnStage != null)
                    {
                        todo.CurrentStageId = returnStage.StageId;
                        todo.CurrentReviewerUserId = returnStage.SpecificReviewerUserId;
                    }
                }
                break;
        }
    }

    private async Task HandleReturnToLevel1(TodoLists todo, int currentUserId)
    {
        // Set as returned to level 1 review stage
        todo.CurrentStageId = 4; // StageId 4: Returned to level 1 review

        // Find review history, locate most recent level 1 review record
        var reviews = await _reviewRepository.FindAsync(r =>
            r.TodoId == todo.TodoListId &&
            r.StageId == 1 && // Level 1 review stage
            r.Action == "approve"); // Approve action

        // Sort by time, get most recent level 1 review record
        var latestLevel1Review = reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();

        if (latestLevel1Review != null)
        {
            // Set reviewer after return to the most recent level 1 reviewer
            todo.CurrentReviewerUserId = latestLevel1Review.ReviewerUserId;
        }
        else
        {
            // If no level 1 review record found, use default reviewer for level 1 review stage
            var level1Stage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 1);
            if (level1Stage != null)
            {
                todo.CurrentReviewerUserId = level1Stage.SpecificReviewerUserId;
            }
            else
            {
                // If still not found, set to current reviewer (return action initiator)
                todo.CurrentReviewerUserId = currentUserId;
            }
        }
    }

    private async Task HandleApproveAction(TodoLists todo, StageTransitions transition)
    {
        if (transition.NextStageId.HasValue)
        {
            // Move to next stage
            var nextStage = await _stageRepository.GetByIdAsync(transition.NextStageId.Value);
            if (nextStage != null)
            {
                todo.CurrentStageId = nextStage.StageId;
                todo.CurrentReviewerUserId = nextStage.SpecificReviewerUserId;
            }
            else
            {
                // No next stage, review completed
                todo.CurrentStageId = null;
                todo.CurrentReviewerUserId = null;
                todo.Status = "completed";
            }
        }
        else
        {
            // Final approval, review completed
            todo.CurrentStageId = null;
            todo.CurrentReviewerUserId = null;
            todo.Status = "approved";
        }
    }

    private async Task HandleRejectAction(TodoLists todo)
    {
        // Reject action, review terminated
        todo.CurrentStageId = null;
        todo.CurrentReviewerUserId = null;
        todo.Status = "rejected";
    }

    private async Task<ReviewStages?> GetStageByTemplateAndOrderAsync(int templateId, int stageOrder)
    {
        var stages = await _stageRepository.FindAsync(s =>
            s.TemplateId == templateId && s.StageOrder == stageOrder);
        return stages.FirstOrDefault();
    }

    private async Task<ResultDto<ReviewActionResponse>> ExecuteResubmitActionAsync(ReviewActionRequest request)
    {
        try
        {
            // 1. Get todo item
            var todos = await _todoRepository.FindWithIncludesAsync(
                t => t.TodoListId == request.TodoId,
                t => t.ReviewTemplate,
                t => t.CurrentStage);

            var todo = todos.FirstOrDefault();
            if (todo == null)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Todo ID {request.TodoId} does not exist");
            }

            // 2. Check if user has permission to resubmit
            bool canResubmit = false;
            ReviewStages? targetStage = null;

            if (todo.Status == "returned_to_creator")
            {
                // Returned to creator: only creator can resubmit
                if (todo.CreatedByUserId != request.UserId)
                {
                    return ResultDto<ReviewActionResponse>.Failure("Only the creator can resubmit");
                }
                // Resubmit to first stage
                targetStage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 1);
                canResubmit = true;
            }
            else if (todo.CurrentStageId == 4) // StageId 4: Returned to level 1 review
            {
                // Returned to level 1 review: only current reviewer can resubmit
                if (todo.CurrentReviewerUserId != request.UserId)
                {
                    return ResultDto<ReviewActionResponse>.Failure("Only the current reviewer can resubmit");
                }
                // Resubmit to second stage (according to your data, TransitionId=8 has NextStageId=1, but we need to go to level 2 review)
                targetStage = await GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 2);
                canResubmit = true;
            }

            if (!canResubmit || targetStage == null)
            {
                return ResultDto<ReviewActionResponse>.Failure("Unable to resubmit this todo item");
            }

            // 3. Query resubmit transition rules
            var resubmitTransitions = await _transitionRepository.FindAsync(t =>
                t.StageId == todo.CurrentStageId && t.ActionName == "resubmit");

            var resubmitTransition = resubmitTransitions.FirstOrDefault();
            if (resubmitTransition == null)
            {
                return ResultDto<ReviewActionResponse>.Failure("The system has not configured resubmit rules");
            }

            // 4. Create resubmit review record
            var review = new Reviews
            {
                TodoId = request.TodoId,
                ReviewerUserId = request.UserId,
                Action = "resubmit",
                Comment = request.Comment,
                ReviewedAt = DateTime.UtcNow,
                PreviousStatus = todo.Status,
                NewStatus = resubmitTransition.ResultStatus,
                StageId = targetStage.StageId
            };

            await _reviewRepository.AddAsync(review);

            // 5. Update todo status
            var previousStatus = todo.Status;
            todo.Status = resubmitTransition.ResultStatus;
            todo.CurrentStageId = targetStage.StageId;
            todo.CurrentReviewerUserId = targetStage.SpecificReviewerUserId;

            await _todoRepository.UpdateAsync(todo);

            // 6. Return result
            var response = new ReviewActionResponse
            {
                TodoListId = todo.TodoListId,
                Title = todo.Title,
                Status = todo.Status,
                PreviousStatus = previousStatus,
                Action = "resubmit",
                ReviewId = review.ReviewId,
                ReviewedAt = review.ReviewedAt,
                Comment = review.Comment,
                CurrentStageName = targetStage.StageName,
                CurrentReviewerUserName = targetStage.SpecificReviewerUserId.HasValue
                    ? $"User{targetStage.SpecificReviewerUserId.Value}"
                    : "None",
                NextStageName = targetStage.StageName,
                IsCompleted = false
            };

            return ResultDto<ReviewActionResponse>.Success(response, "Resubmit successful");
        }
        catch (Exception ex)
        {
            return ResultDto<ReviewActionResponse>.Failure($"An error occurred during resubmit: {ex.Message}");
        }
    }

    public async Task<ResultDto<ReviewHistoryFullResponse>> GetReviewHistoryAsync(int userId, int todoId)
    {
        try
        {
            // 1. Get todo item
            var todos = await _todoRepository.FindWithIncludesAsync(
                t => t.TodoListId == todoId,
                t => t.CreatedByUser,
                t => t.ReviewTemplate);

            var todo = todos.FirstOrDefault();
            if (todo == null)
            {
                return ResultDto<ReviewHistoryFullResponse>.Failure($"Todo ID {todoId} does not exist");
            }

            // 2. Check permissions: creator or admin can view
            var canView = todo.CreatedByUserId == userId ||
                         await _queryHelper.UserHasPermissionAsync(userId, "admin_manage");

            if (!canView)
            {
                return ResultDto<ReviewHistoryFullResponse>.Failure("You do not have permission to view review history");
            }

            // 3. Batch get all required data
            // Get review history
            var reviews = await _reviewRepository.FindWithIncludesAsync(
                r => r.TodoId == todoId,
                r => r.ReviewerUser,
                r => r.ReviewStage);

            // 4. Create timeline item list
            var timelineItems = new List<ReviewTimelineItemResponse>();

            // Add creation record
            timelineItems.Add(new ReviewTimelineItemResponse
            {
                Time = todo.CreatedAt,
                Stage = "Creation",
                ReviewerUserName = $"User{todo.CreatedByUserId}",
                Action = "created",
                ResultStatus = todo.Status,
                ActionDisplayName = "Created",
                StatusDisplayName = GetStatusDisplayName(todo.Status),
                Comment = "Todo item created"
            });

            // Add review records
            foreach (var review in reviews.OrderBy(r => r.ReviewedAt))
            {
                var stageName = review.ReviewStage?.StageName ?? "Unknown stage";

                timelineItems.Add(new ReviewTimelineItemResponse
                {
                    Time = review.ReviewedAt,
                    Stage = stageName,
                    ReviewerUserName = $"User{review.ReviewerUserId}",
                    Action = review.Action,
                    ResultStatus = review.NewStatus ?? "",
                    Comment = review.Comment,
                    ActionDisplayName = GetActionDisplayName(review.Action),
                    StatusDisplayName = GetStatusDisplayName(review.NewStatus)
                });
            }

            // 5. Calculate review summary
            var summary = CalculateReviewSummary(todo, timelineItems);

            // 6. Build response
            var response = new ReviewHistoryFullResponse
            {
                TodoListId = todo.TodoListId,
                Title = todo.Title,
                Status = todo.Status,
                CreatedByUserName = $"User{todo.CreatedByUserId}",
                CreatedAt = todo.CreatedAt,
                TemplateName = todo.ReviewTemplate?.TemplateName ?? "Unknown template",
                Timeline = timelineItems.OrderBy(t => t.Time).ToList(),
                Summary = summary
            };

            return ResultDto<ReviewHistoryFullResponse>.Success(response, "Review history retrieved successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<ReviewHistoryFullResponse>.Failure($"Error retrieving review history: {ex.Message}");
        }
    }

    private ReviewSummaryResponse CalculateReviewSummary(TodoLists todo, List<ReviewTimelineItemResponse> timeline)
    {
        var summary = new ReviewSummaryResponse
        {
            CurrentStatus = GetStatusDisplayName(todo.Status)
        };

        if (timeline.Any())
        {
            var reviewItems = timeline.Where(t => t.Action != "created").ToList();

            if (reviewItems.Any())
            {
                var firstReview = reviewItems.Min(t => t.Time);
                var lastReview = reviewItems.Max(t => t.Time);
                var duration = lastReview - firstReview;

                summary.TotalReviews = reviewItems.Count;
                summary.FirstReviewDate = firstReview.ToString("yyyy-MM-dd HH:mm:ss");
                summary.LastReviewDate = lastReview.ToString("yyyy-MM-dd HH:mm:ss");
                summary.TotalDuration = $"{duration.Days} days {duration.Hours} hours {duration.Minutes} minutes";
                summary.ApprovalCount = reviewItems.Count(r => r.Action == "approve");
                summary.ReturnCount = reviewItems.Count(r => r.Action == "return");
                summary.RejectCount = reviewItems.Count(r => r.Action == "reject");
            }
            else
            {
                // If only creation record, no review records
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

    private string GetStatusDisplayName(string? status)
    {
        if (string.IsNullOrEmpty(status))
            return "unknown";

        return status;
    }
}