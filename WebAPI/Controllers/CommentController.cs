using System.Net;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[ApiController]
[Route("api/comment")]
public class CommentController : Controller
{
   private readonly ICommentService _commentService;

   public CommentController(ICommentService commentService)
   {
      _commentService = commentService;
   }
   [HttpGet]
   public async Task<IActionResult> GetComments()
   {
      var result = await _commentService.GetAllComment();
      return Ok(result);
   }
   [HttpGet("getComment-pagination")]
   public async Task<IActionResult> GetCommentPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
   {
      var result = await _commentService.GetAllCommentPagination(pageNumber, pageSize);
      return Ok(result);
   }
   [HttpGet("get-comments-by-productId")]
   public async Task<IActionResult> GetCommentsByProductId([FromQuery] Guid productId)
   {
      if (productId == Guid.Empty)
      {
         return BadRequest("Product ID is required.");
      }

      var comments = await _commentService.GetCommentByProductId(productId);

      if (comments == null || comments.Count == 0)
      {
         return NotFound("No comments found for this product.");
      }

      return Ok(new { data = comments });
   }
   [HttpPost("create-comment")]
   public async Task<ActionResult<Result<Comment>>> CreateComment( [FromBody] CreateCommentRequest request)
   {
      var result = await _commentService.CreateComment(request);
      if (result.ResultStatus != ResultStatus.Success.ToString())
      {
         return StatusCode((int)HttpStatusCode.InternalServerError, result);
      }
      return Ok(result);
   }
}