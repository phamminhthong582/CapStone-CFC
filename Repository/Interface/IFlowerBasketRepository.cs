using BusinessObject.Entities;

namespace Repository.Interface;

public interface IFlowerBasketRepository
{
    Task<List<FlowerBasket>> GetAllFlowerBasket();
    Task<FlowerBasket> CreateFlowerBasket(FlowerBasket flowerBasket);
    Task<FlowerBasket> UpdateFlowerBasket(FlowerBasket flowerBasket);
    Task<FlowerBasket> DeleteFlowerBasket(Guid id);
    Task<FlowerBasket> GetFlowerBasketById(Guid id);
}