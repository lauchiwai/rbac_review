using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Common.DTOs.Transition.Requests;
using Common.DTOs.Transition.Response;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewTemplateController : ControllerBase
{
    private readonly IReviewTemplateService _reviewTemplateService;

    public ReviewTemplateController(IReviewTemplateService reviewTemplateService)
    {
        _reviewTemplateService = reviewTemplateService;
    }

    [HttpPost("InitializeTemplate")]
    public async Task<ActionResult<ResultDto<TemplateInitResponse>>> InitializeTemplate(
        [FromBody] TemplateInitRequest request)
    {
        var result = await _reviewTemplateService.InitializeReviewTemplateAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }

    [HttpPost("SetupTransitions")]
    public async Task<ActionResult<ResultDto<TransitionSetupResponse>>> SetupTransitions(
        [FromBody] TransitionSetupRequest request)
    {
        var result = await _reviewTemplateService.SetupStageTransitionsAsync(request);
        return StatusCode((int)result.StatusCode, result);
    }
}