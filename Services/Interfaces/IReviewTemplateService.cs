using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;
using Common.DTOs;

namespace Services.Interfaces;

public interface IReviewTemplateService
{
    Task<ResultDto<TemplateInitResponse>> InitializeReviewTemplateAsync(TemplateInitRequest request);
    Task<ResultDto<TransitionSetupResponse>> SetupStageTransitionsAsync(TransitionSetupRequest request);
    Task<ResultDto<bool>> HasPermissionAsync(int userId, string permissionName);
}