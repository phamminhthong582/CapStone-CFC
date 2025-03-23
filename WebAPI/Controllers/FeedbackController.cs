using System.Net;
using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[ApiController]
[Route("api/feedback")]
public class FeedbackController : Controller
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }
    [HttpGet]
    public async Task<IActionResult> GetFeedbacks()
    {
        var result = await _feedbackService.GetAllFeedback();
        return Ok(result);
    }
    [HttpGet("getFeedback-pagination")]
    public async Task<IActionResult> GetFeedbackPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _feedbackService.GetAllFeedbackPagination(pageNumber, pageSize);
        return Ok(result);
    }
    [HttpPost("create-feedback")]
    public async Task<ActionResult<Result<Feedback>>> CreateFeedback( [FromBody] CreateFeedbackRequest request)
    {
        var result = await _feedbackService.CreateFeedback(request);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}