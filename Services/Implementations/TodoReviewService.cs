using Common.DTOs;
using Common.DTOs.ReviewTodoLists.Requests;
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Responses;
using Common.Enums;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Net;

namespace Services.Implementations
{
    public class TodoReviewService : ITodoReviewService
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRbacService _rbacService;
        private readonly IRolesService _roleService;

        public TodoReviewService(
            IRepository<TodoLists> todoRepository,
            IRepository<Reviews> reviewRepository,
            IRbacService rbacService,
            IRolesService roleService)
        {
            _todoRepository = todoRepository;
            _reviewRepository = reviewRepository;
            _rbacService = rbacService;
            _roleService = roleService;
        }

        public async Task<ResultDto<List<TodoWithReviewHistoryViewModel>>> GetReviewTodosAsync(GetReviewTodosRequest request)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(request.CurrentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<List<TodoWithReviewHistoryViewModel>>.BadRequest(roleValidation.ErrorMessage);

                var hasViewOwnPermission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewOwn.GetPermissionName());
                var hasViewLevel1Permission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewLevel1.GetPermissionName());
                var hasViewLevel2Permission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewLevel2.GetPermissionName());

                if (!hasViewOwnPermission.Data && !hasViewLevel1Permission.Data && !hasViewLevel2Permission.Data)
                    return ResultDto<List<TodoWithReviewHistoryViewModel>>.Forbidden("User does not have view permissions");

                var todos = await _todoRepository.FindAsync(todo => true);

                var filteredTodos = todos.Where(todo =>
                {
                    if (hasViewOwnPermission.Data && todo.CreatedByRole == request.CurrentUserRoleId)
                        return true;

                    if (hasViewLevel1Permission.Data &&
                        (todo.Status == ReviewAction.Pending || todo.Status == ReviewAction.Returned))
                        return true;

                    if (hasViewLevel2Permission.Data && todo.Status == ReviewAction.InProgress)
                        return true;

                    return false;
                }).ToList();

                if (!string.IsNullOrEmpty(request.Status))
                {
                    filteredTodos = filteredTodos.Where(todo => todo.Status == request.Status).ToList();
                }

                var todoIds = filteredTodos.Select(t => t.TodoListId).ToList();

                var allReviews = await _reviewRepository.FindAsync(r => todoIds.Contains(r.TodoId));
                var reviewsList = allReviews.ToList();

                var reviewsGrouped = reviewsList
                    .GroupBy(r => r.TodoId)
                    .ToDictionary(g => g.Key, g => g.OrderBy(r => r.ReviewId).ToList()); 

                var response = new List<TodoWithReviewHistoryViewModel>();
                foreach (var todo in filteredTodos)
                {
                    var reviews = reviewsGrouped.ContainsKey(todo.TodoListId)
                        ? reviewsGrouped[todo.TodoListId]
                        : new List<Reviews>();

                    var reviewHistories = reviews.Select(review => new ReviewHistoryViewModel
                    {
                        ReviewId = review.ReviewId,
                        ReviewerRoleId = review.ReviewerRole,
                        Action = review.Action,
                        Comment = review.Comment,
                        PreviousStatus = review.PreviousStatus,
                        NewStatus = review.NewStatus,
                        CreatedAt = review.ReviewedAt
                    }).ToList();

                    response.Add(new TodoWithReviewHistoryViewModel
                    {
                        Id = todo.TodoListId,
                        Title = todo.Title,
                        Status = todo.Status,
                        CreatedByRoleId = todo.CreatedByRole,
                        CreatedAt = todo.CreatedAt,
                        ReviewHistories = reviewHistories
                    });
                }

                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Failure($"Error retrieving review todos: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<ReviewHistoryItem>> ReviewTodoAsync(ReviewTodoRequest request)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(request.ReviewerRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<ReviewHistoryItem>.BadRequest(roleValidation.ErrorMessage);

                var todo = await _todoRepository.GetByIdAsync(request.TodoId);
                if (todo == null)
                    return ResultDto<ReviewHistoryItem>.NotFound($"Todo with ID {request.TodoId} not found");

                var previousStatus = todo.Status;

                var processResult = await ProcessReviewAsync(todo, request);
                if (!processResult.IsSuccess)
                {
                    return ResultDto<ReviewHistoryItem>.BadRequest(processResult.Message);
                }

                await _todoRepository.UpdateAsync(todo);

                var review = new Reviews
                {
                    TodoId = request.TodoId,
                    ReviewerRole = request.ReviewerRoleId,
                    ReviewLevel = request.NextReviewLevel ?? (int)GetReviewLevelByRole(request.ReviewerRoleId),
                    Action = request.Action,
                    Comment = request.Comment,
                    PreviousStatus = previousStatus,
                    NewStatus = todo.Status,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(review);

                var response = new ReviewHistoryItem
                {
                    TodoId = review.TodoId,
                    Action = review.Action,
                    ReviewerRoleId = review.ReviewerRole,
                    Comment = review.Comment,
                    ReviewedAt = review.ReviewedAt,
                    PreviousStatus = review.PreviousStatus,
                    NewStatus = review.NewStatus
                };

                return ResultDto<ReviewHistoryItem>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<ReviewHistoryItem>.Failure($"Error processing review: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<List<ReviewHistoryItem>>> GetReviewHistoryAsync(int todoId, int currentUserRoleId)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(currentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<List<ReviewHistoryItem>>.BadRequest(roleValidation.ErrorMessage);

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<List<ReviewHistoryItem>>.NotFound($"Todo with ID {todoId} not found");

                var hasViewOwnPermission = await _rbacService.HasPermissionAsync(currentUserRoleId, Permission.TodoViewOwn.GetPermissionName());
                var hasViewLevel1Permission = await _rbacService.HasPermissionAsync(currentUserRoleId, Permission.TodoViewLevel1.GetPermissionName());
                var hasViewLevel2Permission = await _rbacService.HasPermissionAsync(currentUserRoleId, Permission.TodoViewLevel2.GetPermissionName());

                var canView = false;

                if (hasViewOwnPermission.Data && todo.CreatedByRole == currentUserRoleId)
                    canView = true;

                if (hasViewLevel1Permission.Data &&
                    (todo.Status == ReviewAction.Pending || todo.Status == ReviewAction.Returned))
                    canView = true;
                if (hasViewLevel2Permission.Data && todo.Status == ReviewAction.InProgress)
                    canView = true;

                if (!canView)
                {
                    var hasParticipated = await HasParticipatedInReviewAsync(currentUserRoleId, todoId);
                    canView = hasParticipated;
                }

                if (!canView)
                    return ResultDto<List<ReviewHistoryItem>>.Forbidden("Access denied to review history");

                var reviews = await _reviewRepository.FindAsync(r => r.TodoId == todoId);
                var response = reviews.Select(review => new ReviewHistoryItem
                {
                    TodoId = review.TodoId,
                    Action = review.Action,
                    ReviewerRoleId = review.ReviewerRole,
                    Comment = review.Comment,
                    ReviewedAt = review.ReviewedAt,
                    PreviousStatus = review.PreviousStatus,
                    NewStatus = review.NewStatus
                }).OrderByDescending(r => r.ReviewedAt).ToList();

                return ResultDto<List<ReviewHistoryItem>>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<List<ReviewHistoryItem>>.Failure($"Error retrieving review history: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        private async Task<bool> HasParticipatedInReviewAsync(int roleId, int todoId)
        {
            var participation = await _reviewRepository.FindAsync(r =>
                r.TodoId == todoId && r.ReviewerRole == roleId);
            return participation.Any();
        }

        private async Task<ResultDto> ProcessReviewAsync(TodoLists todo, ReviewTodoRequest request)
        {
            var hasLevel1Permission = await _rbacService.HasPermissionAsync(request.ReviewerRoleId, Permission.TodoReviewLevel1.GetPermissionName());
            var hasLevel2Permission = await _rbacService.HasPermissionAsync(request.ReviewerRoleId, Permission.TodoReviewLevel2.GetPermissionName());

            if (todo.Status == ReviewAction.Pending && hasLevel1Permission.Data)
            {
                return ProcessLevel1Review(todo, request.Action);
            }
            else if (todo.Status == ReviewAction.InProgress && hasLevel2Permission.Data)
            {
                return ProcessLevel2Review(todo, request.Action);
            }
            else if (todo.Status == ReviewAction.Returned && todo.CreatedByRole == request.ReviewerRoleId)
            {
                if (request.Action == "resubmit")
                {
                    todo.Status = ReviewAction.Pending;
                    return ResultDto.Success();
                }
            }

            return ResultDto.BadRequest("Invalid review operation");
        }

        private ResultDto ProcessLevel1Review(TodoLists todo, string action)
        {
            var newStatus = action.ToLower() switch
            {
                "approve" => ReviewAction.InProgress,
                "return" => ReviewAction.Returned,
                "reject" => ReviewAction.Rejected,
                _ => null
            };

            if (newStatus == null)
                return ResultDto.BadRequest($"Invalid action for level 1 review: {action}");

            todo.Status = newStatus;
            return ResultDto.Success();
        }

        private ResultDto ProcessLevel2Review(TodoLists todo, string action)
        {
            var newStatus = action.ToLower() switch
            {
                "approve" => ReviewAction.Completed,
                "return" => ReviewAction.Returned,
                "reject" => ReviewAction.Rejected,
                _ => null
            };

            if (newStatus == null)
                return ResultDto.BadRequest($"Invalid action for level 2 review: {action}");

            todo.Status = newStatus;
            return ResultDto.Success();
        }

        private ReviewLevel GetReviewLevelByRole(int roleId)
        {
            return roleId switch
            {
                2 => ReviewLevel.Level1,  
                3 => ReviewLevel.Level2,  
                _ => ReviewLevel.None    
            };
        }

        private async Task<(bool IsSuccess, string ErrorMessage)> ValidateRoleExistsAsync(int roleId)
        {
            try
            {
                var roleResult = await _roleService.GetRoleByIdAsync(roleId);
                if (!roleResult.IsSuccess || roleResult.Data == null)
                {
                    return (false, $"Role with ID {roleId} does not exist");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Error validating role: {ex.Message}");
            }
        }
    }
}
