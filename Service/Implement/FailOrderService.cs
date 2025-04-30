using BusinessObject.DTO.FailOrder;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class FailOrderService : IFailOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FailOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FailOrderResponse> GetFailOrderByOrderId(Guid orderId)
        {
            var order = await _unitOfWork.GetRepo<FailOrder>().Entities
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null)
            {
                return null; // Hoặc throw exception tùy use-case
            }

            var response = new FailOrderResponse
            {
                FailOrderId = order.FailOrderId,
                ReasonFail = order.ReasonFail,
                ImageFail = order.ImageFail,
                TimeDelay = order.TimeDelay,
                RefundPrice = order.RefundPrice,
                Wallet = order.Wallet,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderId = order.OrderId,
                StaffId = order.StaffId,
                DeliveryId = order.DeliveryId,
                ShipperId = order.ShipperId
            };

            return response;
        }

    }
}
