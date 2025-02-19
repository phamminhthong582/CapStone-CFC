using Repository.Interface;

namespace Service.Implement;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class PromotionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PromotionBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var promotionRepository = scope.ServiceProvider.GetRequiredService<IPromotionRepository>();
                await UpdateExpiredPromotions(promotionRepository);
            }

            // Delay để chạy mỗi 1 giờ
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
    private async Task UpdateExpiredPromotions(IPromotionRepository promotionRepository)
    {
        var expiredPromotions = await promotionRepository.GetExpiredPromotions();
        foreach (var promotion in expiredPromotions)
        {
            if (promotion.EndDate <= DateTime.UtcNow && promotion.Status == true)
            {
                promotion.Status = false;  
                await promotionRepository.UpdatePromotion(promotion); 
            }
        }
    }
}
