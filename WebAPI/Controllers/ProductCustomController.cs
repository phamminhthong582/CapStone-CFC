using System.Net;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;

namespace WebAPI.Controllers;
[Route("api/productcustoms")]
[ApiController]
public class ProductCustomController : Controller
{
    private readonly IProductCustomService _productCustomService;

    public ProductCustomController(IProductCustomService productCustomService)
    {
        _productCustomService = productCustomService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAllProductCustom()
    {
        var result = await _productCustomService.GetAllProductCustom();
        return Ok(result);
    }
    //[HttpGet("Id")]
    //public async Task<IActionResult> GetProductCustomById(Guid id)
    //{
    //    var result = await _productCustomService.GetProductCustomById(id);

    //    if (result.ResultStatus != ResultStatus.Success.ToString())
    //        return StatusCode((int)HttpStatusCode.InternalServerError, result);
    //    return Ok(result);
    //}
    [HttpPost("create-productcustom")]
    public async Task<ActionResult<Result<FlowerBasket>>> CreateProductCustom(Guid CustomerId, [FromBody] CreateProductCustomRequest request)
    {
        var result = await _productCustomService.CreateProductCustom(CustomerId,request);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
    // [Authorize(Roles = "Admin")]
    [HttpPut("{productcustomId}")]
    public async Task<IActionResult> UpdateProductCustom([FromRoute] Guid productcustomId, UpdateProductCustomRequest request)
    {
        var result = await _productCustomService.UpdateProductCustom(productcustomId, request);
        return result.ResultStatus != ResultStatus.Success .ToString()? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }

    [HttpDelete("delete-productcustom")]
    public async Task<ActionResult<Result<Flower>>> DeleteProductCustom(Guid id)
    {
        var result = await _productCustomService.DeleteProductCustom(id);
        if (result.ResultStatus != ResultStatus.Success.ToString())
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }
        return Ok(result);
    }
}