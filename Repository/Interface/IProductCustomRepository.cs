using BusinessObject.Entities;

namespace Repository.Interface;

public interface IProductCustomRepository
{
    Task<List<ProductCustom>> GetAllProductCustom();
    Task<ProductCustom> CreateProductCustom(ProductCustom productCustom);
    Task<ProductCustom> UpdateProductCustom(ProductCustom productCustom);
    Task<ProductCustom> DeleteProductCustom(Guid id);
    Task<ProductCustom> GetProductCustomById(Guid id);
}