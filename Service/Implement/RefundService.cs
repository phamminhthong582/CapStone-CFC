using BusinessObject.DTO.Refund;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var walletAdmin = await _unitOfWork.Repository<Wallet>().Entities
               .FirstOrDefaultAsync(n => n.WalletId == Guid.Parse("55d9964b-8543-4b74-96d6-e0ab2ce86d3f"));
            if (order.Transfer == false && order.Status != "Received")
            {
                if (order.Status != "Order Successfully" && order.Status != "Delivery")
                {
                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "Cancel deposit";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    await _unitOfWork.CompleteAsync();
                }
                else if(order.Status == "Order Successfully" && order.Status != "Delivery")
                {
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = order.OrderPrice/2,
                        CreateAt = vietnamTime,
                        Status = "Refund Successfull",
                        StoreId = order.StoreId,
                        
                    };
                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    var inComWallet = new IncomeWallet
                    {
                        WalletID = wallet.WalletId,
                        IncomePrice = order.OrderPrice / 2,
                        Method ="Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                        OrderId = order.OrderId,

                    };
                    await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                    await _unitOfWork.CompleteAsync();

                    walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                    _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                    var incomeWallet = new IncomeWallet
                    {
                        WalletID = walletAdmin.WalletId,
                        IncomePrice = -inComWallet.IncomePrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                    };
                    await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);

                    await _unitOfWork.CompleteAsync();
                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;

                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += order.OrderPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);
                    await _unitOfWork.CompleteAsync();
                }
            }
           
            else if (order.Transfer == true && order.Status != "Received")
            {
                TimeSpan timeUntilDelivery = order.RecipientTime.Value - vietnamTime; // Tính khoảng cách thời gian

                if (order.Status == "Order Successfully")
                {
                    double? refundPrice = order.OrderPrice;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = order.OrderPrice,
                        CreateAt = vietnamTime,
                        Status = "Refund Successfull",
                        StoreId = order.StoreId,

                    };
                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    var inComWallet = new IncomeWallet
                    {
                        WalletID = wallet.WalletId,
                        IncomePrice = refundPrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                        OrderId = order.OrderId,
                    };
                    await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                    await _unitOfWork.CompleteAsync();

                    walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                    _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                    var incomeWallet = new IncomeWallet
                    {
                        WalletID = walletAdmin.WalletId,
                        IncomePrice = -inComWallet.IncomePrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                    };
                    await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);

                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;

                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += refundPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);
                    await _unitOfWork.CompleteAsync();
                }
                else if (order.Status != "Order Successfully"&&timeUntilDelivery.TotalHours > 24 && order.Status != "Received")
                {

                    double? refundPrice = order.OrderPrice * 70 / 100;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = refundPrice,
                        CreateAt = vietnamTime,
                        Status = "Refund Successfull",
                        StoreId = order.StoreId,

                    };

                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    var inComWallet = new IncomeWallet
                    {
                        WalletID = wallet.WalletId,
                        IncomePrice = refundPrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                        OrderId = order.OrderId,

                    };
                    await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                    await _unitOfWork.CompleteAsync();

                    walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                    _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                    var incomeWallet = new IncomeWallet
                    {
                        WalletID = walletAdmin.WalletId,
                        IncomePrice = -inComWallet.IncomePrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                    };
                    await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;

                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += refundPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);
                    await _unitOfWork.CompleteAsync();
                }
                else if (order.Status != "Order Successfully" && timeUntilDelivery.TotalHours <= 24 && order.Status != "Received")
                {
                    double? refundPrice = order.OrderPrice * 50 / 100;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = refundPrice,
                        CreateAt = vietnamTime,
                        Status = "Refund Successfull",
                        StoreId = order.StoreId,
                    };

                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    var inComWallet = new IncomeWallet
                    {
                        WalletID = wallet.WalletId,
                        IncomePrice = refundPrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                        OrderId = order.OrderId,

                    };
                    await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                    await _unitOfWork.CompleteAsync();

                    walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                    _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                    var incomeWallet = new IncomeWallet
                    {
                        WalletID = walletAdmin.WalletId,
                        IncomePrice = -inComWallet.IncomePrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                    };
                    await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;

                    order.Refund = true;
                    _unitOfWork.Repository<Order>().Update(order);
                    payment.Status = "refund";
                    _unitOfWork.Repository<Payment>().Update(payment);
                    wallet.TotalPrice += refundPrice;
                    _unitOfWork.Repository<Wallet>().Update(wallet);

                    await _unitOfWork.CompleteAsync();
                }
                else if (order.Status != "Order Successfully" && timeUntilDelivery.TotalHours < 3 && order.Status != "Received")
                {

                    double? refundPrice = order.OrderPrice * 30 / 100;
                    var refund = new Refund
                    {
                        OrderId = order.OrderId,
                        WallerId = wallet.WalletId,
                        Price = refundPrice,
                        CreateAt = vietnamTime
                    };

                    await _unitOfWork.Repository<Refund>().AddAsync(refund);
                    var inComWallet = new IncomeWallet
                    {
                        WalletID = wallet.WalletId,
                        IncomePrice = refundPrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                        OrderId = order.OrderId,

                    };
                    await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                    await _unitOfWork.CompleteAsync();
                    walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                    _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                    var incomeWallet = new IncomeWallet
                    {
                        WalletID = walletAdmin.WalletId,
                        IncomePrice = -inComWallet.IncomePrice,
                        Method = "Refund",
                        Status = "Successfull",
                        CreateAt = vietnamTime,
                        UpdateAt = vietnamTime,
                    };
                    await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                    order.Status = "Cancel";
                    order.UpdateAt = vietnamTime;
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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            refund.Status = status;
            refund.UpdateAt = vietnamTime;
            if(status == "Refund")
            {

            }
            _unitOfWork.Repository<Refund>().Update(refund);
            await _unitOfWork.CompleteAsync();
        }

    }
}
