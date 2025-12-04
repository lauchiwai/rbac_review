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

    public async Task<ResultDto<TemplateInitResponse>> InitializeReviewTemplateAsync(TemplateInitRequest request)
    {
        try
        {
            // 1. Check admin permission
            var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "admin_manage");
            if (!hasPermission)
                return ResultDto<TemplateInitResponse>.Failure("User does not have admin permission (admin_manage)");

            // 2. Validate input data
            if (string.IsNullOrWhiteSpace(request.TemplateName))
                return ResultDto<TemplateInitResponse>.Failure("Template name cannot be empty");

            if (request.Stages == null || !request.Stages.Any())
                return ResultDto<TemplateInitResponse>.Failure("At least one review stage is required");

            // 3. Check stage order sequence
            var stageOrders = request.Stages.Select(s => s.StageOrder).ToList();
            if (!_queryHelper.ValidateStageOrders(stageOrders))
                return ResultDto<TemplateInitResponse>.Failure("Stage order must be consecutive from 1 to N");

            // 4. Batch validate reviewers and roles
            // Only check non-null reviewers
            var reviewerIds = request.Stages
                .Where(s => s.SpecificReviewerUserId.HasValue)
                .Select(s => s.SpecificReviewerUserId.Value)
                .Distinct()
                .ToList();

            var roleIds = request.Stages.Select(s => s.RequiredRoleId).Distinct().ToList();

            // Batch check if users exist (only check non-null users)
            if (reviewerIds.Any())
            {
                var usersExist = await _queryHelper.CheckUsersExistAsync(reviewerIds);
                var missingUsers = usersExist.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();
                if (missingUsers.Any())
                    return ResultDto<TemplateInitResponse>.Failure($"Reviewer ID {string.Join(", ", missingUsers)} does not exist");
            }

            // Batch check if roles exist
            var rolesExist = await _queryHelper.CheckRolesExistAsync(roleIds);
            var missingRoles = rolesExist.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();
            if (missingRoles.Any())
                return ResultDto<TemplateInitResponse>.Failure($"Required role ID {string.Join(", ", missingRoles)} does not exist");

            // 5. Create template
            var template = new ReviewTemplates
            {
                TemplateName = request.TemplateName,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.UserId
            };

            await _templateRepository.AddAsync(template);

            // 6. Batch create stages
            var stages = new List<ReviewStages>();
            foreach (var stageRequest in request.Stages)
            {
                var stage = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = stageRequest.StageName,
                    StageOrder = stageRequest.StageOrder,
                    RequiredRoleId = stageRequest.RequiredRoleId,
                    SpecificReviewerUserId = stageRequest.SpecificReviewerUserId  // Can be null
                };
                stages.Add(stage);
            }

            // Batch add stages
            foreach (var stage in stages)
            {
                await _stageRepository.AddAsync(stage);
            }

            // 7. Return result
            var response = new TemplateInitResponse
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                CreatedAt = template.CreatedAt,
                StageCount = request.Stages.Count
            };

            return ResultDto<TemplateInitResponse>.Success(response, "Review template initialized successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<TemplateInitResponse>.Failure($"Error occurred while initializing review template: {ex.Message}");
        }
    }

    public async Task<ResultDto<TransitionSetupResponse>> SetupStageTransitionsAsync(TransitionSetupRequest request)
    {
        try
        {
            // 1. Check admin permission - using optimized method
            var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "admin_manage");
            if (!hasPermission)
                return ResultDto<TransitionSetupResponse>.Failure("User does not have admin permission (admin_manage)");

            // 2. Batch check if template exists
            var templatesDict = await _queryHelper.GetTemplatesByIdsAsync(new[] { request.TemplateId });
            if (!templatesDict.TryGetValue(request.TemplateId, out var template))
                return ResultDto<TransitionSetupResponse>.Failure($"Review template ID {request.TemplateId} does not exist");

            // 3. Batch get all stages of the template
            var stagesByTemplate = await _queryHelper.GetStagesByTemplateIdsAsync(new[] { request.TemplateId });
            if (!stagesByTemplate.TryGetValue(request.TemplateId, out var stages) || !stages.Any())
                return ResultDto<TransitionSetupResponse>.Failure($"Template ID {request.TemplateId} does not have any stages");

            var stageIds = stages.Select(s => s.StageId).ToHashSet();

            // 4. Batch validate transition rules
            var existingTransitionsDict = await _queryHelper.GetTransitionsByTemplateIdsAsync(new[] { request.TemplateId });
            var existingTransitions = existingTransitionsDict.TryGetValue(request.TemplateId, out var transitions)
                ? transitions
                : new List<StageTransitions>();

            // Create cache of existing transition rules
            var existingRulesCache = existingTransitions
                .GroupBy(t => t.StageId)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(t => t.ActionName)));

            foreach (var rule in request.TransitionRules)
            {
                // Check if stage belongs to the template
                if (!stageIds.Contains(rule.StageId))
                    return ResultDto<TransitionSetupResponse>.Failure($"Stage ID {rule.StageId} does not belong to template ID {request.TemplateId}");

                // Check if next stage exists (if specified)
                if (rule.NextStageId.HasValue && !stageIds.Contains(rule.NextStageId.Value))
                    return ResultDto<TransitionSetupResponse>.Failure($"Next stage ID {rule.NextStageId} does not belong to template ID {request.TemplateId}");

                // Batch check if same action name already exists
                if (existingRulesCache.TryGetValue(rule.StageId, out var existingActions) &&
                    existingActions.Contains(rule.ActionName))
                {
                    return ResultDto<TransitionSetupResponse>.Failure($"Stage ID {rule.StageId} already has a transition rule with action name '{rule.ActionName}'");
                }
            }

            // 5. Batch create transition rules
            int addedCount = 0;
            var transitionsToAdd = new List<StageTransitions>();

            foreach (var rule in request.TransitionRules)
            {
                var transition = new StageTransitions
                {
                    StageId = rule.StageId,
                    ActionName = rule.ActionName,
                    NextStageId = rule.NextStageId,
                    ResultStatus = rule.ResultStatus
                };
                transitionsToAdd.Add(transition);
            }

            // Batch add transition rules
            foreach (var transition in transitionsToAdd)
            {
                await _transitionRepository.AddAsync(transition);
                addedCount++;
            }

            // 6. Return result
            var response = new TransitionSetupResponse
            {
                TransitionsAdded = addedCount,
                TemplateId = request.TemplateId
            };

            return ResultDto<TransitionSetupResponse>.Success(response, "Stage transition rules set up successfully");
        }
        catch (Exception ex)
        {
            return ResultDto<TransitionSetupResponse>.Failure($"Error occurred while setting up stage transition rules: {ex.Message}");
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