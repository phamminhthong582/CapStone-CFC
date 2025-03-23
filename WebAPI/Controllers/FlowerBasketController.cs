using System.Net;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Repository.Interface;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/flowerBaskets")]
[ApiController]

public class FlowerBasketController : Controller
{
    private readonly IFlowerBasketService _flowerBasketService;

    public FlowerBasketController(IFlowerBasketService flowerBasketService)
    {
        _flowerBasketService = flowerBasketService;
    }
    [HttpGet]
    public async Task<IActionResult> GetFlowerBasket()
    {
        var result = await _flowerBasketService.GetAllFlowerBasket();
        return Ok(result);
    }
    [HttpGet("Id")]
    public async Task<IActionResult> GetFlowerBasketById(Guid id)
    {
        var result = await _flowerBasketService.GetFlowerBasketById(id);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        return Ok(result);
    }
    [HttpPost("create-flowerbasket")]
    public async Task<ActionResult<Result<FlowerBasket>>> CreateFlowerBasket([FromForm] CreateFlowerBasketRequest request)
    {
        var result = await _flowerBasketService.CreateFlowerBasket(request);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
    // [Authorize(Roles = "Admin")]
    [HttpPut("{flowerbasketId}")]
    public async Task<IActionResult> UpdateFlowerBasket([FromRoute] Guid flowerbasketId, [FromForm] UpdateFlowerBasketRequest request)
    {
        var result = await _flowerBasketService.UpdateFlowerBasket(flowerbasketId, request);
        return result.ResultStatus != ResultStatus.Success.ToString() ? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }

    [HttpDelete("delete-flowerbasket")]
    public async Task<ActionResult<Result<Flower>>> DeleteFlowerasket(Guid id)
    {
        var result = await _flowerBasketService.DeleteFlowerBasket(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
    [HttpGet("getFlowerBasket-pagination")]
    public async Task<IActionResult> GetFlowerBasketPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _flowerBasketService.GetAllFlowerBasketPagination(pageNumber, pageSize);
        return Ok(result);
    }
}