using Common.DTOs;
using Common.DTOs.Todo.Requests;
using Common.DTOs.Todo.Response;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations;

public class TodoService : ITodoService
{
    private readonly IRepository<TodoLists> _todoRepository;
    private readonly IRepository<Users> _userRepository;
    private readonly IRepository<Users_Roles> _userRoleRepository;
    private readonly ITodoQueryHelper _queryHelper;
    private readonly IReviewQueryHelper _reviewQueryHelper;

    public TodoService(
        IRepository<TodoLists> todoRepository,
        IRepository<Users> userRepository,
        IRepository<Users_Roles> userRoleRepository,
        ITodoQueryHelper todoQueryHelper,
        IReviewQueryHelper reviewQueryHelper)
    {
        _todoRepository = todoRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _queryHelper = todoQueryHelper;
        _reviewQueryHelper = reviewQueryHelper;
    }

    public async Task<ResultDto<bool>> HasPermissionAsync(int userId, string permissionName)
    {
        try
        {
            // Use helper class for batch permission checking
            var hasPermission = await _queryHelper.CheckUserPermissionAsync(userId, permissionName);
            return ResultDto<bool>.Success(hasPermission);
        }
        catch (Exception ex)
        {
            return ResultDto<bool>.Failure($"Error occurred while checking permission: {ex.Message}");
        }
    }

    public async Task<ResultDto<TodoCreateResponse>> CreateTodoAsync(TodoCreateRequest request)
    {
        try
        {
            // 1. Check creation permission - using optimized method
            var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "todo_create");
            if (!hasPermission)
                return ResultDto<TodoCreateResponse>.Failure("User does not have permission to create todo items (todo_create)");

            // 2. Batch check if template exists and is active
            var templatesDict = await _queryHelper.GetTemplatesByIdsAsync(new[] { request.TemplateId });
            if (!templatesDict.TryGetValue(request.TemplateId, out var template) || !template.IsActive)
                return ResultDto<TodoCreateResponse>.Failure($"Review template ID {request.TemplateId} does not exist or is disabled");

            // 3. Batch get first stage
            var firstStagesDict = await _queryHelper.GetFirstStagesByTemplateIdsAsync(new[] { request.TemplateId });
            if (!firstStagesDict.TryGetValue(request.TemplateId, out var firstStage))
                return ResultDto<TodoCreateResponse>.Failure("Review template does not have any stages configured");

            // 4. Determine reviewer according to priority order
            int? reviewerUserId = await DetermineReviewerAsync(request, firstStage);

            if (reviewerUserId == null)
            {
                return ResultDto<TodoCreateResponse>.Failure("Unable to determine a reviewer for this todo");
            }

            // 5. Check if reviewer exists
            var reviewer = await _userRepository.GetByIdAsync(reviewerUserId.Value);
            if (reviewer == null)
            {
                return ResultDto<TodoCreateResponse>.Failure($"Reviewer user ID {reviewerUserId} does not exist");
            }

            // 6. Check if reviewer has the required role
            var reviewerRoles = await _reviewQueryHelper.GetUserRolesAsync(reviewerUserId.Value);
            if (!reviewerRoles.Contains(firstStage.RequiredRoleId))
            {
                return ResultDto<TodoCreateResponse>.Failure($"Reviewer does not have the required role (RoleId: {firstStage.RequiredRoleId})");
            }

            // 7. Create todo item
            var todo = new TodoLists
            {
                Title = request.Title,
                TemplateId = request.TemplateId,
                CurrentStageId = firstStage.StageId,
                Status = "pending_review_level1",
                CreatedByUserId = request.UserId,
                CurrentReviewerUserId = reviewerUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _todoRepository.AddAsync(todo);

            // 8. Get stage and reviewer information
            var stageName = firstStage.StageName;
            var reviewerName = $"User{reviewerUserId}";

            // 9. Return result
            var response = new TodoCreateResponse
            {
                TodoListId = todo.TodoListId,
                Title = todo.Title,
                Status = todo.Status,
                CurrentStageId = todo.CurrentStageId,
                CurrentReviewerUserId = todo.CurrentReviewerUserId,
                CreatedAt = todo.CreatedAt,
                CurrentStageName = stageName,
                CurrentReviewerName = reviewerName
            };

            return ResultDto<TodoCreateResponse>.Success(response, "Todo item created successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<TodoCreateResponse>.Failure($"Error occurred while creating todo item: {ex.Message}");
        }
    }

    private async Task<int?> DetermineReviewerAsync(TodoCreateRequest request, ReviewStages firstStage)
    {
        // 1. Priority: use requested reviewer
        if (request.CurrentReviewerUserId.HasValue)
        {
            return request.CurrentReviewerUserId.Value;
        }

        // 2. Use stage-specific reviewer
        if (firstStage.SpecificReviewerUserId.HasValue)
        {
            return firstStage.SpecificReviewerUserId.Value;
        }

        // 3. Dynamic allocation based on role
        return await FindReviewerByRoleAsync(firstStage.RequiredRoleId, request.UserId);
    }

    private async Task<int?> FindReviewerByRoleAsync(int requiredRoleId, int excludeUserId)
    {
        try
        {
            // Find all users with this role
            var userRoles = await _userRoleRepository.FindAsync(
                ur => ur.RoleId == requiredRoleId && ur.UserId != excludeUserId);

            if (!userRoles.Any())
                return null;

            var userIds = userRoles.Select(ur => ur.UserId).Distinct().ToList();

            // Simple load balancing strategy: find reviewer with fewest pending reviews
            var reviewersWithWorkload = new List<(int UserId, int Workload)>();

            foreach (var userId in userIds)
            {
                // Calculate how many pending reviews this reviewer currently has
                var pendingCount = await GetPendingReviewCountAsync(userId);
                reviewersWithWorkload.Add((userId, pendingCount));
            }

            // Select reviewer with lowest workload
            var selectedReviewer = reviewersWithWorkload
                .OrderBy(r => r.Workload)
                .ThenBy(r => r.UserId)
                .FirstOrDefault();

            return selectedReviewer.UserId;
        }
        catch (Exception)
        {
            // If error, return first user with this role
            var userRoles = await _userRoleRepository.FindAsync(
                ur => ur.RoleId == requiredRoleId && ur.UserId != excludeUserId);

            return userRoles.FirstOrDefault()?.UserId;
        }
    }

    private async Task<int> GetPendingReviewCountAsync(int reviewerId)
    {
        try
        {
            // Query how many pending todo items this reviewer has
            var pendingTodos = await _todoRepository.FindAsync(t =>
                t.CurrentReviewerUserId == reviewerId &&
                t.Status.StartsWith("pending_review"));

            return pendingTodos.Count();
        }
        catch
        {
            return 0;
        }
    }
}