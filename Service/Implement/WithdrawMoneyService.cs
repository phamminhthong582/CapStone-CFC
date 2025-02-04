using BusinessObject.DTO.Wallet;
using BusinessObject.DTO.WithdrawMoney;
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
    public class WithdrawMoneyService : IWithdrawMoneyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WithdrawMoneyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ConfirmOPT(Guid WithdrawMoneyId, string otp)
        {
            var WithdrawMoney = await _unitOfWork.Repository<WithdrawMoney>().GetByIdAsync(WithdrawMoneyId);
            if(otp == WithdrawMoney.Otp)
            {
                WithdrawMoney.Status = "Đã xác nhận";
            }
            _unitOfWork.Repository<WithdrawMoney>().Update(WithdrawMoney);
            await _unitOfWork.CompleteAsync();

        }
        public async Task DeleteWithdrawMoney(Guid withdrawMoneyId)
        {
            // Tìm giao dịch rút tiền theo ID
            var withdrawMoney = await _unitOfWork.Repository<WithdrawMoney>().GetByIdAsync(withdrawMoneyId);
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(withdrawMoney.WalletId);  
            // Kiểm tra nếu không tìm thấy giao dịch
            if (withdrawMoney == null)
            {
                throw new Exception("Không tìm thấy giao dịch rút tiền.");
            }

            // Kiểm tra trạng thái giao dịch, chỉ cho phép xóa khi trạng thái là 'Đang chờ xác nhận OTP'
            if (withdrawMoney.Status == "Đang chờ xác nhận OTP")
            {
                _unitOfWork.Repository<WithdrawMoney>().Delete(withdrawMoney);
                wallet.TotalPrice += withdrawMoney.Price;
                _unitOfWork.Repository<Wallet>().Update(wallet);
               await _unitOfWork.CompleteAsync();

            }
            else
            {
                throw new Exception("Chỉ có thể xóa giao dịch đang chờ xác nhận OTP.");
            }
        }


        public async Task CreateWithdrawMoney(Guid WalletId, WithdrawMoneyRequest withdrawMoneyRequest)
        {
            var wallet = await _unitOfWork.Repository<Wallet>()
                                    .Entities
                                    .Include(w => w.Customer)  // Include thông tin Customer để lấy email
                                    .FirstOrDefaultAsync(w => w.WalletId == WalletId);

            if (wallet == null)
            {
                throw new Exception("Wallet not found.");
            }
            if (withdrawMoneyRequest.Price > wallet.TotalPrice)
            {
                throw new Exception("Số tiền không hợp lệ");
            }
            var otp = new Random().Next(100000, 999999).ToString();

            var withdrawMoney = new WithdrawMoney
            {
                WalletId = WalletId,
                Price = withdrawMoneyRequest.Price,
                BankAccountName = withdrawMoneyRequest.BankAccountName,
                BankName = withdrawMoneyRequest.BankName,
                BankNumber = withdrawMoneyRequest.BankNumber,
                Reason = withdrawMoneyRequest.Reason,
                Status = "Đang chờ xác nhập OTP",
                Otp = otp,
                CreateAt = DateTime.Now,
            };
            await _unitOfWork.Repository<WithdrawMoney>().AddAsync(withdrawMoney);
            wallet.TotalPrice -= withdrawMoneyRequest.Price;
            _unitOfWork.Repository<Wallet>().Update(wallet);
            await _unitOfWork.CompleteAsync();
            var emailService = new SendMailWithrawMoneyService();
            string subject = "Xác nhận rút tiền";
            string body = $"Xin chào {wallet.Customer.FullName},\n\n" +
                          $"Bạn đã yêu cầu rút số tiền {withdrawMoneyRequest.Price} VNĐ từ ví của mình.\n" +
                          $"Mã OTP của bạn là: {otp}\n\n" +
                          $"Vui lòng nhập mã OTP này để hoàn tất quá trình rút tiền.\n\n" +
                          $"Trân trọng,\nYour Company Name.";

            await emailService.SendEmailAsync(wallet.Customer.Email, subject, body);


        }

      

        public async Task<IEnumerable<WithdrawMoneyResponse>> GetWithDrawMoneyByWalletId(Guid WalletId)
        {
            var withdraws = (await _unitOfWork.Repository<WithdrawMoney>().GetAllAsync()).Where(w => w.WalletId == WalletId);

            if (!withdraws.Any())
            {
                throw new Exception("Không tìm thấy giao dịch rút tiền nào cho ví này.");
            }

            var withdrawResponses = withdraws.Select(w => new WithdrawMoneyResponse
            {
                WithdrawMoneyId = w.WithdrawMoneyId,
                Price = w.Price,
                BankAccountName = w.BankAccountName,
                BankName = w.BankName,
                BankNumber = w.BankNumber,
                Reason = w.Reason,
            });

            return withdrawResponses;
        }
        public async Task<WithdrawMoneyResponse> GetWithDrawMoneyByWithdrawMoneyId(Guid WithdrawMoneyId)
        {
            var withdrawMoney = await _unitOfWork.Repository<WithdrawMoney>().GetByIdAsync(WithdrawMoneyId);
            var withdrawResponses = new WithdrawMoneyResponse
            {
                WithdrawMoneyId = withdrawMoney.WithdrawMoneyId,
                Price = withdrawMoney.Price,
                BankAccountName = withdrawMoney.BankAccountName,
                BankName = withdrawMoney.BankName,
                BankNumber = withdrawMoney.BankNumber,
                Reason = withdrawMoney.Reason,
            };

            return withdrawResponses;

        }
        public async Task UpdateStatusWithdrawMoney(Guid WithdrawMoneyId, string status)
        {
            var withdrawMoney = await _unitOfWork.Repository<WithdrawMoney>().GetByIdAsync(WithdrawMoneyId);
            withdrawMoney.Status = status; ;
            _unitOfWork.Repository<WithdrawMoney>().Update(withdrawMoney);  
            await _unitOfWork.CompleteAsync();  
        }

       
    }
}
