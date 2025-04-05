using BusinessObject.Entities;

namespace Repository.Interface;

public interface IFlowerCustomRepository
{
    Task<List<FlowerCustom>> GetAllFlowerCustom();
    Task<string> GetFlowerCustomInfoAsync(string flowerName);
    Task<FlowerCustom> CreateFlowerCustom(FlowerCustom flowerCustom);
    Task<FlowerCustom> UpdateFlowerCustom(FlowerCustom flowerCustom);
    Task<FlowerCustom> DeleteFlowerCustom(Guid id);
    Task<FlowerCustom> GetFlowerCustomById(Guid id);
    Task<int> CountFlowersCustomAsync();
    Task<List<FlowerCustom>> GetFlowerCustomPaginatedAsync(int pageNumber, int pageSize);
    Task<bool> ExistsFlowers(List<Guid> flowerIds);
    Task<bool> ExistsProductCustom(Guid productCustomId);
}