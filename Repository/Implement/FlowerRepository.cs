using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class FlowerRepository : IFlowerRepository
{
    private readonly CustomFlowerChainContext _context;

    public FlowerRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<Flower?>> GetAllFlower()
    {
        var list = await _context.Flowers.Include(f => f.Category).ToListAsync();
        return list;
    }

    public async Task<Flower?> GetFlowerById(Guid id)
    {
        var flower = await _context.Flowers.FirstOrDefaultAsync(x => x.FlowerId == id);
        return flower;
    }

    public async Task<Flower?> AddFlower(Flower flower)
    {
        await _context.Flowers.AddAsync(flower);
        await _context.SaveChangesAsync();
        return flower;
    }

    public async Task<Flower?> UpdateFlower(Flower flower)
    {
        _context.Flowers.Update(flower);
        await _context.SaveChangesAsync();
        return flower;
    }

    public async Task<Flower?> DeleteFlower(Guid id)
    {
        var flower = await _context.Flowers.FirstOrDefaultAsync(x => x.FlowerId == id);
        if (flower == null)
        {
            return null;
        }
        _context.Flowers.Remove(flower);
        await _context.SaveChangesAsync();
        return flower;
    }

    public async Task<Flower?> FindFlowerByName(string name)
    {
        var flower = await _context.Flowers.FirstOrDefaultAsync(x => x. FlowerName== name);
        return flower;
    }

    public async Task<List<Flower>> FilterFlowersByPrice(double minPrice, double? maxPrice)
    {
        return await _context.Flowers
            .Where(flower => flower.Price >= minPrice && (maxPrice == null || flower.Price <= maxPrice))
            .ToListAsync();
    }
}