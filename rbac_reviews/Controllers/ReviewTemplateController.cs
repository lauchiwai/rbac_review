using Common.DTOs;
using Common.DTOs.Template.Requests;
using Common.DTOs.Template.Response;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewTemplateController : ControllerBase
    {
        private readonly IReviewTemplateService _reviewTemplateService;

        public ReviewTemplateController(IReviewTemplateService reviewTemplateService)
        {
            _reviewTemplateService = reviewTemplateService;
        }

        [HttpPost("CreateLevel1Review")]
        public async Task<ActionResult<ResultDto<TemplateResponse>>> CreateLevel1Review(
            [FromBody] CreateLevel1ReviewRequest request)
        {
            var result = await _reviewTemplateService.CreateLevel1ReviewAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("CreateLevel2Review")]
        public async Task<ActionResult<ResultDto<TemplateResponse>>> CreateLevel2Review(
            [FromBody] CreateLevel2ReviewRequest request)
        {
            var result = await _reviewTemplateService.CreateLevel2ReviewAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }

        [HttpPost("CreateLevel3Review")]
        public async Task<ActionResult<ResultDto<TemplateResponse>>> CreateLevel3Review(
            [FromBody] CreateLevel3ReviewRequest request)
        {
            var result = await _reviewTemplateService.CreateLevel3ReviewAsync(request);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}