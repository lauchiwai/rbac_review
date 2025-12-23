using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;
using Common.Models;
using Repositories.MyRepository;
using Services.Helpers;
using Services.Interfaces;

namespace Services.Implementations
{
    public class ReviewTemplateService : IReviewTemplateService
    {
        private readonly IRepository<ReviewTemplates> _templateRepository;
        private readonly IRepository<ReviewStages> _stageRepository;
        private readonly IRepository<StageTransitions> _transitionRepository;
        private readonly IRepository<Users> _userRepository;
        private readonly ITodoQueryHelper _queryHelper;

        public ReviewTemplateService(
            IRepository<ReviewTemplates> templateRepository,
            IRepository<ReviewStages> stageRepository,
            IRepository<StageTransitions> transitionRepository,
            IRepository<Users> userRepository,
            ITodoQueryHelper todoQueryHelper)
        {
            _templateRepository = templateRepository;
            _stageRepository = stageRepository;
            _transitionRepository = transitionRepository;
            _userRepository = userRepository;
            _queryHelper = todoQueryHelper;
        }

        public async Task<ResultDto<bool>> HasPermissionAsync(int userId, string permissionName)
        {
            try
            {
                var hasPermission = await _queryHelper.CheckUserPermissionAsync(userId, permissionName);
                return ResultDto<bool>.Success(hasPermission);
            }
            catch (Exception ex)
            {
                return ResultDto<bool>.Failure($"Error occurred while checking permission: {ex.Message}");
            }
        }

        public async Task<ResultDto<TemplateResponse>> CreateLevel1ReviewAsync(CreateLevel1ReviewRequest request)
        {
            try
            {
                // 1. Check admin permission
                var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "admin_manage");
                if (!hasPermission)
                    return ResultDto<TemplateResponse>.Failure("User does not have admin permission (admin_manage)");

                // 2. Create template
                var template = new ReviewTemplates
                {
                    TemplateName = request.TemplateName ?? "Level 1 Review Process",
                    Description = request.Description ?? "Standard level 1 review process",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = request.UserId
                };

                await _templateRepository.AddAsync(template);

                // 3. Create level 1 review stage
                var stage = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 1 Review",
                    StageOrder = 1,
                    RequiredRoleId = 2, // Senior staff
                    SpecificReviewerUserId = null // Dynamically assigned by system
                };

                await _stageRepository.AddAsync(stage);

                // 4. Create transition rules
                var transitions = new List<StageTransitions>
                {
                    new StageTransitions
                    {
                        StageId = stage.StageId,
                        ActionName = "approve",
                        NextStageId = null, // No next stage (only one level)
                        ResultStatus = "approved"
                    },
                    new StageTransitions
                    {
                        StageId = stage.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_creator"
                    },
                    new StageTransitions
                    {
                        StageId = stage.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    }
                };

                foreach (var transition in transitions)
                {
                    await _transitionRepository.AddAsync(transition);
                }

                // 5. Return result
                var response = new TemplateResponse
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    Description = template.Description,
                    StageCount = 1,
                    CreatedAt = template.CreatedAt
                };

                return ResultDto<TemplateResponse>.Success(response, "Level 1 review process created successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<TemplateResponse>.Failure($"Error occurred while creating level 1 review process: {ex.Message}");
            }
        }

        public async Task<ResultDto<TemplateResponse>> CreateLevel2ReviewAsync(CreateLevel2ReviewRequest request)
        {
            try
            {
                // 1. Check admin permission
                var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "admin_manage");
                if (!hasPermission)
                    return ResultDto<TemplateResponse>.Failure("User does not have admin permission (admin_manage)");

                // 2. Check if level 1 reviewer exists
                if (request.Level1ReviewerId.HasValue)
                {
                    var reviewer = await _userRepository.GetByIdAsync(request.Level1ReviewerId.Value);
                    if (reviewer == null)
                        return ResultDto<TemplateResponse>.Failure($"Level 1 reviewer ID {request.Level1ReviewerId} does not exist");
                }

                // 3. Create template
                var template = new ReviewTemplates
                {
                    TemplateName = request.TemplateName ?? "Level 2 Review Process",
                    Description = request.Description ?? "Standard level 2 review process, including level 1 and level 2 review",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = request.UserId
                };

                await _templateRepository.AddAsync(template);

                // 4. Create two review stages
                var stage1 = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 1 Review",
                    StageOrder = 1,
                    RequiredRoleId = 2, // Senior staff
                    SpecificReviewerUserId = request.Level1ReviewerId
                };

                var stage2 = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 2 Review",
                    StageOrder = 2,
                    RequiredRoleId = 3, // Manager
                    SpecificReviewerUserId = null // Dynamically assigned by system
                };

                await _stageRepository.AddAsync(stage1);
                await _stageRepository.AddAsync(stage2);

                // 5. Create transition rules
                var transitions = new List<StageTransitions>
                {
                    // Level 1 review stage transition rules
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "approve",
                        NextStageId = stage2.StageId,
                        ResultStatus = "pending_review_level2"
                    },
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_creator"
                    },
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    },

                    // Level 2 review stage transition rules
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "approve",
                        NextStageId = null,
                        ResultStatus = "approved"
                    },
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_level1"
                    },
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    }
                };

                foreach (var transition in transitions)
                {
                    await _transitionRepository.AddAsync(transition);
                }

                // 6. Return result
                var response = new TemplateResponse
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    Description = template.Description,
                    StageCount = 2,
                    CreatedAt = template.CreatedAt
                };

                return ResultDto<TemplateResponse>.Success(response, "Level 2 review process created successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<TemplateResponse>.Failure($"Error occurred while creating level 2 review process: {ex.Message}");
            }
        }

        public async Task<ResultDto<TemplateResponse>> CreateLevel3ReviewAsync(CreateLevel3ReviewRequest request)
        {
            try
            {
                // 1. Check admin permission
                var hasPermission = await _queryHelper.CheckUserPermissionAsync(request.UserId, "admin_manage");
                if (!hasPermission)
                    return ResultDto<TemplateResponse>.Failure("User does not have admin permission (admin_manage)");

                // 2. Check if reviewers exist
                if (request.Level1ReviewerId.HasValue)
                {
                    var reviewer1 = await _userRepository.GetByIdAsync(request.Level1ReviewerId.Value);
                    if (reviewer1 == null)
                        return ResultDto<TemplateResponse>.Failure($"Level 1 reviewer ID {request.Level1ReviewerId} does not exist");
                }

                if (request.Level2ReviewerId.HasValue)
                {
                    var reviewer2 = await _userRepository.GetByIdAsync(request.Level2ReviewerId.Value);
                    if (reviewer2 == null)
                        return ResultDto<TemplateResponse>.Failure($"Level 2 reviewer ID {request.Level2ReviewerId} does not exist");
                }

                // 3. Create template
                var template = new ReviewTemplates
                {
                    TemplateName = request.TemplateName ?? "Level 3 Review Process",
                    Description = request.Description ?? "Standard level 3 review process, including level 1, level 2, and level 3 review",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = request.UserId
                };

                await _templateRepository.AddAsync(template);

                // 4. Create three review stages
                var stage1 = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 1 Review",
                    StageOrder = 1,
                    RequiredRoleId = 2, // Senior staff
                    SpecificReviewerUserId = request.Level1ReviewerId
                };

                var stage2 = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 2 Review",
                    StageOrder = 2,
                    RequiredRoleId = 3, // Manager
                    SpecificReviewerUserId = request.Level2ReviewerId
                };

                var stage3 = new ReviewStages
                {
                    TemplateId = template.TemplateId,
                    StageName = "Level 3 Review",
                    StageOrder = 3,
                    RequiredRoleId = 4, // Administrator
                    SpecificReviewerUserId = null // Dynamically assigned by system
                };

                await _stageRepository.AddAsync(stage1);
                await _stageRepository.AddAsync(stage2);
                await _stageRepository.AddAsync(stage3);

                // 5. Create transition rules
                var transitions = new List<StageTransitions>
                {
                    // Level 1 review stage transition rules
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "approve",
                        NextStageId = stage2.StageId,
                        ResultStatus = "pending_review_level2"
                    },
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_creator"
                    },
                    new StageTransitions
                    {
                        StageId = stage1.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    },

                    // Level 2 review stage transition rules
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "approve",
                        NextStageId = stage3.StageId,
                        ResultStatus = "pending_review_level3"
                    },
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_level1"
                    },
                    new StageTransitions
                    {
                        StageId = stage2.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    },

                    // Level 3 review stage transition rules
                    new StageTransitions
                    {
                        StageId = stage3.StageId,
                        ActionName = "approve",
                        NextStageId = null,
                        ResultStatus = "approved"
                    },
                    new StageTransitions
                    {
                        StageId = stage3.StageId,
                        ActionName = "return",
                        NextStageId = null,
                        ResultStatus = "returned_to_level2"
                    },
                    new StageTransitions
                    {
                        StageId = stage3.StageId,
                        ActionName = "reject",
                        NextStageId = null,
                        ResultStatus = "rejected"
                    }
                };

                foreach (var transition in transitions)
                {
                    await _transitionRepository.AddAsync(transition);
                }

                // 6. Return result
                var response = new TemplateResponse
                {
                    TemplateId = template.TemplateId,
                    TemplateName = template.TemplateName,
                    Description = template.Description,
                    StageCount = 3,
                    CreatedAt = template.CreatedAt
                };

                return ResultDto<TemplateResponse>.Success(response, "Level 3 review process created successfully");
            }
            catch (Exception ex)
            {
                return ResultDto<TemplateResponse>.Failure($"Error occurred while creating level 3 review process: {ex.Message}");
            }
        }
    }
}