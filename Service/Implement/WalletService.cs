using BusinessObject.DTO.Wallet;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Implement
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public WalletService(IUnitOfWork unitOfWork, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<bool> CheckWallet(Guid CustomerId)
        {
            var wallet =await _unitOfWork.GetRepo<Wallet>().Entities.FirstOrDefaultAsync(n => n.CustomerId == CustomerId);
            if (wallet != null)
            {
                return true;
            }
            return false;
        }

        public async Task CreateWallet(Guid CustomerId, string PasswordWallet)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var Wallet = new Wallet
            {
                CustomerId = CustomerId,
                PasswordWallet = PasswordWallet,
                TotalPrice = 0,
                CreateAt = vietnamTime,
            };
            await _unitOfWork.Repository<Wallet>().AddAsync(Wallet);
            await _unitOfWork.CompleteAsync();

        }

        public async Task DepositWallet(Guid WalletId, double price)
        {
           var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(WalletId);
            wallet.TotalPrice += price; // Cộng dồn số tiền vào ví
            _unitOfWork.GetRepo<Wallet>().Update(wallet);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var IncomeWallet = new IncomeWallet
            {
                WalletID = wallet.WalletId,
                IncomePrice = price,
                Method = "Deposit",
                Status = "Successfull",
                CreateAt = vietnamTime,
                UpdateAt= vietnamTime,
            };
           await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(IncomeWallet);

            await _unitOfWork.CompleteAsync();

        }

        public async Task<WalletResponse> GetWallet(Guid customerId)
        {
            var wallet = await _unitOfWork.Repository<Wallet>()
                                           .Entities
                                           .FirstOrDefaultAsync(w => w.CustomerId == customerId);

            if (wallet == null)
            {
                throw new Exception("Wallet not found for this customer.");
            }

            var walletResponse = new WalletResponse
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                TotalPrice = wallet.TotalPrice,
                UpdateAt = wallet.UpdateAt,
                CreateAt = wallet.CreateAt,
            };

            return walletResponse;
        }

        public async Task<WalletResponse> GetWalletByAdmin(Guid id)
        {
            var wallet = await _unitOfWork.Repository<Wallet>()
                                           .Entities
                                           .FirstOrDefaultAsync(w => w.WalletId == id);

            if (wallet == null)
            {
                throw new Exception("Wallet not found for this customer.");
            }

            var walletResponse = new WalletResponse
            {
                WalletId = wallet.WalletId,
                CustomerId = wallet.CustomerId,
                TotalPrice = wallet.TotalPrice,
                UpdateAt = wallet.UpdateAt,
                CreateAt = wallet.CreateAt,
            };

            return walletResponse;
        }

        public async Task PaymentByWallet(Guid OrderId, string passwordWallet)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities.Include(m => m.DesignCustom).FirstOrDefaultAsync(n => n.OrderId == OrderId);
            var wallet = await _unitOfWork.GetRepo<Wallet>().Entities.Where(n => n.CustomerId == order.CustomerId).FirstOrDefaultAsync();
            var designcustom = await _unitOfWork.Repository<DesignCustom>().Entities.FirstOrDefaultAsync(n => n.OrderId == OrderId);
            var walletAdmin = await _unitOfWork.Repository<Wallet>().Entities
                .FirstOrDefaultAsync(n => n.WalletId == Guid.Parse("55d9964b-8543-4b74-96d6-e0ab2ce86d3f"));
            if (passwordWallet == wallet.PasswordWallet)
            {
                if (order.Transfer == false) {
                    if (wallet.TotalPrice > order.OrderPrice/2) {
                        await _paymentService.CreatePayment(OrderId);
                        wallet.TotalPrice -= order.OrderPrice / 2;
                         _unitOfWork.GetRepo<Wallet>().Update(wallet);
                        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                        if(order.DesignCustomId != null)
                        {
                            designcustom.Status = "Design Successfully";
                            _unitOfWork.GetRepo<DesignCustom>().Update(designcustom);
                            await _unitOfWork.CompleteAsync();

                        }
                        var incomeWallet = new IncomeWallet
                        {
                            WalletID = wallet.WalletId,
                            IncomePrice = -(order.OrderPrice / 2),
                            Method = "Payment",
                            Status = "Successfull",
                            CreateAt = vietnamTime,
                            UpdateAt = vietnamTime,
                            OrderId = OrderId,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                        await _unitOfWork.CompleteAsync();
                        walletAdmin.TotalPrice += (order.OrderPrice / 2);
                        _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                        var inComeWallet = new IncomeWallet
                        {
                            WalletID = walletAdmin.WalletId,
                            IncomePrice = (order.OrderPrice / 2),
                            Method = "Payment",
                            Status = "Successfull",
                            CreateAt = vietnamTime,
                            UpdateAt = vietnamTime,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(inComeWallet);
                        await _unitOfWork.CompleteAsync();


                    }
                    else
                    {
                        throw new Exception("Wallet not found for this customer.");

                    }
                }
                else if (order.Transfer == true)
                {
                    if (wallet.TotalPrice > order.OrderPrice )
                    {
                        await _paymentService.CreatePayment(OrderId);
                        wallet.TotalPrice -= order.OrderPrice;
                        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                        if (order.DesignCustomId != null)
                        {
                            designcustom.Status = "Design Successfully";
                            _unitOfWork.GetRepo<DesignCustom>().Update(designcustom);
                            await _unitOfWork.CompleteAsync();

                        }
                        var incomeWallet = new IncomeWallet
                        {

                            WalletID = wallet.WalletId,
                            IncomePrice = -(order.OrderPrice),
                            Method = "Payment",
                            Status = "Successfull",
                            OrderId = OrderId,
                            CreateAt = vietnamTime,
                            UpdateAt = vietnamTime,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                        _unitOfWork.GetRepo<Wallet>().Update(wallet);
                        await _unitOfWork.CompleteAsync();
                        walletAdmin.TotalPrice += order.OrderPrice;
                        _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                        var inComeWallet = new IncomeWallet
                        {
                            WalletID = walletAdmin.WalletId,
                            IncomePrice = order.OrderPrice,
                            Method = "Payment",
                            Status = "Successfull",
                            CreateAt = vietnamTime,
                            UpdateAt = vietnamTime,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(inComeWallet);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                else
                {
                    throw new Exception("Wallet not found for this customer.");

                }
            }

            else
            {
                throw new Exception("Wallet not found for this customer.");

            }
        }
    }
}
