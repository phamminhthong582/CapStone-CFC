using BusinessObject.DTO.Employee;
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

        Task<IEnumerable<OrderResponse>> GetFailOrderByCustomerId(Guid CustomerID);
        Task<IEnumerable<OrderResponse>> GetCanelOrderByCustomerId(Guid CustomerID);


        Task<IEnumerable<OrderResponse>> GetRefundOrderByCustomerId(Guid CustomerID);

        Task<IEnumerable<OrderResponse>> GetOrderByStoreID(Guid StoreID);
        Task<IEnumerable<OrderResponse>> GetRefundOrderByStoreID(Guid StoreID);
        Task<IEnumerable<OrderResponse>> GetFailOrderByStoreID(Guid StoreID);

        Task<Order> CreateOrder(OrderRequest orderRequest, Guid customerId);
        Task UpdateOrder(OrderRequest orderRequest, Guid orderId);  
        Task DeleteOrder(Guid OrderID);
        Task UpdateOrderByStoreId(Guid orderId, Guid StaffId);
        Task UpdateStatusOrderByStaffId(Guid orderId, string Status);
        Task<IEnumerable<OrderResponse>> GetOrderByStaffId(Guid StaffId);
        Task<IEnumerable<OrderResponse>> GetRefundOrderByStaffId(Guid StaffId);

        Task<Order> ConvertCartToOrder(Guid CustomerID, OrderCartRequest orderRequest);

        Task<Order> CreateOrderCustom(Guid Customer , OrderCustomRequest orderCustomRequest);

        Task AutoUpdateOrder();

        Task<IEnumerable<EmployeeResponse>> GetStaffForOrderId(Guid orderId);
        Task<IEnumerable<EmployeeResponse>> GetDeliveryForOrderId(Guid orderId);




    }
}
