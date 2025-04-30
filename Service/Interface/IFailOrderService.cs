using BusinessObject.DTO.FailOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IFailOrderService
    {
        Task<FailOrderResponse> GetFailOrderByOrderId(Guid orderId);
    }
}
