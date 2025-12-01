using Common.DTOs;
using Common.DTOs.ReviewTodoLists.Requests;
using Common.DTOs.ReviewTodoLists.Responses;
using Common.DTOs.TodoLists.Requests;
using Common.DTOs.TodoLists.Responses;
using Common.Enums;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;
using System.Net;

namespace Services.Implementations
{
    public class TodoListsService : ITodoListsService
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRepository<Users> _userRepository;
        private readonly IRepository<Users_Roles> _userRoleRepository;
        private readonly IRepository<Roles> _roleRepository;
        private readonly IRbacService _rbacService;
        private readonly TodoQueryHelper _todoQueryHelper;

        public TodoListsService(
            IRepository<TodoLists> todoRepository,
            IRepository<Reviews> reviewRepository,
            IRepository<Users> userRepository,
            IRepository<Users_Roles> userRoleRepository,
            IRepository<Roles> roleRepository,
            IRbacService rbacService,
            TodoQueryHelper todoQueryHelper)
        {
            _todoRepository = todoRepository;
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _rbacService = rbacService;
            _todoQueryHelper = todoQueryHelper;
        }

        public async Task<ResultDto<List<TodoWithReviewHistoryViewModel>>> GetReviewTodosAsync(GetReviewTodosRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
                if (user == null)
                    return ResultDto<List<TodoWithReviewHistoryViewModel>>.NotFound($"User with ID {request.CurrentUserId} not found");

                var result = await _todoQueryHelper.GetReviewTodosAsync(request.CurrentUserId, request.Status);

                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Success(result);
            }
            catch (Exception ex)
            {
                return ResultDto<List<TodoWithReviewHistoryViewModel>>.Failure(
                    $"Error retrieving review todos: {ex.Message}",
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<List<TodoViewModel>>> GetAllTodosAsync(GetTodosRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
                if (user == null)
                    return ResultDto<List<TodoViewModel>>.NotFound($"User with ID {request.CurrentUserId} not found");

                var userRoles = await GetUserRolesAsync(request.CurrentUserId);
                if (userRoles.Count == 0)
                    return ResultDto<List<TodoViewModel>>.Forbidden("User does not have any roles assigned");

                var todosQuery = _todoRepository.GetQueryable();

                var filteredQuery = todosQuery.Where(t =>
                    (t.CreatedByUserId == request.CurrentUserId) ||
                    (t.CurrentReviewerUserId == request.CurrentUserId));

                if (!string.IsNullOrEmpty(request.Status))
                {
                    filteredQuery = filteredQuery.Where(t => t.Status == request.Status);
                }

                var todos = await filteredQuery
                    .Select(t => new TodoViewModel
                    {
                        Id = t.TodoListId,
                        Title = t.Title,
                        Status = t.Status,
                        CreatedByUserId = t.CreatedByUserId,
                        CurrentReviewerUserId = t.CurrentReviewerUserId,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                return ResultDto<List<TodoViewModel>>.Success(todos);
            }
            catch (Exception ex)
            {
                return ResultDto<List<TodoViewModel>>.Failure(
                    $"Error retrieving todos: {ex.Message}",
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> CreateTodoAsync(CreateTodoRequest request)
        {
            try
            {
                var creator = await _userRepository.GetByIdAsync(request.CreatedByUserId);
                if (creator == null)
                    return ResultDto<TodoViewModel>.NotFound($"User with ID {request.CreatedByUserId} not found");

                var reviewer = await _userRepository.GetByIdAsync(request.ReviewerUserId);
                if (reviewer == null)
                    return ResultDto<TodoViewModel>.NotFound($"Reviewer with ID {request.ReviewerUserId} not found");

                var creatorRoles = await GetUserRolesAsync(request.CreatedByUserId);
                if (creatorRoles.Count == 0)
                    return ResultDto<TodoViewModel>.Forbidden($"Creator does not have any roles assigned");

                bool hasCreatePermission = await CheckPermissionAsync(
                    creatorRoles,
                    Permission.TodoCreate.GetPermissionName());

                if (!hasCreatePermission)
                    return ResultDto<TodoViewModel>.Forbidden($"Creator does not have {Permission.TodoCreate.GetDescription()}");

                var reviewerRoles = await GetUserRolesAsync(request.ReviewerUserId);
                if (reviewerRoles.Count == 0)
                    return ResultDto<TodoViewModel>.BadRequest($"Reviewer does not have any roles assigned");

                bool hasReviewLevel1Permission = await CheckPermissionAsync(
                    reviewerRoles,
                    Permission.TodoReviewLevel1.GetPermissionName());

                if (!hasReviewLevel1Permission)
                    return ResultDto<TodoViewModel>.BadRequest($"Reviewer does not have {Permission.TodoReviewLevel1.GetDescription()}");

                if (request.CreatedByUserId == request.ReviewerUserId)
                    return ResultDto<TodoViewModel>.BadRequest("Cannot assign the creator as the reviewer");

                var todo = new TodoLists
                {
                    Title = request.Title,
                    Status = ReviewAction.Pending,
                    CreatedByUserId = request.CreatedByUserId,
                    CurrentReviewerUserId = request.ReviewerUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _todoRepository.AddAsync(todo);

                var initialReview = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerUserId = request.CreatedByUserId,
                    ReviewLevel = 0,
                    Action = "create",
                    Comment = request.Title,
                    PreviousStatus = null,
                    NewStatus = ReviewAction.Pending,
                    NextReviewerUserId = request.ReviewerUserId,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(initialReview);

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserId = todo.CreatedByUserId,
                    CurrentReviewerUserId = todo.CurrentReviewerUserId,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response, statusCode: HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error creating todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> GetTodoAsync(int todoId, int currentUserId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return ResultDto<TodoViewModel>.NotFound($"User with ID {currentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<TodoViewModel>.NotFound($"Todo with ID {todoId} not found");

                bool canView = false;

                if (todo.CreatedByUserId == currentUserId)
                {
                    canView = true;
                }

                if (!canView)
                {
                    if (todo.CurrentReviewerUserId == currentUserId)
                    {
                        canView = true;
                    }
                }

                if (!canView)
                    return ResultDto<TodoViewModel>.Forbidden("Access denied: insufficient view permissions");

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserId = todo.CreatedByUserId,
                    CurrentReviewerUserId = todo.CurrentReviewerUserId,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error retrieving todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> UpdateTodoAsync(UpdateTodoRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.CurrentUserId);
                if (user == null)
                    return ResultDto<TodoViewModel>.NotFound($"User with ID {request.CurrentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(request.TodoId);
                if (todo == null)
                    return ResultDto<TodoViewModel>.NotFound($"Todo with ID {request.TodoId} not found");

                var userRoles = await GetUserRolesAsync(request.CurrentUserId);
                if (userRoles.Count == 0)
                    return ResultDto<TodoViewModel>.Forbidden("User does not have any roles assigned");

                if (todo.CreatedByUserId != request.CurrentUserId)
                    return ResultDto<TodoViewModel>.Forbidden("Access denied: only creator can update todo");

                if (todo.Status != ReviewAction.Pending && todo.Status != "draft")
                {
                    return ResultDto<TodoViewModel>.BadRequest("Todo cannot be updated in its current status");
                }

                if (todo.Title != request.Title)
                {
                    var updateReview = new Reviews
                    {
                        TodoId = todo.TodoListId,
                        ReviewerUserId = request.CurrentUserId,
                        ReviewLevel = 0,
                        Action = "update",
                        Comment = $"Title update: {todo.Title} → {request.Title}",
                        PreviousStatus = todo.Status,
                        NewStatus = todo.Status,
                        NextReviewerUserId = todo.CurrentReviewerUserId,
                        ReviewedAt = DateTime.UtcNow
                    };

                    await _reviewRepository.AddAsync(updateReview);
                }

                todo.Title = request.Title;
                await _todoRepository.UpdateAsync(todo);

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserId = todo.CreatedByUserId,
                    CurrentReviewerUserId = todo.CurrentReviewerUserId,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error updating todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto> DeleteTodoAsync(int todoId, int currentUserId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return ResultDto.NotFound($"User with ID {currentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto.NotFound($"Todo with ID {todoId} not found");

                var userRoles = await GetUserRolesAsync(currentUserId);
                if (userRoles.Count == 0)
                    return ResultDto.Forbidden("User does not have any roles assigned");

                if (todo.CreatedByUserId != currentUserId)
                    return ResultDto.Forbidden("Access denied: only creator can delete todo");

                if (todo.Status == ReviewAction.Approved || todo.Status == ReviewAction.Rejected)
                {
                    return ResultDto.BadRequest("Cannot delete a todo that has already been approved or rejected");
                }

                var deleteReview = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerUserId = currentUserId,
                    ReviewLevel = 0,
                    Action = "delete",
                    Comment = "The to-do list has been deleted.",
                    PreviousStatus = todo.Status,
                    NewStatus = "deleted",
                    NextReviewerUserId = todo.CurrentReviewerUserId,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(deleteReview);
                await _todoRepository.DeleteAsync(todo);

                return ResultDto.Success("Todo deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultDto.Failure($"Error deleting todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto> AssignReviewerAsync(int todoId, int reviewerUserId, int currentUserId)
        {
            try
            {
                var currentUser = await _userRepository.GetByIdAsync(currentUserId);
                var reviewer = await _userRepository.GetByIdAsync(reviewerUserId);

                if (currentUser == null)
                    return ResultDto.NotFound($"User with ID {currentUserId} not found");

                if (reviewer == null)
                    return ResultDto.NotFound($"Reviewer with ID {reviewerUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto.NotFound($"Todo with ID {todoId} not found");

                if (todo.CreatedByUserId != currentUserId)
                    return ResultDto.Forbidden("Access denied: only creator can assign reviewer");

                if (todo.Status == ReviewAction.Approved || todo.Status == ReviewAction.Rejected)
                {
                    return ResultDto.BadRequest("Cannot reassign reviewer for a todo that has already been approved or rejected");
                }

                var reviewerRoles = await GetUserRolesAsync(reviewerUserId);
                bool canReview = false;

                if (todo.Status == ReviewAction.Pending)
                {
                    canReview = await CheckPermissionAsync(
                        reviewerRoles,
                        Permission.TodoReviewLevel1.GetPermissionName());
                }
                else if (todo.Status == ReviewAction.InProgress)
                {
                    canReview = await CheckPermissionAsync(
                        reviewerRoles,
                        Permission.TodoReviewLevel2.GetPermissionName());
                }

                if (!canReview)
                    return ResultDto.BadRequest($"The assigned reviewer does not have permission to review this todo (current status: {todo.Status})");

                if (reviewerUserId == currentUserId)
                {
                    return ResultDto.BadRequest("Cannot assign yourself as reviewer");
                }

                if (todo.CurrentReviewerUserId == reviewerUserId)
                {
                    return ResultDto.BadRequest("The selected user is already the current reviewer");
                }

                var previousReviewerId = todo.CurrentReviewerUserId;
                todo.CurrentReviewerUserId = reviewerUserId;
                await _todoRepository.UpdateAsync(todo);

                var assignmentReview = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerUserId = currentUserId,
                    ReviewLevel = 0,
                    Action = "assign",
                    Comment = $"Reviewer assigned: {previousReviewerId} → {reviewerUserId}",
                    PreviousStatus = todo.Status,
                    NewStatus = todo.Status,
                    NextReviewerUserId = reviewerUserId,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(assignmentReview);

                return ResultDto.Success("Reviewer assigned successfully");
            }
            catch (Exception ex)
            {
                return ResultDto.Failure($"Error assigning reviewer: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> ReviewTodoAsync(int todoId, int currentUserId, string action, string comment)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return ResultDto<TodoViewModel>.NotFound($"User with ID {currentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<TodoViewModel>.NotFound($"Todo with ID {todoId} not found");

                if (todo.CurrentReviewerUserId != currentUserId)
                    return ResultDto<TodoViewModel>.Forbidden("Access denied: only the current reviewer can review this todo");

                var userRoles = await GetUserRolesAsync(currentUserId);
                if (userRoles.Count == 0)
                    return ResultDto<TodoViewModel>.Forbidden("User does not have any roles assigned");

                bool hasReviewPermission = false;
                if (todo.Status == ReviewAction.Pending)
                {
                    hasReviewPermission = await CheckPermissionAsync(
                        userRoles,
                        Permission.TodoReviewLevel1.GetPermissionName());
                }
                else if (todo.Status == ReviewAction.InProgress)
                {
                    hasReviewPermission = await CheckPermissionAsync(
                        userRoles,
                        Permission.TodoReviewLevel2.GetPermissionName());
                }
                else
                {
                    return ResultDto<TodoViewModel>.BadRequest($"Todo cannot be reviewed in its current status: {todo.Status}");
                }

                if (!hasReviewPermission)
                    return ResultDto<TodoViewModel>.Forbidden("User does not have permission to review this todo");

                string newStatus;
                if (action.ToLower() == "approve")
                {
                    if (todo.Status == ReviewAction.Pending)
                    {
                        newStatus = ReviewAction.InProgress;
                    }
                    else if (todo.Status == ReviewAction.InProgress)
                    {
                        newStatus = ReviewAction.Approved;
                    }
                    else
                    {
                        return ResultDto<TodoViewModel>.BadRequest($"Invalid approval for status: {todo.Status}");
                    }
                }
                else if (action.ToLower() == "reject")
                {
                    newStatus = ReviewAction.Rejected;
                }
                else
                {
                    return ResultDto<TodoViewModel>.BadRequest("Invalid action. Must be 'approve' or 'reject'");
                }

                var review = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerUserId = currentUserId,
                    ReviewLevel = todo.Status == ReviewAction.Pending ? 1 : 2,
                    Action = action.ToLower(),
                    Comment = comment,
                    PreviousStatus = todo.Status,
                    NewStatus = newStatus,
                    NextReviewerUserId = newStatus == ReviewAction.InProgress ? GetNextReviewerUserId(todo) : null,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(review);

                var previousStatus = todo.Status;
                todo.Status = newStatus;

                if (newStatus == ReviewAction.InProgress)
                {
                    todo.CurrentReviewerUserId = GetNextReviewerUserId(todo);
                }
                else if (newStatus == ReviewAction.Approved || newStatus == ReviewAction.Rejected)
                {
                    todo.CurrentReviewerUserId = null;
                }

                await _todoRepository.UpdateAsync(todo);

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByUserId = todo.CreatedByUserId,
                    CurrentReviewerUserId = todo.CurrentReviewerUserId,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error reviewing todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        private async Task<bool> CheckPermissionAsync(List<Roles> roles, string permissionName)
        {
            foreach (var role in roles)
            {
                var result = await _rbacService.HasPermissionAsync(role.RoleId, permissionName);
                if (result.IsSuccess && result.Data)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<List<Roles>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);

            var roles = new List<Roles>();
            foreach (var userRole in userRoles)
            {
                var role = await _roleRepository.GetByIdAsync(userRole.RoleId);
                if (role != null)
                {
                    roles.Add(role);
                }
            }

            return roles;
        }

        private int? GetNextReviewerUserId(TodoLists todo)
        {
            return null;
        }

        public async Task<ResultDto<List<ReviewHistoryViewModel>>> GetTodoReviewHistoryAsync(int todoId, int currentUserId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(currentUserId);
                if (user == null)
                    return ResultDto<List<ReviewHistoryViewModel>>.NotFound($"User with ID {currentUserId} not found");

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<List<ReviewHistoryViewModel>>.NotFound($"Todo with ID {todoId} not found");

                bool canView = false;

                if (todo.CreatedByUserId == currentUserId)
                {
                    canView = true;
                }

                if (!canView)
                {
                    var userReviews = await _reviewRepository.FindAsync(r =>
                        r.TodoId == todoId && r.ReviewerUserId == currentUserId);
                    if (userReviews.Any())
                    {
                        canView = true;
                    }
                }

                if (!canView && todo.CurrentReviewerUserId == currentUserId)
                {
                    canView = true;
                }

                if (!canView)
                    return ResultDto<List<ReviewHistoryViewModel>>.Forbidden("Access denied: insufficient permissions to view review history");

                var reviews = await _reviewRepository.FindAsync(r => r.TodoId == todoId);
                var reviewHistory = reviews
                    .OrderBy(r => r.ReviewedAt)
                    .Select(review => new ReviewHistoryViewModel
                    {
                        ReviewId = review.ReviewId,
                        ReviewerUserId = review.ReviewerUserId,
                        Action = review.Action,
                        Comment = review.Comment,
                        PreviousStatus = review.PreviousStatus,
                        NewStatus = review.NewStatus,
                        CreatedAt = review.ReviewedAt
                    })
                    .ToList();

                return ResultDto<List<ReviewHistoryViewModel>>.Success(reviewHistory);
            }
            catch (Exception ex)
            {
                return ResultDto<List<ReviewHistoryViewModel>>.Failure(
                    $"Error retrieving review history: {ex.Message}",
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }
    }
}