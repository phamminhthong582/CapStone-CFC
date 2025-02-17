using BusinessObject.DTO.Refund;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IRefundService
    {
        Task CancelOrder (Guid OrderId);
        Task UpdateStatusRefund(Guid RefundId,string status);
        Task<IEnumerable<RefundResponse>> GetRefundByStoreId(Guid storeId);
        Task<IEnumerable<RefundResponse>> GetRefundByWalletId(Guid WalletId);
        Task<RefundResponse> GetRefundById(Guid refundId);
    }
}
