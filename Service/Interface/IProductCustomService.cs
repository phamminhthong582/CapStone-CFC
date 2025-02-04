using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IProductCustomService
{
    Task<List<ProductCustomResponse>> GetAllProductCustom();
    Task<Result<ProductCustom>> CreateProductCustom(CreateProductCustomRequest request);
    Task<Result<ProductCustomResponse>> UpdateProductCustom(Guid id, UpdateProductCustomRequest request);
    Task<Result<ProductCustom>> DeleteProductCustom(Guid id);
    Task<Result<ProductCustomResponse>> GetProductCustomById(Guid id);
}