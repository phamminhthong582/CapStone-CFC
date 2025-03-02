using BusinessObject.DTO.Refund;
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
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RefundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CancelOrder(Guid OrderId)
        {
            if (OrderId == null)
            {
                throw new Exception("orderId not found");
            }
            var order = (await _unitOfWork.Repository<Order>().GetByIdAsync(OrderId));
            var payment  = (await _unitOfWork.Repository<Payment>().GetAllAsync()).FirstOrDefault(n => n.OrderId == OrderId);
            /*var CustomerByOrder = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Customer).ToListAsync();*/
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(order.CustomerId);
            var wallet = (await _unitOfWork.Repository<Wallet>().GetAllAsync()).FirstOrDefault(n => n.CustomerId == customer.CustomerId);

            if (order.Transfer == false&& order.Status == "đã cọc")
            {
                order.Status = "Hủy thành công";
                _unitOfWork.Repository<Order>().Update(order);
                payment.Status = "Đã mất cọc";
                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.CompleteAsync();
            }
            else if (order.Transfer == true && order.Status == "đã thanh toán" )
            {
                TimeSpan timeUntilDelivery = order.RecipientTime.Value - DateTime.Now; // Tính khoảng cách thời gian

                if (timeUntilDelivery.TotalHours > 24 && order.Status == "đã thanh toán")
                {
                    double? refundPrice = order.OrderPrice * 70 / 100;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = refundPrice,
                        CreateAt = DateTime.Now
                    };

                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    order.Status = "Hủy thành công";
                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += refundPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);
                    await _unitOfWork.CompleteAsync();
                }
                else if (timeUntilDelivery.TotalHours <= 24 && order.Status == "đã thanh toán")
                {
                    double? refundPrice = order.OrderPrice * 50 / 100;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = refundPrice,
                        CreateAt = DateTime.Now
                    };

                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    order.Status = "Hủy thành công";
                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += refundPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);

                    await _unitOfWork.CompleteAsync();
                }
            }
        }

        public async Task<RefundResponse> GetRefundById(Guid refundId)
        {
            // Lấy thông tin refund từ database
            var refund = await _unitOfWork.Repository<Refund>().GetByIdAsync(refundId);

            // Kiểm tra nếu không tìm thấy refund
            if (refund == null)
            {
                throw new Exception($"Refund with ID {refundId} not found.");
            }

            var refundResponse = new RefundResponse
            {
                RefundId = refund.RefundId,
                OrderId = refund.OrderId,
                Price = refund.Price,
                WallerId = refund.WallerId,   
                CreateAt = refund.CreateAt,
                UpdateAt = refund.UpdateAt,
                Status = refund.Status
            };

            return refundResponse;
        }

        public async Task<IEnumerable<RefundResponse>> GetRefundByStoreId(Guid storeId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RefundResponse>> GetRefundByWalletId(Guid WalletId)
        {
            // Truy vấn các bản ghi refund có WalletId tương ứng
            var refunds = await _unitOfWork.Repository<Refund>().Entities.Where(n => n.WallerId == WalletId).ToListAsync();
                                            
            // Kiểm tra nếu không tìm thấy refund nào
            if (refunds == null || !refunds.Any())
            {
                throw new Exception($"No refunds found for Wallet ID: {WalletId}");
            }

            // Chuyển đổi danh sách Refund sang RefundResponse
            var refundResponses = refunds.Select(refund => new RefundResponse
            {
                RefundId = refund.RefundId,
                OrderId = refund.OrderId,
                Price = refund.Price,
                WallerId = refund.WallerId,   
                CreateAt = refund.CreateAt,
                UpdateAt = refund.UpdateAt,
                Status = refund.Status
            });

            return refundResponses;
        }


        public async Task UpdateStatusRefund(Guid RefundId, string status)
        {
            var refund = await _unitOfWork.Repository<Refund>().GetByIdAsync(RefundId);

            if (refund == null)
            {
                throw new Exception($"Refund with ID {RefundId} not found.");
            }

            refund.Status = status;
            refund.UpdateAt = DateTime.Now;
            if(status == "Refund")
            {

            }
            _unitOfWork.Repository<Refund>().Update(refund);
            await _unitOfWork.CompleteAsync();
        }

    }
}
