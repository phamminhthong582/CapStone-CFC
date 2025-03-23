using System.Net;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.Entities;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/flowers")]
[ApiController]

public class FlowerController : Controller
{
    private readonly IFlowerService _flowerService;

    public FlowerController(IFlowerService flowerService)
    {
        _flowerService = flowerService;
    }
    [HttpGet]
    public async Task<IActionResult> GetFlower()
    {
        var result = await _flowerService.GetAllFlower();
        return Ok(result);
    }
    [HttpGet("getFlower-pagination")]
    public async Task<IActionResult> GetFlowerPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _flowerService.GetAllFlowerPagination(pageNumber, pageSize);
        return Ok(result);
    }
    [HttpGet("Id")]
    public async Task<IActionResult> GetFlowerById(Guid id)
    {
        var result = await _flowerService.GetFlowerById(id);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [HttpGet("Name")]
    public async Task<IActionResult> GetFlowerByName(string name)
    {
        var result = await _flowerService.GetFlowerByName(name);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [HttpGet("filter-price")]
    public async Task<IActionResult> FilterFlowerByPrice(double minPrice, double? maxPrice)
    {
        var result = await _flowerService.GetFlowerByPrice(minPrice , maxPrice);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    
    [HttpPost("create-flower")]
    public async Task<IActionResult> CreateFlower([FromForm] CreateFlowerRequest request)
    {
        var result = await _flowerService.CreateFlower(request);
        return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: "Thêm sản phẩm mới thành công"));
    }
    // [Authorize(Roles = "Admin")]
    [HttpPut("{flowerId}")]
    public async Task<IActionResult> UpdateFlower(Guid flowerId, [FromForm]UpdateFlowerRequest request)
    {
        var result = await _flowerService.UpdateFlower(flowerId, request);
        return result.ResultStatus != ResultStatus.Success .ToString()? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }

    [HttpDelete("delete-flower")]
    public async Task<ActionResult<Result<Flower>>> DeleteFlower(Guid id)
    {
        var result = await _flowerService.DeleteFlower(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}