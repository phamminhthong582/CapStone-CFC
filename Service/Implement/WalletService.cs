using BusinessObject.DTO.Wallet;
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
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateWallet(Guid CustomerId)
        {
            var walllet = new Wallet
            {
                CustomerId = CustomerId,
                TotalPrice = 0,
                CreateAt = DateTime.Now,
            };
            await _unitOfWork.Repository<Wallet>().AddAsync(walllet);
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
