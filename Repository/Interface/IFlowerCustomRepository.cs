using BusinessObject.Entities;

namespace Repository.Interface;

public interface IFlowerCustomRepository
{
    Task<List<FlowerCustom>> GetAllFlowerCustom();
    Task<FlowerCustom> CreateFlowerCustom(FlowerCustom flowerCustom);
    Task<FlowerCustom> UpdateFlowerCustom(FlowerCustom flowerCustom);
    Task<FlowerCustom> DeleteFlowerCustom(Guid id);
    Task<FlowerCustom> GetFlowerCustomById(Guid id);
    Task<bool> ExistsFlowers(List<Guid> flowerIds);
    Task<bool> ExistsProductCustom(Guid productCustomId);
}