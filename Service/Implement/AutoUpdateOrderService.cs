using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Service.Interface;
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
                        var chatRoomService = scope.ServiceProvider.GetRequiredService<IChatRoomService>();

                        var orders = (await unitOfWork.Repository<Order>().GetAllAsync())
                            .Where(o => o.Status == "Order Successfully" && o.StaffId == null)
                            .ToList();

                        if (orders.Any())
                        {
                            var random = new Random();
                            var staffRepo = unitOfWork.Repository<Employee>();
                            var staffList = await staffRepo.GetAllAsync();

                            foreach (var order in orders)
                            {
                                var availableStaff = staffList.Where(s => s.StoreId == order.StoreId).ToList();
                                if (!availableStaff.Any())
                                {
                                    _logger.LogWarning($"No available staff for Order {order.OrderId} in Store {order.StoreId}");
                                    continue;
                                }

                                var assignedStaff = availableStaff[random.Next(availableStaff.Count)];
                                order.StaffId = assignedStaff.EmployeeId;
                                order.Status = "Arranging & Packing";
                                order.UpdateAt = DateTime.UtcNow;

                                _logger.LogInformation($"Order {order.OrderId} assigned to Staff {order.StaffId} and updated to 'Arranging & Packing'.");

                                // Tạo ChatRoom
                                var chatRoomRequest = new CreateChatRoomRequest { OrderId = order.OrderId };
                                var chatRoomResult = await chatRoomService.CreateChatRoom(chatRoomRequest);

                                if (chatRoomResult.ResultStatus != ResultStatus.Success.ToString())
                                {
                                    _logger.LogError($"Failed to create chat room for Order {order.OrderId}: {string.Join(", ", chatRoomResult.Messages)}");
                                }
                                else
                                {
                                    await unitOfWork.Repository<ChatRoom>().AddAsync(chatRoomResult.Data);
                                    _logger.LogInformation($"ChatRoom created for Order {order.OrderId}");
                                }
                            }

                            await unitOfWork.CompleteAsync();
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
