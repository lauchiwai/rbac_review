using Common.DTOs;
using Common.DTOs.ReviewTodoLists.Requests;
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Responses;
using Common.Enums;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;
using System.Net;

namespace Services.Implementations
{
    public class TodoReviewService : ITodoReviewService
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRepository<Users> _userRepository;
        private readonly IRepository<Users_Roles> _userRoleRepository;
        private readonly IRepository<Roles> _roleRepository;
        private readonly IRbacService _rbacService;
        private readonly ReviewQueryHelper _reviewQueryHelper;

        public TodoReviewService(
            IRepository<TodoLists> todoRepository,
            IRepository<Reviews> reviewRepository,
            IRepository<Users> userRepository,
            IRepository<Users_Roles> userRoleRepository,
            IRepository<Roles> roleRepository,
            IRbacService rbacService,
            ReviewQueryHelper reviewQueryHelper)
        {
            _todoRepository = todoRepository;
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _rbacService = rbacService;
            _reviewQueryHelper = reviewQueryHelper;
        }

        public async Task<ResultDto<ReviewHistoryItem>> ReviewTodoAsync(ReviewTodoRequest request)
        {
            try
            {
                var todo = await _todoRepository.GetByIdAsync(request.TodoId);
                if (todo == null)
                    return ResultDto<ReviewHistoryItem>.NotFound($"Todo with ID {request.TodoId} not found");

                var reviewer = await _userRepository.GetByIdAsync(request.ReviewerUserId);
                if (reviewer == null)
                    return ResultDto<ReviewHistoryItem>.NotFound($"Reviewer with ID {request.ReviewerUserId} not found");

                var action = request.Action.ToLower();

                if (action == "resubmit")
                {
                    return await ProcessResubmitAsync(todo, request);
                }
                else
                {
                    return await ProcessReviewActionAsync(todo, request);
                }
            }
            catch (Exception ex)
            {
                return ResultDto<ReviewHistoryItem>.Failure($"Error processing review: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        private async Task<ResultDto<ReviewHistoryItem>> ProcessReviewActionAsync(TodoLists todo, ReviewTodoRequest request)
        {
            var action = request.Action.ToLower();

            bool hasPermission = await CheckReviewPermissionAsync(todo, request.ReviewerUserId);
            if (!hasPermission)
                return ResultDto<ReviewHistoryItem>.Forbidden("You do not have permission to review this todo");

            var previousStatus = todo.Status;
            if (!IsValidTransition(previousStatus, action))
                return ResultDto<ReviewHistoryItem>.BadRequest($"Invalid action '{action}' for status '{previousStatus}'");

            var processResult = await ProcessReviewWithStatusAsync(todo, request, previousStatus);
            if (!processResult.IsSuccess)
                return ResultDto<ReviewHistoryItem>.BadRequest(processResult.Message);

            int? nextReviewerUserId = null;

            nextReviewerUserId = await DetermineNextReviewerAsync(todo, request, previousStatus, action);

            if (nextReviewerUserId.HasValue)
                todo.CurrentReviewerUserId = nextReviewerUserId.Value;
            else
                todo.CurrentReviewerUserId = null;

            await _todoRepository.UpdateAsync(todo);

            var review = new Reviews
            {
                TodoId = request.TodoId,
                ReviewerUserId = request.ReviewerUserId,
                ReviewLevel = GetReviewLevelByStatus(previousStatus),
                Action = request.Action,
                Comment = request.Comment,
                PreviousStatus = previousStatus,
                NewStatus = todo.Status,
                NextReviewerUserId = nextReviewerUserId,
                ReviewedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            var response = new ReviewHistoryItem
            {
                ReviewId = review.ReviewId,
                TodoId = review.TodoId,
                Action = review.Action,
                ReviewerUserId = review.ReviewerUserId,
                Comment = review.Comment,
                ReviewedAt = review.ReviewedAt,
                PreviousStatus = review.PreviousStatus,
                NewStatus = review.NewStatus,
                NextReviewerUserId = review.NextReviewerUserId
            };

            return ResultDto<ReviewHistoryItem>.Success(response);
        }

        private async Task<bool> CheckReviewPermissionAsync(TodoLists todo, int reviewerUserId)
        {
            if (todo.CreatedByUserId == reviewerUserId && todo.Status != ReviewAction.Returned)
                return false;

            var userRoles = await GetUserRolesWithRoleAsync(reviewerUserId);
            if (userRoles.Count == 0)
                return false;

            foreach (var userRole in userRoles)
            {
                if (userRole.Role == null) continue;

                if (todo.Status == ReviewAction.Pending || todo.Status == ReviewAction.ReturnedToLevel1)
                {
                    var hasPermission = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel1.GetPermissionName());
                    if (hasPermission.IsSuccess && hasPermission.Data)
                        return true;
                }
                else if (todo.Status == ReviewAction.InProgress)
                {
                    var hasPermission = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel2.GetPermissionName());
                    if (hasPermission.IsSuccess && hasPermission.Data)
                        return true;
                }
                else if (todo.Status == ReviewAction.Returned)
                {
                    var hasPermission = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel1.GetPermissionName());
                    if (hasPermission.IsSuccess && hasPermission.Data)
                        return true;
                }
            }

            return false;
        }

        private bool IsValidTransition(string currentStatus, string action)
        {
            return (currentStatus, action.ToLower()) switch
            {
                (ReviewAction.Pending, "approve") => true,
                (ReviewAction.Pending, "return") => true,
                (ReviewAction.Pending, "reject") => true,

                (ReviewAction.InProgress, "approve") => true,
                (ReviewAction.InProgress, "return") => true,
                (ReviewAction.InProgress, "reject") => true,

                (ReviewAction.ReturnedToLevel1, "approve") => true,
                (ReviewAction.ReturnedToLevel1, "return") => true,
                (ReviewAction.ReturnedToLevel1, "reject") => true,

                _ => false
            };
        }

        private async Task<ResultDto> ProcessReviewWithStatusAsync(TodoLists todo, ReviewTodoRequest request, string previousStatus)
        {
            var action = request.Action.ToLower();

            switch (previousStatus)
            {
                case ReviewAction.Pending:
                    return await ProcessPendingReviewAsync(todo, action);

                case ReviewAction.InProgress:
                    return await ProcessInProgressReviewAsync(todo, action);

                case ReviewAction.ReturnedToLevel1:
                    return await ProcessReturnedToLevel1ReviewAsync(todo, action);

                default:
                    return ResultDto.BadRequest($"Invalid review operation for status: {previousStatus}");
            }
        }

        private async Task<ResultDto> ProcessPendingReviewAsync(TodoLists todo, string action)
        {
            switch (action)
            {
                case "approve":
                    todo.Status = ReviewAction.InProgress;
                    return ResultDto.Success();

                case "return":
                    todo.Status = ReviewAction.Returned;
                    return ResultDto.Success();

                case "reject":
                    todo.Status = ReviewAction.Rejected;
                    return ResultDto.Success();

                default:
                    return ResultDto.BadRequest($"Invalid action for pending review: {action}");
            }
        }

        private async Task<ResultDto> ProcessInProgressReviewAsync(TodoLists todo, string action)
        {
            switch (action)
            {
                case "approve":
                    todo.Status = ReviewAction.Approved;
                    return ResultDto.Success();

                case "return":
                    todo.Status = ReviewAction.ReturnedToLevel1;
                    return ResultDto.Success();

                case "reject":
                    todo.Status = ReviewAction.Rejected;
                    return ResultDto.Success();

                default:
                    return ResultDto.BadRequest($"Invalid action for in-progress review: {action}");
            }
        }

        private async Task<ResultDto> ProcessReturnedToLevel1ReviewAsync(TodoLists todo, string action)
        {
            switch (action)
            {
                case "approve":
                    todo.Status = ReviewAction.InProgress;
                    return ResultDto.Success();

                case "return":
                    todo.Status = ReviewAction.Returned;
                    return ResultDto.Success();

                case "reject":
                    todo.Status = ReviewAction.Rejected;
                    return ResultDto.Success();

                default:
                    return ResultDto.BadRequest($"Invalid action for returned to level 1 review: {action}");
            }
        }

        private async Task<int?> DetermineNextReviewerAsync(TodoLists todo, ReviewTodoRequest request, string previousStatus, string action)
        {
            if (action == "reject")
                return null;

            if (action == "approve" && todo.Status == ReviewAction.Approved)
                return null;

            if (action == "return")
            {
                return await HandleReturnReviewerAsync(todo, request, previousStatus);
            }

            if (action == "approve")
            {
                return await HandleApproveReviewerAsync(todo, request, previousStatus);
            }

            return null;
        }

        private async Task<int?> HandleReturnReviewerAsync(TodoLists todo, ReviewTodoRequest request, string previousStatus)
        {
            if (previousStatus == ReviewAction.InProgress)
            {
                if (request.NextReviewerUserId.HasValue)
                {
                    bool hasLevel1Permission = await ValidateReviewerPermissionAsync(
                        request.NextReviewerUserId.Value,
                        Permission.TodoReviewLevel1);

                    if (!hasLevel1Permission)
                        throw new Exception("Return target does not have level 1 review permission");

                    return request.NextReviewerUserId.Value;
                }
                else
                {
                    return await FindUserWithPermissionAsync(Permission.TodoReviewLevel1);
                }
            }
            else if (previousStatus == ReviewAction.Pending || previousStatus == ReviewAction.ReturnedToLevel1)
            {
                return todo.CreatedByUserId;
            }

            return null;
        }

        private async Task<int?> HandleApproveReviewerAsync(TodoLists todo, ReviewTodoRequest request, string previousStatus)
        {
            if ((previousStatus == ReviewAction.Pending || previousStatus == ReviewAction.ReturnedToLevel1)
                && todo.Status == ReviewAction.InProgress)
            {
                if (request.NextReviewerUserId.HasValue)
                {
                    bool hasLevel2Permission = await ValidateReviewerPermissionAsync(
                        request.NextReviewerUserId.Value,
                        Permission.TodoReviewLevel2);

                    if (!hasLevel2Permission)
                        throw new Exception("Next reviewer does not have level 2 review permission");

                    return request.NextReviewerUserId.Value;
                }
                else
                {
                    return await FindUserWithPermissionAsync(Permission.TodoReviewLevel2);
                }
            }

            return null;
        }

        private async Task<ResultDto<ReviewHistoryItem>> ProcessResubmitAsync(TodoLists todo, ReviewTodoRequest request)
        {
            if (todo.Status != ReviewAction.Returned)
                return ResultDto<ReviewHistoryItem>.BadRequest("Todo must be in returned status to resubmit");

            bool canResubmit = false;

            if (todo.CreatedByUserId == request.ReviewerUserId)
            {
                canResubmit = true;
            }
            else
            {
                var userRoles = await GetUserRolesWithRoleAsync(request.ReviewerUserId);
                foreach (var userRole in userRoles)
                {
                    if (userRole.Role == null) continue;

                    var hasReviewLevel1Permission = await _rbacService.HasPermissionAsync(
                        userRole.Role.RoleId,
                        Permission.TodoReviewLevel1.GetPermissionName());

                    if (hasReviewLevel1Permission.IsSuccess && hasReviewLevel1Permission.Data)
                    {
                        canResubmit = true;
                        break;
                    }
                }
            }

            if (!canResubmit)
                return ResultDto<ReviewHistoryItem>.Forbidden("You do not have permission to resubmit this todo");

            var previousStatus = todo.Status;
            todo.Status = ReviewAction.Pending;

            int? nextReviewerUserId = null;
            if (request.NextReviewerUserId.HasValue)
            {
                bool hasLevel1Permission = await ValidateReviewerPermissionAsync(
                    request.NextReviewerUserId.Value,
                    Permission.TodoReviewLevel1);

                if (!hasLevel1Permission)
                {
                    return ResultDto<ReviewHistoryItem>.BadRequest(
                        "Next reviewer does not have level 1 review permission");
                }

                nextReviewerUserId = request.NextReviewerUserId.Value;
                todo.CurrentReviewerUserId = nextReviewerUserId;
            }
            else
            {
                var level1Reviewer = await FindUserWithPermissionAsync(Permission.TodoReviewLevel1);
                todo.CurrentReviewerUserId = level1Reviewer;
                nextReviewerUserId = level1Reviewer;
            }

            await _todoRepository.UpdateAsync(todo);

            var review = new Reviews
            {
                TodoId = request.TodoId,
                ReviewerUserId = request.ReviewerUserId,
                ReviewLevel = GetReviewLevelByStatus(previousStatus),
                Action = request.Action,
                Comment = request.Comment,
                PreviousStatus = previousStatus,
                NewStatus = todo.Status,
                NextReviewerUserId = nextReviewerUserId,
                ReviewedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            var response = new ReviewHistoryItem
            {
                ReviewId = review.ReviewId,
                TodoId = review.TodoId,
                Action = review.Action,
                ReviewerUserId = review.ReviewerUserId,
                Comment = review.Comment,
                ReviewedAt = review.ReviewedAt,
                PreviousStatus = review.PreviousStatus,
                NewStatus = review.NewStatus,
                NextReviewerUserId = review.NextReviewerUserId
            };

            return ResultDto<ReviewHistoryItem>.Success(response);
        }

        private async Task<int?> FindUserWithPermissionAsync(Permission permission)
        {
            try
            {
                var allUserRoles = await _userRoleRepository.FindWithIncludesAsync(
                    null,
                    ur => ur.Role);

                var rolesWithPermission = new Dictionary<int, bool>();
                foreach (var userRole in allUserRoles)
                {
                    if (userRole.Role == null) continue;

                    if (!rolesWithPermission.ContainsKey(userRole.Role.RoleId))
                    {
                        var hasPermission = await _rbacService.HasPermissionAsync(
                            userRole.Role.RoleId,
                            permission.GetPermissionName());

                        rolesWithPermission[userRole.Role.RoleId] =
                            hasPermission.IsSuccess && hasPermission.Data;
                    }

                    if (rolesWithPermission[userRole.Role.RoleId])
                    {
                        return userRole.UserId;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<bool> ValidateReviewerPermissionAsync(int userId, Permission requiredPermission)
        {
            var userRoles = await GetUserRolesWithRoleAsync(userId);

            foreach (var userRole in userRoles)
            {
                if (userRole.Role == null) continue;

                var hasPermission = await _rbacService.HasPermissionAsync(
                    userRole.Role.RoleId,
                    requiredPermission.GetPermissionName());

                if (hasPermission.IsSuccess && hasPermission.Data)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<List<Users_Roles>> GetUserRolesWithRoleAsync(int userId)
        {
            try
            {
                var userRoles = await _userRoleRepository.FindWithIncludesAsync(
                    ur => ur.UserId == userId,
                    ur => ur.Role);

                if (userRoles == null)
                    return new List<Users_Roles>();

                return userRoles
                    .Where(ur => ur != null && ur.Role != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user roles for user {userId}: {ex.Message}");
                return new List<Users_Roles>();
            }
        }

        private int GetReviewLevelByStatus(string status)
        {
            return status switch
            {
                ReviewAction.Pending => (int)ReviewLevel.Level1,
                ReviewAction.InProgress => (int)ReviewLevel.Level2,
                ReviewAction.Approved => (int)ReviewLevel.Level2,
                ReviewAction.ReturnedToLevel1 => (int)ReviewLevel.Level1,
                _ => (int)ReviewLevel.None
            };
        }

        public async Task<ResultDto<List<TodoWithReviewHistoryViewModel>>> GetReviewTodosAsync(GetReviewTodosRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
                if (user == null)
                    return ResultDto<List<TodoWithReviewHistoryViewModel>>.NotFound($"User with ID {request.CurrentUserId} not found");

                var result = await _reviewQueryHelper.GetReviewTodosAsync(request.CurrentUserId, request.Status);
                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Success(result);
            }
            catch (Exception ex)
            {
                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Failure(
                    $"Error retrieving review todos: {ex.Message}",
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<List<ReviewHistoryItem>>> GetReviewHistoryAsync(int todoId, int currentUserId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return ResultDto<List<ReviewHistoryItem>>.NotFound($"User with ID {currentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<List<ReviewHistoryItem>>.NotFound($"Todo with ID {todoId} not found");

                bool canView = await CanViewReviewHistoryAsync(currentUserId, todoId, todo);

                if (!canView)
                    return ResultDto<List<ReviewHistoryItem>>.Forbidden("Access denied to review history");

                var reviews = await _reviewRepository.FindAsync(r => r.TodoId == todoId);
                var response = reviews
                    .OrderByDescending(r => r.ReviewedAt)
                    .Select(review => new ReviewHistoryItem
                    {
                        ReviewId = review.ReviewId,
                        TodoId = review.TodoId,
                        Action = review.Action,
                        ReviewerUserId = review.ReviewerUserId,
                        Comment = review.Comment,
                        ReviewedAt = review.ReviewedAt,
                        PreviousStatus = review.PreviousStatus,
                        NewStatus = review.NewStatus,
                        NextReviewerUserId = review.NextReviewerUserId
                    }).ToList();

                return ResultDto<List<ReviewHistoryItem>>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<List<ReviewHistoryItem>>.Failure(
                    $"Error retrieving review history: {ex.Message}",
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }

        private async Task<bool> CanViewReviewHistoryAsync(int currentUserId, int todoId, TodoLists todo)
        {
            if (todo.CreatedByUserId == currentUserId)
                return true;

            if (todo.CurrentReviewerUserId == currentUserId)
                return true;

            var participation = await _reviewRepository.FindAsync(r =>
                r.TodoId == todoId && r.ReviewerUserId == currentUserId);

            return participation.Any();
        }
    }
}