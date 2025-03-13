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

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
