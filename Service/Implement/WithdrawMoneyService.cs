using BusinessObject.DTO.Wallet;
using BusinessObject.DTO.WithdrawMoney;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(WithdrawMoney.WalletId);
            if (otp == WithdrawMoney.Otp)
            {
                WithdrawMoney.Status = "request successful";

                wallet.TotalPrice -= WithdrawMoney.Price;
            }
            _unitOfWork.Repository<Wallet>().Update(wallet);
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
            if (withdrawMoney.Status == "request successful")
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


        public async Task<Guid> CreateWithdrawMoney(Guid WalletId, WithdrawMoneyRequest withdrawMoneyRequest)
        {
            var wallet = await _unitOfWork.Repository<Wallet>()
                                    .Entities
                                    .Include(w => w.Customer)
                                    .FirstOrDefaultAsync(w => w.WalletId == WalletId);

            if (wallet == null)
            {
                throw new Exception("Wallet not found.");
            }

            if (withdrawMoneyRequest.PasswordWallet != wallet.PasswordWallet)
            {
                throw new Exception("Sai mật khẩu.");
            }

            if (withdrawMoneyRequest.Price > wallet.TotalPrice)
            {
                throw new Exception("Số tiền không hợp lệ.");
            }

            var otp = new Random().Next(100000, 999999).ToString();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var withdrawMoney = new WithdrawMoney
            {
                WalletId = WalletId,
                Price = withdrawMoneyRequest.Price,
                BankAccountName = withdrawMoneyRequest.BankAccountName,
                BankName = withdrawMoneyRequest.BankName,
                BankNumber = withdrawMoneyRequest.BankNumber,
                Reason = withdrawMoneyRequest.Reason,
                Status = "Waiting OTP",
                Otp = otp,
                CreateAt = vietnamTime,
            };

            await _unitOfWork.Repository<WithdrawMoney>().AddAsync(withdrawMoney);
            await _unitOfWork.CompleteAsync(); // Lưu vào database để có Id

            var emailService = new SendMailWithrawMoneyService();
            string subject = "Xác nhận rút tiền";
            string body = $"Xin chào {wallet.Customer.FullName},\n\n" +
                          $"Bạn đã yêu cầu rút số tiền {withdrawMoneyRequest.Price} VNĐ từ ví của mình.\n" +
                          $"Mã OTP của bạn là: {otp}\n\n" +
                          $"Vui lòng nhập mã OTP này để hoàn tất quá trình rút tiền.\n\n" +
                          $"Trân trọng,\nYour Company Name.";

            await emailService.SendEmailAsync(wallet.Customer.Email, subject, body);

            return withdrawMoney.WithdrawMoneyId; // Trả về ID của yêu cầu rút tiền
        }

          public async Task<IEnumerable<WithdrawMoneyResponse>> GetWithDrawMoney()
        {
            var withdraws = (await _unitOfWork.Repository<WithdrawMoney>().GetAllAsync()).Where(w =>  w.Status != "Waiting OTP");


            var withdrawResponses = withdraws.Select(w => new WithdrawMoneyResponse
            {
                WalletId = w.WalletId,
                WithdrawMoneyId = w.WithdrawMoneyId,
                Price = w.Price,
                BankAccountName = w.BankAccountName,
                BankName = w.BankName,
                BankNumber = w.BankNumber,
                Reason = w.Reason,
                Status = w.Status,

            });

            return withdrawResponses;
        }
        public async Task<IEnumerable<WithdrawMoneyResponse>> GetWithDrawMoneyByWalletId(Guid WalletId)
        {
            var withdraws = (await _unitOfWork.Repository<WithdrawMoney>().GetAllAsync()).Where(w => w.WalletId == WalletId && w.Status != "Waiting OTP");

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
                Status = w.Status,
               
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
            var wallet = await _unitOfWork.Repository<Wallet>().Entities.FirstOrDefaultAsync(m => m.WalletId == withdrawMoney.WalletId);
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var walletAdmin = await _unitOfWork.Repository<Wallet>().Entities
            .FirstOrDefaultAsync(n => n.WalletId == Guid.Parse("55d9964b-8543-4b74-96d6-e0ab2ce86d3f"));
            withdrawMoney.Status = status;
            if (status == "Successfull")
            {
                var incomWallet = new IncomeWallet
                {
                    WalletID = withdrawMoney.WalletId,
                    IncomePrice = -(withdrawMoney.Price),
                    Method = "Withdraw Money successfull",
                    Status = "Successfull",
                    CreateAt = withdrawMoney.CreateAt,
                    UpdateAt = vietnamTime,

                };
                await _unitOfWork.Repository<IncomeWallet>().AddAsync(incomWallet);
                await _unitOfWork.CompleteAsync();
                //walletAdmin.TotalPrice -= incomWallet.IncomePrice;
                //_unitOfWork.Repository<Wallet>().Update(walletAdmin);
                //var inComeWallet = new IncomeWallet
                //{
                //    WalletID = walletAdmin.WalletId,
                //    IncomePrice = incomWallet.IncomePrice,
                //    Method = "Payment",
                //    Status = "Successfull",
                //    CreateAt = vietnamTime,
                //    UpdateAt = vietnamTime,
                //};
                //await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(inComeWallet);
                //await _unitOfWork.CompleteAsync();

            }
            if (status == "Failure")
            {
                wallet.TotalPrice += withdrawMoney.Price;
                wallet.UpdateAt = DateTime.Now;
                 _unitOfWork.Repository<Wallet>().Update(wallet);
                var incomWallet = new IncomeWallet
                {
                    WalletID = withdrawMoney.WalletId,
                    IncomePrice = (withdrawMoney.Price),
                    Method = "Withdraw Money failure",
                    Status = "Successfull",
                    CreateAt = withdrawMoney.CreateAt,
                    UpdateAt = vietnamTime,

                };
                await _unitOfWork.Repository<IncomeWallet>().AddAsync(incomWallet);
            }
            _unitOfWork.Repository<WithdrawMoney>().Update(withdrawMoney);  
            await _unitOfWork.CompleteAsync();  
        }

      
    }
}
