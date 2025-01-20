using BusinessObject.DTO.Order;
using BusinessObject.Entities;
using MimeKit.Cryptography;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class OrderService : IOrderService
    {
        public async Task CreateOrder(OrderRequest orderRequest, Guid customerId)
        {
            Guid? PromotionID = orderRequest.PromotionId;
            string? DeliveryAddress = orderRequest.DeliveryAddress;
            string? Note = orderRequest.Note;   
            DateTime? DeliveryDateTime = orderRequest.DeliveryDateTime;
            string? Phone = orderRequest.Phone;
            bool? Transfer= orderRequest.Transfer;  

        }

        public Task DeleteOrder(Guid OrderID)
        {
            throw new NotImplementedException();
        }

        public Task<OrderResponse> GetOrderByCustomerId(Guid CustomerID)
        {
            throw new NotImplementedException();
        }

        public Task<OrderResponse> GetOrderByStoreId(Guid OrderId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateOrder(OrderResponse orderResponse)
        {
            throw new NotImplementedException();
        }
    }
}
