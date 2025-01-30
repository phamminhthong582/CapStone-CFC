using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IFlowerBasketService
{
    Task<List<FlowerBasketResponse>> GetAllFlowerBasket();
    Task<Result<FlowerBasket>> CreateFlowerBasket(CreateFlowerBasketRequest request);
    Task<Result<FlowerBasketResponse>> UpdateFlowerBasket(Guid id, UpdateFlowerBasketRequest request);
    Task<Result<FlowerBasket>> DeleteFlower(Guid id);
    Task<Result<FlowerBasketResponse>> GetFlowerById(Guid id);
}