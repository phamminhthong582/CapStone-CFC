using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IFlowerCustomService
{
    Task<PaginationResponse<FlowerCustomResponse>> GetAllFlowerCustom(int pageNumber, int pageSize);
    Task<Result<FlowerCustom>> CreateFlowerCustom(CreateFlowerCustomRequest request);
    Task<Result<FlowerCustomResponse>> UpdateFlowerCustom(Guid id, UpdateFlowerCustomRequest request);
    Task<Result<FlowerCustom>> DeleteFlowerCustom(Guid id);
    Task<Result<FlowerCustomResponse>> GetFlowerCustomById(Guid id);
}