using BusinessObject.DTO.IncomeWallet;
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
    public class IncomWalletService : IIncomWalletService
    {
        private readonly IUnitOfWork _unitOfWork;

        public IncomWalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<IncomeWalletResponse>> GetInComWalletByWalletId(Guid walletId)
        {
            var wallet = await _unitOfWork.GetRepo<Wallet>().GetByIdAsync(walletId);
            if (wallet == null)
            {
                return Enumerable.Empty<IncomeWalletResponse>(); // Trả về danh sách rỗng nếu không tìm thấy ví
            }

            var incomes = await _unitOfWork.GetRepo<IncomeWallet>().Entities
                                          .Where(n => n.WalletID == walletId)
                                          .OrderByDescending(n => n.CreateAt) // Sắp xếp theo thời gian giảm dần
                                          .ToListAsync();

            return incomes.Select(income => new IncomeWalletResponse
            {
                IncomeWalletID = income.IncomeWalletID,
                WalletID = income.WalletID,
                IncomePrice = income.IncomePrice,
                Method = income.Method,
                Status = income.Status,
                OrderId = income.OrderId,
                CreateAt = income.CreateAt,
                UpdateAt = income.UpdateAt,
            }).ToList();
        }

    }
}