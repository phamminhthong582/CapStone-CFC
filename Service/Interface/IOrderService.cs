using BusinessObject.DTO.Order;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IOrderService
    {
        Task<OrderResponse> GetOrderById(Guid OrderId);
        Task<IEnumerable<OrderResponse>> GetOrderByCustomerId(Guid CustomerID);
        Task<IEnumerable<OrderResponse>> GetOrderByStoreID(Guid StoreID);
        Task<Order> CreateOrder(OrderRequest orderRequest, Guid customerId);
        Task UpdateOrder(OrderRequest orderRequest, Guid orderId);  
        Task DeleteOrder(Guid OrderID);
        Task UpdateOrderByStoreId(Guid orderId, Guid StaffId);
        Task UpdateStatusOrderByStaffId(Guid orderId, string Status);
        Task<IEnumerable<OrderResponse>> GetOrderByStaffId(Guid StaffId);
        Task<Order> ConvertCartToOrder(Guid CustomerID, OrderCartRequest orderRequest);

        Task<Order> CreateOrderCustom(Guid Customer , OrderCustomRequest orderCustomRequest);
    }
}
