using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IProductCustomService
{
    Task<IEnumerable<ProductCustomResponse>> GetAllProductCustom();

    Task<Result<ProductCustom>> CreateProductCustom(Guid CustomerId, CreateProductCustomRequest request);
    Task<Result<ProductCustomResponse>> UpdateProductCustom(Guid id, UpdateProductCustomRequest request);
    Task<string> CreateImageProductCustom(Guid ProductCustomId);
    Task<Result<ProductCustom>> DeleteProductCustom(Guid id);
    Task<ProductCustomResponse> GetProductCustomById(Guid id);
}