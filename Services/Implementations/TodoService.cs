using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Todo.Requests;
using Common.DTOs.Todo.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations;

public class TodoService : ITodoService
{
    private readonly IRepository<TodoLists> _todoRepository;
    private readonly IRepository<ReviewTemplates> _templateRepository;
    private readonly IRepository<ReviewStages> _stageRepository;
    private readonly IRepository<StageTransitions> _transitionRepository;

    private readonly TodoQueryHelper _queryHelper;

    public TodoService(
        IRepository<TodoLists> todoRepository,
        IRepository<Users> userRepository,
        IRepository<Roles> roleRepository,
        IRepository<Permissions> permissionRepository,
        IRepository<ReviewTemplates> templateRepository,
        IRepository<ReviewStages> stageRepository,
        IRepository<StageTransitions> transitionRepository,
        IRepository<Users_Roles> userRoleRepository,
        IRepository<Roles_Permissions> rolePermissionRepository)
    {
        _todoRepository = todoRepository;
        _templateRepository = templateRepository;
        _stageRepository = stageRepository;
        _transitionRepository = transitionRepository;

        _queryHelper = new TodoQueryHelper(
            userRoleRepository,
            rolePermissionRepository,
            permissionRepository,
            templateRepository,
            stageRepository,
            transitionRepository,
            userRepository,
            roleRepository);
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

            // 4. Create todo item
            var todo = new TodoLists
            {
                Title = request.Title,
                TemplateId = request.TemplateId,
                CurrentStageId = firstStage.StageId,
                Status = "pending_review_level1",
                CreatedByUserId = request.UserId,
                CurrentReviewerUserId = firstStage.SpecificReviewerUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _todoRepository.AddAsync(todo);

            // 5. Get stage and reviewer information
            var stageName = firstStage.StageName;
            var reviewerName = $"User{firstStage.SpecificReviewerUserId}";

            // 6. Return result
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
}