using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;

namespace Services.Interfaces
{
    public interface IReviewTemplateService
    {
        Task<ResultDto<bool>> HasPermissionAsync(int userId, string permissionName);

        Task<ResultDto<TemplateResponse>> CreateLevel1ReviewAsync(CreateLevel1ReviewRequest request);

        Task<ResultDto<TemplateResponse>> CreateLevel2ReviewAsync(CreateLevel2ReviewRequest request);

        Task<ResultDto<TemplateResponse>> CreateLevel3ReviewAsync(CreateLevel3ReviewRequest request);
    }
}