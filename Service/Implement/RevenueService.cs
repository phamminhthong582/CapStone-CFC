using BusinessObject.DTO.Revenue;
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
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RevenueResponse> GetGeneralRevenue()
        {
            var revenueResponse = new RevenueResponse
            {
                January = 0,
                February = 0,
                March = 0,
                April = 0,
                May = 0,
                June = 0,
                July = 0,
                August = 0,
                September = 0,
                October = 0,
                November = 0,
                December = 0
            };

            // Truy vấn tất cả đơn hàng và lọc "Hoàn thành"
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Where(o => o.Status.Trim().ToLower() == "hoàn thành") // So sánh chính xác, tránh lỗi Unicode
                .ToListAsync(); // Đảm bảo truy vấn dữ liệu từ database

            foreach (var order in orders)
            {
                if (!order.UpdateAt.HasValue) continue; // Bỏ qua nếu UpdateAt null
                if (!order.OrderPrice.HasValue) continue; // Bỏ qua nếu OrderPrice null

                int month = order.UpdateAt.Value.Month;
                double orderPrice = order.OrderPrice.Value; // Lấy giá trị thực

                switch (month)
                {
                    case 1: revenueResponse.January += orderPrice; break;
                    case 2: revenueResponse.February += orderPrice; break;
                    case 3: revenueResponse.March += orderPrice; break;
                    case 4: revenueResponse.April += orderPrice; break;
                    case 5: revenueResponse.May += orderPrice; break;
                    case 6: revenueResponse.June += orderPrice; break;
                    case 7: revenueResponse.July += orderPrice; break;
                    case 8: revenueResponse.August += orderPrice; break;
                    case 9: revenueResponse.September += orderPrice; break;
                    case 10: revenueResponse.October += orderPrice; break;
                    case 11: revenueResponse.November += orderPrice; break;
                    case 12: revenueResponse.December += orderPrice; break;
                }
            }

            return revenueResponse; // Đảm bảo luôn trả về đầy đủ 12 tháng
        }


        public async Task<RevenueResponse> GetRevenueByStoreId(Guid storeId)
        {
            // Khởi tạo tất cả các tháng có giá trị mặc định là 0
            var revenueResponse = new RevenueResponse
            {
                January = 0,
                February = 0,
                March = 0,
                April = 0,
                May = 0,
                June = 0,
                July = 0,
                August = 0,
                September = 0,
                October = 0,
                November = 0,
                December = 0
            };

            // Lấy danh sách đơn hàng hoàn thành
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Where(o => o.StoreId == storeId && o.Status.Trim().ToLower() == "hoàn thành")
                .ToListAsync();

            foreach (var order in orders)
            {
                if (!order.UpdateAt.HasValue) continue; // Bỏ qua nếu UpdateAt là null

                int month = order.UpdateAt.Value.Month;
                double orderPrice = order.OrderPrice ?? 0; // Nếu OrderPrice null thì thay bằng 0

                switch (month)
                {
                    case 1: revenueResponse.January += orderPrice; break;
                    case 2: revenueResponse.February += orderPrice; break;
                    case 3: revenueResponse.March += orderPrice; break;
                    case 4: revenueResponse.April += orderPrice; break;
                    case 5: revenueResponse.May += orderPrice; break;
                    case 6: revenueResponse.June += orderPrice; break;
                    case 7: revenueResponse.July += orderPrice; break;
                    case 8: revenueResponse.August += orderPrice; break;
                    case 9: revenueResponse.September += orderPrice; break;
                    case 10: revenueResponse.October += orderPrice; break;
                    case 11: revenueResponse.November += orderPrice; break;
                    case 12: revenueResponse.December += orderPrice; break;
                }
            }

            return revenueResponse; // Luôn trả về đủ 12 tháng dù không có đơn hàng
        }



    }
}
