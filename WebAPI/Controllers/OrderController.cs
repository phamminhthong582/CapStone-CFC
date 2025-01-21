using BusinessObject.DTO.Order;
using BusinessObject.DTO.Product;
using Core.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Service.Implement;
using Service.Interface;

namespace WebAPI.Controllers
{
    [Route("api/Order")]
    [ApiController]
    public class OrderController : Controller
    {
       private IOrderService orderService;

        public OrderController(IOrderService orderService)
        {
            this.orderService = orderService;
        }
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(OrderRequest orderRequest,Guid CustomerId)
        {
            await orderService.CreateOrder(orderRequest, CustomerId);
            return Ok(new BaseResponseModel<string>(
                     statusCode: StatusCodes.Status200OK,
                     code: ResponseCodeConstants.SUCCESS,
                     data: "Order thành công"));
        }
        [HttpGet("GetOrderByCustomer")]
        public async Task<IActionResult> GetOrderByCustomer(Guid CusomterId)
        {
            var result = await orderService.GetOrderByCustomerId(CusomterId);
            return Ok(new BaseResponseModel<IEnumerable<OrderResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpGet("GetOrderByStore")]
        public async Task<IActionResult> GetOrderByStore(Guid StoreId)
        {
            var result = await orderService.GetOrderByStoreID(StoreId);
            return Ok(new BaseResponseModel<IEnumerable<OrderResponse>>(
              statusCode: StatusCodes.Status200OK,
              code: ResponseCodeConstants.SUCCESS,
              data: result));
        }
        [HttpDelete("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            await orderService.DeleteOrder(id);

            return Ok(new BaseResponseModel<string>(
                             statusCode: StatusCodes.Status200OK,
                             code: ResponseCodeConstants.SUCCESS,
                             data: "xóa sản phẩm thành công"));
        }
        [HttpPut("UpdateOrder/{orderId}")]
        public async Task<IActionResult> UpdateOrder(OrderRequest orderRequest,Guid id)
        {
            await orderService.UpdateOrder(orderRequest,id);
            return Ok(new BaseResponseModel<string>(
                      statusCode: StatusCodes.Status200OK,
                      code: ResponseCodeConstants.SUCCESS,
                      data: "cập nhật sẩn phẩm thành công"));
        }

    }
}
