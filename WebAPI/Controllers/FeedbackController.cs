using System.Net;
using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;
using BusinessObject.DTO.Order;
using BusinessObject.Entities;
using Core.Infrastructures;
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
    [HttpPost("create-feedback")]
    public async Task<ActionResult<Result<Feedback>>> CreateFeedback(Guid customerId, Guid orderId, [FromForm] CreateFeedbackRequest request)
    {
        await _feedbackService.CreateFeedbackByCustomer(customerId, orderId,request);
        return Ok(new BaseResponseModel<string>(
                 statusCode: StatusCodes.Status200OK,
                 code: ResponseCodeConstants.SUCCESS,
                 data: "Thêm sản phẩm mới thành công"));
    }
    [HttpGet("GetFeedBackByOrderId")]

    public async Task<IActionResult> GetFeedBackByOrderId(Guid OrderId)
    {
        var result = await _feedbackService.GetFeedBackByOrderId(OrderId);
        return Ok(result);
    }

    [HttpGet("CheckFeedBack")]

    public async Task<IActionResult> CheckFeedBack(Guid OrderId)
    {
        var result = await _feedbackService.CheckFeedBack(OrderId);
        return Ok(result);
    }
}