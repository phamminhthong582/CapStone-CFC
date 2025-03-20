using BusinessObject.Entities;

namespace Repository.Interface;

public interface IPromotionRepository
{
    Task<List<Promotion>> GetAllPromotion();
    Task<IEnumerable<Promotion>> GetExpiredPromotions();
    Task<Promotion> GetPromotionById(Guid id);
    Task<Promotion> AddPromotion(Promotion promotion);
    Task<Promotion> UpdatePromotion(Promotion promotion);
    Task<Promotion> DeletePromotion(Guid id);
    Task<int> CountPromotionsAsync();
    Task<List<Promotion>> GetPromotionPaginatedAsync(int pageNumber, int pageSize);
}