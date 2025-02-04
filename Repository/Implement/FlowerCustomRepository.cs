using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class FlowerCustomRepository : IFlowerCustomRepository
{
    private readonly CustomFlowerChainContext _context;

    public FlowerCustomRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<FlowerCustom>> GetAllFlowerCustom()
    {
        var list = await _context.FlowerCustoms.ToListAsync();
        return list;
    }

    public async Task<FlowerCustom> CreateFlowerCustom(FlowerCustom flowerCustom)
    {
        await _context.FlowerCustoms.AddAsync(flowerCustom);
        await _context.SaveChangesAsync();
        return flowerCustom;
    }

    public async Task<FlowerCustom> UpdateFlowerCustom(FlowerCustom flowerCustom)
    {
        _context.FlowerCustoms.Update(flowerCustom);
        await _context.SaveChangesAsync();
        return flowerCustom;
    }

    public async Task<FlowerCustom> DeleteFlowerCustom(Guid id)
    {
        var flowerCustom = await _context.FlowerCustoms.FirstOrDefaultAsync(x => x.FlowerCustomId == id);
        if (flowerCustom == null)
        {
            return null;
        }
        _context.FlowerCustoms.Remove(flowerCustom);
        await _context.SaveChangesAsync();
        return flowerCustom;
    }

    public async Task<FlowerCustom> GetFlowerCustomById(Guid id)
    {
        var flowerCustom = await _context.FlowerCustoms.FirstOrDefaultAsync(x => x.FlowerCustomId == id);
        return flowerCustom;
    }

    public async Task<bool> ExistsFlower(Guid flowerId)
    {
        return await _context.Flowers.AnyAsync(f => f.FlowerId == flowerId);
    }

    public async Task<bool> ExistsProductCustom(Guid productCustomId)
    {
        return await _context.ProductCustoms.AnyAsync(p => p.ProductCustomId == productCustomId);
    }
}