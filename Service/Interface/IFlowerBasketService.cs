using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IFlowerBasketService
{
    Task<List<FlowerBasketResponse>> GetAllFlowerBasket();
    Task<PaginationResponse<FlowerBasketResponse>>GetAllFlowerBasketPagination(int pageNumber, int pageSize);
    Task<Result<FlowerBasket>> CreateFlowerBasket(CreateFlowerBasketRequest request);
    Task<Result<FlowerBasketResponse>> UpdateFlowerBasket(Guid id, UpdateFlowerBasketRequest request);
    Task<Result<FlowerBasket>> DeleteFlowerBasket(Guid id);
    Task<Result<FlowerBasketResponse>> GetFlowerBasketById(Guid id);
}