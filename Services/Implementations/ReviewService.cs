using Common.DTOs;
using Common.DTOs.Review.Requests;
using Common.DTOs.Review.Response;
using Common.DTOs.Stage.Response;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Users> _userRepository;
        private readonly IRepository<ReviewStages> _stageRepository;
        private readonly IRepository<StageTransitions> _transitionRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IReviewQueryHelper _queryHelper;
        private readonly IUserNameHelper _userNameHelper;
        private readonly IReviewHelper _reviewHelper;

        public ReviewService(
            IRepository<TodoLists> todoRepository,
            IRepository<Users> userRepository,
            IRepository<ReviewStages> stageRepository,
            IRepository<StageTransitions> transitionRepository,
            IRepository<Reviews> reviewRepository,
            IReviewQueryHelper reviewQueryHelper,
            IUserNameHelper userNameHelper,
            IReviewHelper reviewHelper)
        {
            _todoRepository = todoRepository;
            _userRepository = userRepository;
            _stageRepository = stageRepository;
            _transitionRepository = transitionRepository;
            _reviewRepository = reviewRepository;
            _queryHelper = reviewQueryHelper;
            _userNameHelper = userNameHelper;
            _reviewHelper = reviewHelper;
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
                        // Condition 1: Current reviewer is the user
                        (t.CurrentReviewerUserId == userId) ||
                        // Condition 2: User is the creator and status is returned_to_creator
                        (t.CreatedByUserId == userId && t.Status == ReviewConstantsHelper.ReturnedToCreator),
                    t => t.CreatedByUser,
                    t => t.CurrentReviewerUser,
                    t => t.CurrentStage,
                    t => t.ReviewTemplate);

                if (!pendingTodos.Any())
                {
                    return ResultDto<List<PendingReviewResponse>>.Success(new List<PendingReviewResponse>(), "No pending reviews");
                }

                // 3. Batch processing: Collect all required IDs
                var stageIds = pendingTodos
                    .Where(t => t.CurrentStageId.HasValue)
                    .Select(t => t.CurrentStageId.Value)
                    .Distinct()
                    .ToList();

                // 4. Batch query related data
                var userRoles = await _queryHelper.GetUserRolesAsync(userId);
                var stagesDict = await _queryHelper.GetStagesByIdsAsync(stageIds);
                var transitionsDict = await _queryHelper.GetTransitionsByStageIdsAsync(stageIds);

                // 5. Batch query required first stage information
                var returnedToCreatorTodos = pendingTodos
                    .Where(t => t.Status == ReviewConstantsHelper.ReturnedToCreator && t.TemplateId.HasValue)
                    .ToList();

                var templateIdsForFirstStage = returnedToCreatorTodos
                    .Select(t => t.TemplateId.Value)
                    .Distinct()
                    .ToList();

                var firstStagesDict = await _reviewHelper.GetFirstStagesByTemplateIdsAsync(templateIdsForFirstStage);

                // 6. Collect all required user IDs
                var allUserIds = new List<int> { userId };
                allUserIds.AddRange(pendingTodos.Select(t => t.CreatedByUserId));
                allUserIds.AddRange(pendingTodos
                    .Where(t => t.CurrentReviewerUserId.HasValue)
                    .Select(t => t.CurrentReviewerUserId.Value));

                // Batch get user names
                var userNamesDict = await _userNameHelper.GetUserNamesAsync(allUserIds.Distinct().ToList());

                var result = new List<PendingReviewResponse>();

                foreach (var todo in pendingTodos)
                {
                    // Process todo items returned to creator
                    if (todo.Status == ReviewConstantsHelper.ReturnedToCreator)
                    {
                        // Check if user is the creator
                        if (todo.CreatedByUserId != userId)
                        {
                            continue;
                        }

                        // Get first stage information from batch query results
                        ReviewStages? firstStage = null;
                        if (todo.TemplateId.HasValue)
                        {
                            firstStagesDict.TryGetValue(todo.TemplateId.Value, out firstStage);
                        }

                        if (firstStage != null)
                        {
                            result.Add(new PendingReviewResponse
                            {
                                TodoListId = todo.TodoListId,
                                Title = todo.Title,
                                Status = todo.Status,
                                CreatedByUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                                CreatedAt = todo.CreatedAt,
                                CurrentStageName = "Returned to creator",
                                CurrentReviewerUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                                AvailableActions = new List<AvailableActionResponse>
                                {
                                    new AvailableActionResponse
                                    {
                                        ActionName = ReviewConstantsHelper.ActionResubmit,
                                        DisplayName = ReviewConstantsHelper.GetActionDisplayName(ReviewConstantsHelper.ActionResubmit),
                                        ResultStatus = ReviewConstantsHelper.PendingReviewLevel1,
                                        NextStageName = firstStage.StageName
                                    }
                                }
                            });
                        }
                        continue;
                    }

                    // Process todo items returned to reviewer
                    if (todo.Status == ReviewConstantsHelper.ReturnedToReviewer)
                    {
                        // Check if user is the current reviewer
                        if (todo.CurrentReviewerUserId != userId)
                        {
                            continue;
                        }

                        // Get stage information from batch query stagesDict
                        ReviewStages? currentStage = null;
                        if (todo.CurrentStageId.HasValue)
                        {
                            stagesDict.TryGetValue(todo.CurrentStageId.Value, out currentStage);
                        }

                        if (currentStage != null)
                        {
                            result.Add(new PendingReviewResponse
                            {
                                TodoListId = todo.TodoListId,
                                Title = todo.Title,
                                Status = todo.Status,
                                CreatedByUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                                CreatedAt = todo.CreatedAt,
                                CurrentStageName = currentStage.StageName,
                                CurrentReviewerUserName = GetUserNameFromDict(userNamesDict, userId),
                                AvailableActions = new List<AvailableActionResponse>
                                {
                                    new AvailableActionResponse
                                    {
                                        ActionName = ReviewConstantsHelper.ActionResubmit,
                                        DisplayName = ReviewConstantsHelper.GetActionDisplayName(ReviewConstantsHelper.ActionResubmit),
                                        ResultStatus = $"pending_review_stage{todo.CurrentStageId}",
                                        NextStageName = currentStage.StageName
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
                            continue;
                        }

                        // If there is a specific reviewer, check if user is the current reviewer
                        if (stage.SpecificReviewerUserId.HasValue &&
                            stage.SpecificReviewerUserId.Value != userId)
                        {
                            continue;
                        }
                    }

                    // Get available actions
                    var availableActions = new List<AvailableActionResponse>();
                    if (transitionsDict.TryGetValue(todo.CurrentStageId.Value, out var transitions))
                    {
                        // Batch query next stage information
                        var nextStageIds = transitions.Where(t => t.NextStageId.HasValue)
                                                    .Select(t => t.NextStageId.Value)
                                                    .Distinct()
                                                    .ToList();

                        var nextStagesDict = await _reviewHelper.GetNextStagesByIdsAsync(nextStageIds);

                        foreach (var transition in transitions)
                        {
                            string? nextStageName = null;
                            if (transition.NextStageId.HasValue)
                            {
                                nextStagesDict.TryGetValue(transition.NextStageId.Value, out var nextStage);
                                nextStageName = nextStage?.StageName;
                            }

                            availableActions.Add(new AvailableActionResponse
                            {
                                ActionName = transition.ActionName,
                                DisplayName = ReviewConstantsHelper.GetActionDisplayName(transition.ActionName),
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
                        CreatedByUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                        CreatedAt = todo.CreatedAt,
                        CurrentStageName = stage?.StageName ?? "Unknown",
                        CurrentReviewerUserName = GetUserNameFromDict(userNamesDict, userId),
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
                             await _queryHelper.UserHasPermissionAsync(userId, ReviewConstantsHelper.PermissionAdminManage);

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

                // 6. Collect all required user IDs
                var allUserIds = new List<int> { todo.CreatedByUserId };
                if (todo.CurrentReviewerUserId.HasValue)
                    allUserIds.Add(todo.CurrentReviewerUserId.Value);
                allUserIds.AddRange(reviews.Select(r => r.ReviewerUserId));
                allUserIds.AddRange(stageUserIds);

                var userNamesDict = await _userNameHelper.GetUserNamesAsync(allUserIds.Distinct().ToList());

                var stageResponses = new List<StageInfoResponse>();
                foreach (var stage in allStages)
                {
                    rolesDict.TryGetValue(stage.RequiredRoleId, out var role);

                    string specificReviewerName = "No specific";
                    if (stage.SpecificReviewerUserId.HasValue)
                    {
                        specificReviewerName = GetUserNameFromDict(userNamesDict, stage.SpecificReviewerUserId.Value);
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

                // 7. Batch query stage and transition data for getting available actions
                Dictionary<int, ReviewStages>? stagesDict = null;
                Dictionary<int, List<StageTransitions>>? transitionsDict = null;

                if (todo.CurrentStageId.HasValue)
                {
                    stagesDict = await _queryHelper.GetStagesByIdsAsync(new List<int> { todo.CurrentStageId.Value });
                    transitionsDict = await _queryHelper.GetTransitionsByStageIdsAsync(new List<int> { todo.CurrentStageId.Value });
                }

                // 8. Get available actions
                var availableActions = await _reviewHelper.GetAvailableActionsForTodoAsync(
                    todo, userId, stagesDict, transitionsDict);

                // 9. Convert to response DTO
                var response = new TodoDetailResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                    CreatedAt = todo.CreatedAt,
                    CurrentStageName = todo.CurrentStage?.StageName ?? "None",
                    CurrentReviewerUserName = todo.CurrentReviewerUserId.HasValue
                        ? GetUserNameFromDict(userNamesDict, todo.CurrentReviewerUserId.Value)
                        : "None",
                    ReviewHistory = reviews.Select(r => new TodoReviewHistoryResponse
                    {
                        ReviewedAt = r.ReviewedAt,
                        ReviewerUserName = GetUserNameFromDict(userNamesDict, r.ReviewerUserId),
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

        public async Task<ResultDto<ReviewActionResponse>> ExecuteApproveActionAsync(ReviewApproveRequest request)
        {
            try
            {
                // 1. Get todo item
                var todos = await _todoRepository.FindWithIncludesAsync(
                    t => t.TodoListId == request.TodoId,
                    t => t.CurrentStage,
                    t => t.ReviewTemplate);

                var todo = todos.FirstOrDefault();
                if (todo == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Todo ID {request.TodoId} does not exist");
                }

                // 2. Check if user is the current reviewer
                if (todo.CurrentReviewerUserId != request.UserId)
                {
                    return ResultDto<ReviewActionResponse>.Failure("You are not the current reviewer");
                }

                // 3. Check if todo item is in a reviewable state
                if (todo.CurrentStageId == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure("This todo is not in a reviewable state");
                }

                // 4. Check if user has review permission for this stage
                var userRoles = await _queryHelper.GetUserRolesAsync(request.UserId);
                if (todo.CurrentStage != null && !userRoles.Contains(todo.CurrentStage.RequiredRoleId))
                {
                    return ResultDto<ReviewActionResponse>.Failure("You do not have the required role to review this stage");
                }

                // 5. Query approve transition rule
                var transitions = await _transitionRepository.FindAsync(t =>
                    t.StageId == todo.CurrentStageId && t.ActionName == ReviewConstantsHelper.ActionApprove);

                var transition = transitions.FirstOrDefault();
                if (transition == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Action 'approve' is not valid for this stage");
                }

                // 6. Create review record
                var review = new Reviews
                {
                    TodoId = request.TodoId,
                    ReviewerUserId = request.UserId,
                    Action = ReviewConstantsHelper.ActionApprove,
                    Comment = request.Comment,
                    ReviewedAt = DateTime.UtcNow,
                    PreviousStatus = todo.Status,
                    NewStatus = transition.ResultStatus,
                    StageId = todo.CurrentStageId
                };

                await _reviewRepository.AddAsync(review);

                // 7. Update todo item status
                var previousStatus = todo.Status;
                todo.Status = transition.ResultStatus;

                // 8. Process approve action
                await _reviewHelper.HandleApproveAction(todo, transition, request.NextReviewerId);

                // 9. Save changes
                await _todoRepository.UpdateAsync(todo);

                // 10. Get next stage information
                string? nextStageName = null;
                string? currentReviewerUserName = null;
                bool isCompleted = false;

                if (todo.CurrentStageId.HasValue)
                {
                    var stage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
                    nextStageName = stage?.StageName;

                    if (todo.CurrentReviewerUserId.HasValue)
                    {
                        currentReviewerUserName = await _userNameHelper.GetUserNameAsync(todo.CurrentReviewerUserId.Value);
                    }

                    // Check if completed
                    if (ReviewConstantsHelper.IsFinalStatus(todo.Status))
                    {
                        isCompleted = true;
                    }
                }

                // 11. Return review result
                var response = new ReviewActionResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    PreviousStatus = previousStatus,
                    Action = ReviewConstantsHelper.ActionApprove,
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

                return ResultDto<ReviewActionResponse>.Success(response, "Review approved successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Error executing approve action: {ex.Message}");
            }
        }

        public async Task<ResultDto<ReviewActionResponse>> ExecuteReturnActionAsync(ReviewActionRequest request)
        {
            try
            {
                // 1. Get todo item
                var todos = await _todoRepository.FindWithIncludesAsync(
                    t => t.TodoListId == request.TodoId,
                    t => t.CurrentStage,
                    t => t.ReviewTemplate);

                var todo = todos.FirstOrDefault();
                if (todo == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Todo ID {request.TodoId} does not exist");
                }

                // 2. Check if user is the current reviewer
                if (todo.CurrentReviewerUserId != request.UserId)
                {
                    return ResultDto<ReviewActionResponse>.Failure("You are not the current reviewer");
                }

                // 3. Check if todo item is in a reviewable state
                if (todo.CurrentStageId == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure("This todo is not in a reviewable state");
                }

                // 4. Check if user has review permission for this stage
                var userRoles = await _queryHelper.GetUserRolesAsync(request.UserId);
                if (todo.CurrentStage != null && !userRoles.Contains(todo.CurrentStage.RequiredRoleId))
                {
                    return ResultDto<ReviewActionResponse>.Failure("You do not have the required role to review this stage");
                }

                // 5. Query return transition rule
                var transitions = await _transitionRepository.FindAsync(t =>
                    t.StageId == todo.CurrentStageId && t.ActionName == ReviewConstantsHelper.ActionReturn);

                var transition = transitions.FirstOrDefault();
                if (transition == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Action 'return' is not valid for this stage");
                }

                // 6. Create review record
                var review = new Reviews
                {
                    TodoId = request.TodoId,
                    ReviewerUserId = request.UserId,
                    Action = ReviewConstantsHelper.ActionReturn,
                    Comment = request.Comment,
                    ReviewedAt = DateTime.UtcNow,
                    PreviousStatus = todo.Status,
                    NewStatus = transition.ResultStatus,
                    StageId = todo.CurrentStageId
                };

                await _reviewRepository.AddAsync(review);

                // 7. Update todo item status
                var previousStatus = todo.Status;
                todo.Status = transition.ResultStatus;

                // 8. Process return action
                await _reviewHelper.HandleReturnAction(todo, transition, request.UserId);

                // 9. Save changes
                await _todoRepository.UpdateAsync(todo);

                // 10. Get returned object information
                string? currentReviewerUserName = null;
                string? nextStageName = null;

                if (todo.CurrentReviewerUserId.HasValue)
                {
                    currentReviewerUserName = await _userNameHelper.GetUserNameAsync(todo.CurrentReviewerUserId.Value);
                }

                if (todo.CurrentStageId.HasValue)
                {
                    var stage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
                    nextStageName = stage?.StageName;
                }

                // 11. Return review result
                var response = new ReviewActionResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    PreviousStatus = previousStatus,
                    Action = ReviewConstantsHelper.ActionReturn,
                    ReviewId = review.ReviewId,
                    ReviewedAt = review.ReviewedAt,
                    Comment = review.Comment,
                    CurrentStageName = todo.CurrentStageId.HasValue
                        ? (await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value))?.StageName ?? "Unknown"
                        : "Returned",
                    CurrentReviewerUserName = currentReviewerUserName ?? "None",
                    NextStageName = nextStageName ?? "None",
                    IsCompleted = false
                };

                return ResultDto<ReviewActionResponse>.Success(response, "Todo returned successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Error executing return action: {ex.Message}");
            }
        }

        public async Task<ResultDto<ReviewActionResponse>> ExecuteRejectActionAsync(ReviewActionRequest request)
        {
            try
            {
                // 1. Get todo item
                var todos = await _todoRepository.FindWithIncludesAsync(
                    t => t.TodoListId == request.TodoId,
                    t => t.CurrentStage,
                    t => t.ReviewTemplate);

                var todo = todos.FirstOrDefault();
                if (todo == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Todo ID {request.TodoId} does not exist");
                }

                // 2. Check if user is the current reviewer
                if (todo.CurrentReviewerUserId != request.UserId)
                {
                    return ResultDto<ReviewActionResponse>.Failure("You are not the current reviewer");
                }

                // 3. Check if todo item is in a reviewable state
                if (todo.CurrentStageId == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure("This todo is not in a reviewable state");
                }

                // 4. Check if user has review permission for this stage
                var userRoles = await _queryHelper.GetUserRolesAsync(request.UserId);
                if (todo.CurrentStage != null && !userRoles.Contains(todo.CurrentStage.RequiredRoleId))
                {
                    return ResultDto<ReviewActionResponse>.Failure("You do not have the required role to review this stage");
                }

                // 5. Query reject transition rule
                var transitions = await _transitionRepository.FindAsync(t =>
                    t.StageId == todo.CurrentStageId && t.ActionName == ReviewConstantsHelper.ActionReject);

                var transition = transitions.FirstOrDefault();
                if (transition == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure($"Action 'reject' is not valid for this stage");
                }

                // 6. Create review record
                var review = new Reviews
                {
                    TodoId = request.TodoId,
                    ReviewerUserId = request.UserId,
                    Action = ReviewConstantsHelper.ActionReject,
                    Comment = request.Comment,
                    ReviewedAt = DateTime.UtcNow,
                    PreviousStatus = todo.Status,
                    NewStatus = transition.ResultStatus,
                    StageId = todo.CurrentStageId
                };

                await _reviewRepository.AddAsync(review);

                // 7. Update todo item status
                var previousStatus = todo.Status;
                todo.Status = transition.ResultStatus;

                // 8. Process reject action
                await _reviewHelper.HandleRejectAction(todo);

                // 9. Save changes
                await _todoRepository.UpdateAsync(todo);

                // 10. Return review result
                var response = new ReviewActionResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    PreviousStatus = previousStatus,
                    Action = ReviewConstantsHelper.ActionReject,
                    ReviewId = review.ReviewId,
                    ReviewedAt = review.ReviewedAt,
                    Comment = review.Comment,
                    CurrentStageName = "Rejected",
                    CurrentReviewerUserName = "None",
                    NextStageName = "None",
                    IsCompleted = true
                };

                return ResultDto<ReviewActionResponse>.Success(response, "Todo rejected successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<ReviewActionResponse>.Failure($"Error executing reject action: {ex.Message}");
            }
        }

        public async Task<ResultDto<ReviewActionResponse>> ExecuteResubmitActionAsync(ReviewActionRequest request)
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
                ReviewStages? targetStage = null;
                string newStatus = "";

                if (todo.Status == ReviewConstantsHelper.ReturnedToCreator)
                {
                    if (todo.CreatedByUserId != request.UserId)
                    {
                        return ResultDto<ReviewActionResponse>.Failure("Only the creator can resubmit");
                    }
                    targetStage = await _reviewHelper.GetStageByTemplateAndOrderAsync(todo.TemplateId ?? 0, 1);
                    newStatus = ReviewConstantsHelper.PendingReviewLevel1;
                }
                else if (todo.Status == ReviewConstantsHelper.ReturnedToReviewer)
                {
                    if (todo.CurrentReviewerUserId != request.UserId)
                    {
                        return ResultDto<ReviewActionResponse>.Failure("Only the current reviewer can resubmit");
                    }
                    if (todo.CurrentStageId.HasValue)
                    {
                        targetStage = await _stageRepository.GetByIdAsync(todo.CurrentStageId.Value);
                        newStatus = $"pending_review_stage{todo.CurrentStageId}";
                    }
                }

                if (targetStage == null)
                {
                    return ResultDto<ReviewActionResponse>.Failure("Unable to resubmit this todo item");
                }

                // 3. Create resubmit review record
                var review = new Reviews
                {
                    TodoId = request.TodoId,
                    ReviewerUserId = request.UserId,
                    Action = ReviewConstantsHelper.ActionResubmit,
                    Comment = request.Comment,
                    ReviewedAt = DateTime.UtcNow,
                    PreviousStatus = todo.Status,
                    NewStatus = newStatus,
                    StageId = targetStage.StageId
                };

                await _reviewRepository.AddAsync(review);

                // 4. Update todo item status
                var previousStatus = todo.Status;
                todo.Status = newStatus;
                todo.CurrentStageId = targetStage.StageId;
                todo.CurrentReviewerUserId = targetStage.SpecificReviewerUserId;

                await _todoRepository.UpdateAsync(todo);

                // 5. Get user name
                string? currentReviewerUserName = null;
                if (targetStage.SpecificReviewerUserId.HasValue)
                {
                    currentReviewerUserName = await _userNameHelper.GetUserNameAsync(targetStage.SpecificReviewerUserId.Value);
                }

                // 6. Return result
                var response = new ReviewActionResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    PreviousStatus = previousStatus,
                    Action = ReviewConstantsHelper.ActionResubmit,
                    ReviewId = review.ReviewId,
                    ReviewedAt = review.ReviewedAt,
                    Comment = review.Comment,
                    CurrentStageName = targetStage.StageName,
                    CurrentReviewerUserName = currentReviewerUserName ?? "None",
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
                             await _queryHelper.UserHasPermissionAsync(userId, ReviewConstantsHelper.PermissionAdminManage);

                if (!canView)
                {
                    return ResultDto<ReviewHistoryFullResponse>.Failure("You do not have permission to view review history");
                }

                // 3. Batch get all required data
                var reviews = await _reviewRepository.FindWithIncludesAsync(
                    r => r.TodoId == todoId,
                    r => r.ReviewerUser,
                    r => r.ReviewStage);

                // 4. Get all related user names
                var allUserIds = new List<int> { todo.CreatedByUserId };
                allUserIds.AddRange(reviews.Select(r => r.ReviewerUserId));
                var userNamesDict = await _userNameHelper.GetUserNamesAsync(allUserIds.Distinct().ToList());

                // 5. Create timeline item list
                var timelineItems = new List<ReviewTimelineItemResponse>();

                // Add creation record
                timelineItems.Add(new ReviewTimelineItemResponse
                {
                    Time = todo.CreatedAt,
                    Stage = "Creation",
                    ReviewerUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
                    Action = ReviewConstantsHelper.ActionCreated,
                    ResultStatus = todo.Status,
                    ActionDisplayName = ReviewConstantsHelper.GetActionDisplayName(ReviewConstantsHelper.ActionCreated),
                    StatusDisplayName = ReviewConstantsHelper.GetStatusDisplayName(todo.Status),
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
                        ReviewerUserName = GetUserNameFromDict(userNamesDict, review.ReviewerUserId),
                        Action = review.Action,
                        ResultStatus = review.NewStatus ?? "",
                        Comment = review.Comment,
                        ActionDisplayName = ReviewConstantsHelper.GetActionDisplayName(review.Action),
                        StatusDisplayName = ReviewConstantsHelper.GetStatusDisplayName(review.NewStatus)
                    });
                }

                // 6. Calculate review summary
                var summary = _reviewHelper.CalculateReviewSummary(todo, timelineItems);

                // 7. Build response
                var response = new ReviewHistoryFullResponse
                {
                    TodoListId = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserName = GetUserNameFromDict(userNamesDict, todo.CreatedByUserId),
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

        private string GetUserNameFromDict(Dictionary<int, string> userNamesDict, int userId)
        {
            return userNamesDict.TryGetValue(userId, out var userName)
                ? userName
                : $"User{userId}";
        }
    }
}