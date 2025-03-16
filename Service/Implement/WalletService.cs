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
            var Wallet = new Wallet
            {
                CustomerId = CustomerId,
                PasswordWallet = PasswordWallet,
                TotalPrice = 0,
                CreateAt = DateTime.Now,
            };
            await _unitOfWork.Repository<Wallet>().AddAsync(Wallet);
            await _unitOfWork.CompleteAsync();

        }

        public async Task DepositWallet(Guid WalletId, double price)
        {
           var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(WalletId);
            wallet.TotalPrice += price; // Cộng dồn số tiền vào ví
            _unitOfWork.GetRepo<Wallet>().Update(wallet);
            var IncomeWallet = new IncomeWallet
            {
                WalletID = wallet.WalletId,
                IncomePrice = price,
                Method = "Deposit",
                Status = "Successfull",
                CreateAt = DateTime.Now,
                UpdateAt= DateTime.Now,
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

        public async Task PaymentByWallet(Guid OrderId, string passwordWallet)
        {
            var order = await _unitOfWork.GetRepo<Order>().GetByIdAsync(OrderId);
            var wallet = await _unitOfWork.GetRepo<Wallet>().Entities.Where(n => n.CustomerId == order.CustomerId).FirstOrDefaultAsync();
            if (passwordWallet == wallet.PasswordWallet)
            {
                if (order.Transfer == false) {
                    if (wallet.TotalPrice > order.OrderPrice/2) {
                        await _paymentService.CreatePayment(OrderId);
                        wallet.TotalPrice -= order.OrderPrice / 2;
                         _unitOfWork.GetRepo<Wallet>().Update(wallet);
                        var incomeWallet = new IncomeWallet
                        {
                            WalletID = wallet.WalletId,
                            IncomePrice = -wallet.TotalPrice,
                            Method = "Payment",
                            Status = "Successfull",
                            CreateAt = DateTime.Now,
                            UpdateAt = DateTime.Now,
                            OrderId = OrderId,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                        await _unitOfWork.CompleteAsync();
                    }else
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
                        var incomeWallet = new IncomeWallet
                        {
                            WalletID = wallet.WalletId,
                            IncomePrice = -wallet.TotalPrice,
                            Method = "Payment",
                            Status = "Successfull",
                            OrderId = OrderId,
                            CreateAt = DateTime.Now,
                            UpdateAt = DateTime.Now,
                        };
                        await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                        _unitOfWork.GetRepo<Wallet>().Update(wallet);
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
