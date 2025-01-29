using BusinessObject.Entities;

namespace Repository.Interface;

public interface IFlowerRepository
{
    Task<List<Flower?>> GetAllFlower();
    Task<Flower?> GetFlowerById(Guid id);
    Task<Flower?> AddFlower(Flower flower);
    Task<Flower?> UpdateFlower(Flower flower);
    Task<Flower?> DeleteFlower(Guid id);
    Task<Flower?> FindFlowerByName(string name);
    Task<List<Flower>> FilterFlowersByPrice(double minPrice, double? maxPrice);
}