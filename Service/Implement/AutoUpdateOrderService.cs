using BusinessObject.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class AutoUpdateOrderService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AutoUpdateOrderService> _logger;

        public AutoUpdateOrderService(IServiceScopeFactory serviceScopeFactory, ILogger<AutoUpdateOrderService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AutoUpdateOrderService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var orders = (await unitOfWork.Repository<Order>().GetAllAsync())
                            .Where(o => o.Status == "đặt hàng thành công" && o.StaffId == null)
                            .ToList();
                        if (orders.Any())
                        {
                            var random = new Random();
                            var staffRepo = unitOfWork.Repository<Employee>();
                            var staffList = (await staffRepo.GetAllAsync());
                            var staffIds = staffList.Select(s => s.EmployeeId).ToList();

                            if (staffIds.Any())
                            {
                                foreach (var order in orders)
                                {
                                    foreach (var staff in staffList) { 
                                    if (order.StoreId != staff.StoreId)
                                    {
                                        return;
                                    }
                                        order.StaffId = staffIds[random.Next(staffIds.Count)];
                                        order.UpdateAt = DateTime.UtcNow;
                                        _logger.LogInformation($"Order {order.OrderId} assigned to Staff {order.StaffId}");
                                    }

                                }

                                await unitOfWork.CompleteAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in AutoUpdateOrderService: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(1000), stoppingToken);
            }
        }
    }
}
