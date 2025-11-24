using Common.DTOs;
using Common.DTOs.TodoLists.Requests;
using Common.DTOs.TodoLists.Responses;
using Common.Enums;
using Common.Models;
using Repositories.MyRepository;
using Services.Interfaces;
using System.Linq.Expressions;
using System.Net;

namespace Services.Implementations
{
    public class TodoListsService : ITodoListsService
    {
        private readonly IRepository<TodoLists> _todoRepository;
        private readonly IRepository<Reviews> _reviewRepository;
        private readonly IRbacService _rbacService;
        private readonly IRolesService _roleService;

        public TodoListsService(
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

        public async Task<ResultDto<TodoViewModel>> CreateTodoAsync(CreateTodoRequest request)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(request.CreatedByRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<TodoViewModel>.BadRequest(roleValidation.ErrorMessage);

                var hasPermission = await _rbacService.HasPermissionAsync(request.CreatedByRoleId, Permission.TodoCreate.GetPermissionName());
                if (!hasPermission.Data)
                    return ResultDto<TodoViewModel>.Forbidden($"User does not have {Permission.TodoCreate.GetDescription()}");

                var todo = new TodoLists
                {
                    Title = request.Title,
                    Status = ReviewAction.Pending,
                    CreatedByRole = request.CreatedByRoleId,
                    CreatedAt = DateTime.UtcNow
                };

                await _todoRepository.AddAsync(todo);

                var initialReview = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerRole = request.CreatedByRoleId,
                    ReviewLevel = 0,
                    Action = "create",
                    Comment = request.Title,
                    PreviousStatus = null,
                    NewStatus = ReviewAction.Pending,
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.AddAsync(initialReview);

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByRoleId = todo.CreatedByRole,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response, statusCode: HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error creating todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> GetTodoAsync(int todoId, int currentUserRoleId)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(currentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<TodoViewModel>.BadRequest(roleValidation.ErrorMessage);

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto<TodoViewModel>.NotFound($"Todo with ID {todoId} not found");

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
                    return ResultDto<TodoViewModel>.Forbidden("Access denied: insufficient view permissions");

                var response = new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByRoleId = todo.CreatedByRole,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error retrieving todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<List<TodoViewModel>>> GetAllTodosAsync(GetTodosRequest request)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(request.CurrentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<List<TodoViewModel>>.BadRequest(roleValidation.ErrorMessage);

                var hasViewOwnPermission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewOwn.GetPermissionName());
                var hasViewLevel1Permission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewLevel1.GetPermissionName());
                var hasViewLevel2Permission = await _rbacService.HasPermissionAsync(request.CurrentUserRoleId, Permission.TodoViewLevel2.GetPermissionName());

                if (!hasViewOwnPermission.Data && !hasViewLevel1Permission.Data && !hasViewLevel2Permission.Data)
                    return ResultDto<List<TodoViewModel>>.Forbidden("User does not have view permissions");

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
                });

                var response = filteredTodos.Select(todo => new TodoViewModel
                {
                    Id = todo.TodoListId,
                    Title = todo.Title,
                    Status = todo.Status,
                    CreatedByRoleId = todo.CreatedByRole,
                    CreatedAt = todo.CreatedAt
                }).ToList();

                return ResultDto<List<TodoViewModel>>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<List<TodoViewModel>>.Failure($"Error retrieving todos: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto<TodoViewModel>> UpdateTodoAsync(UpdateTodoRequest request)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(request.CurrentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto<TodoViewModel>.BadRequest(roleValidation.ErrorMessage);

                var todo = await _todoRepository.GetByIdAsync(request.TodoId);
                if (todo == null)
                    return ResultDto<TodoViewModel>.NotFound($"Todo with ID {request.TodoId} not found");

                if (todo.CreatedByRole != request.CurrentUserRoleId)
                    return ResultDto<TodoViewModel>.Forbidden("Access denied: only creator can update todo");

                if (todo.Title != request.Title)
                {
                    var updateReview = new Reviews
                    {
                        TodoId = todo.TodoListId,
                        ReviewerRole = request.CurrentUserRoleId,
                        ReviewLevel = 0,
                        Action = "update",
                        Comment = $"Tittle update: {todo.Title} → {request.Title}",
                        PreviousStatus = todo.Status,
                        NewStatus = todo.Status,
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
                    CreatedByRoleId = todo.CreatedByRole,
                    CreatedAt = todo.CreatedAt
                };

                return ResultDto<TodoViewModel>.Success(response);
            }
            catch (Exception ex)
            {
                return ResultDto<TodoViewModel>.Failure($"Error updating todo: {ex.Message}", statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ResultDto> DeleteTodoAsync(int todoId, int currentUserRoleId)
        {
            try
            {
                var roleValidation = await ValidateRoleExistsAsync(currentUserRoleId);
                if (!roleValidation.IsSuccess)
                    return ResultDto.BadRequest(roleValidation.ErrorMessage);

                var todo = await _todoRepository.GetByIdAsync(todoId);
                if (todo == null)
                    return ResultDto.NotFound($"Todo with ID {todoId} not found");

                if (todo.CreatedByRole != currentUserRoleId)
                    return ResultDto.Forbidden("Access denied: only creator can delete todo");

                var deleteReview = new Reviews
                {
                    TodoId = todo.TodoListId,
                    ReviewerRole = currentUserRoleId,
                    ReviewLevel = 0,
                    Action = "delete",
                    Comment = "The to-do list has been deleted.",
                    PreviousStatus = todo.Status,
                    NewStatus = "deleted",
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
