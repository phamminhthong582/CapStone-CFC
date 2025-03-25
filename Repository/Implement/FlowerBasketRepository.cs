using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class FlowerBasketRepository : IFlowerBasketRepository
{
    private readonly CustomFlowerChainContext _context;

    public FlowerBasketRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<FlowerBasket>> GetAllFlowerBasket()
    {
        var list = await _context.FlowerBaskets.Include(f => f.Category).ToListAsync();
        return list; 
    }

    public async Task<FlowerBasket> CreateFlowerBasket(FlowerBasket flowerBasket)
    {
        await _context.FlowerBaskets.AddAsync(flowerBasket);
        await _context.SaveChangesAsync();
        return flowerBasket;
    }

    public async Task<FlowerBasket> UpdateFlowerBasket(FlowerBasket flowerBasket)
    {
        _context.FlowerBaskets.Update(flowerBasket);
        await _context.SaveChangesAsync();
        return flowerBasket;
    }

    public async Task<FlowerBasket> DeleteFlowerBasket(Guid id)
    {
        var flowerBasket = await _context.FlowerBaskets.FirstOrDefaultAsync(x => x.FlowerBasketId == id);
        if (flowerBasket == null)
        {
            return null;
        }
        _context.FlowerBaskets.Remove(flowerBasket);
        await _context.SaveChangesAsync();
        return flowerBasket;
    }

    public async Task<FlowerBasket> GetFlowerBasketById(Guid id)
    {
        var flowerBasket = await _context.FlowerBaskets.FirstOrDefaultAsync(x => x.FlowerBasketId == id);
        return flowerBasket;
    }

    public async Task<string> GetFlowerBasketInfoAsync(string basketName)
    {
        var flowerBasket = await _context.FlowerBaskets
            .Where(fb => fb.FlowerBasketName.Contains(basketName))
            .FirstOrDefaultAsync();

        if (flowerBasket != null)
        {
            return $"Flower Basket: {flowerBasket.FlowerBasketName}, Price: {flowerBasket.Price} VND, Quantity: {flowerBasket.Quantity}.";
        }

        return "Sorry, we couldn't find any information about that flower basket.";
    }

    public async Task<int> CountFlowerBasketAsync()
    {
        return await _context.FlowerBaskets.CountAsync();
    }

    public async Task<List<FlowerBasket>> GetFlowerBasketPaginatedAsync(int pageNumber, int pageSize)
    {
        return await _context.FlowerBaskets
            .Skip((pageNumber - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync(); 
    }
}