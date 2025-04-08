using BusinessObject.DTO.Revenue;
using BusinessObject.Entities;
using Microsoft.Identity.Client;
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
        Task<TotalOrderResponse> GetTotalOrdersByStoreId(Guid StoreId);

        Task<RevenueResponse> GetLossByStoreId(Guid StoreId);
        Task<RevenueResponse> GetGeneralLoss();

        Task<TotalOrderResponse> GetTotalOrder();


        Task<IEnumerable<StoreRevenue>> GetAllStoreRevenue();


    }
}
