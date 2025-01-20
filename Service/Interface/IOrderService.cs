using BusinessObject.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IOrderService
    {
        Task<OrderResponse> GetOrderByStoreId(Guid OrderId);
        Task<OrderResponse> GetOrderByCustomerId(Guid CustomerID);
        Task CreateOrder(OrderRequest orderRequest, Guid customerId);
        Task UpdateOrder(OrderResponse orderResponse);  
        Task DeleteOrder(Guid OrderID); 
    }
}
