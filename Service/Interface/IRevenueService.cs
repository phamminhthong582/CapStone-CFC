using BusinessObject.DTO.Revenue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IRevenueService
    {
        Task<RevenueResponse> GetRevenueByStoreId(Guid StoreId);
        /*Task GetMonthRevenueByStoreId(Guid StoreId);
        Task GetYearRevenueByStoreId(Guid StoreId);*/
        Task<RevenueResponse> GetGeneralRevenue();
       /* Task GetMonthGeneralRevenuByStoreId(Guid StoreId);
        Task GetYearGeneralRevenuByStoreId(Guid StoreId);*/

    }
}
