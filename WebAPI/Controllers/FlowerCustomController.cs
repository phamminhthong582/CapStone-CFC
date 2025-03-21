using System.Net;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/flowerCustoms")]
[ApiController]
public class FlowerCustomController : Controller
{
    private readonly IFlowerCustomService _flowerCustomService;

    public FlowerCustomController(IFlowerCustomService flowerCustomService)
    {
        _flowerCustomService = flowerCustomService;
    }
    [HttpGet]
    public async Task<IActionResult> GetFlowerCustom([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _flowerCustomService.GetAllFlowerCustom(pageNumber, pageSize);
        return Ok(result);
    }
    [HttpGet("Id")]
    public async Task<IActionResult> GetFlowerCustomById(Guid id)
    {
        var result = await _flowerCustomService.GetFlowerCustomById(id);

        if (result.ResultStatus != ResultStatus.Success.ToString())
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [HttpPost("create-floweCustom")]
    public async Task<ActionResult<Result<Flower>>> CreateFlowerCustom( [FromBody] CreateFlowerCustomRequest request)
    {
        var result = await _flowerCustomService.CreateFlowerCustom(request);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
    // [Authorize(Roles = "Admin")]
    [HttpPut("{flowerCustomId}")]
    public async Task<IActionResult> UpdateFlowerCustom([FromRoute] Guid flowerCustomIdId, UpdateFlowerCustomRequest request)
    {
        var result = await _flowerCustomService.UpdateFlowerCustom(flowerCustomIdId, request);
        return result.ResultStatus != ResultStatus.Success .ToString()? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }

    [HttpDelete("delete-flowerCustom")]
    public async Task<ActionResult<Result<Flower>>> DeleteFlowerCustom(Guid id)
    {
        var result = await _flowerCustomService.DeleteFlowerCustom(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}