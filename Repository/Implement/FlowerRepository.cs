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
    public async Task<List<Flower>> GetAllFlower()
    {
        var list = await _context.Flowers.ToListAsync();
        return list;
    }

    public async Task<Flower> GetFlowerById(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Flower> AddFlower(Flower flower)
    {
        throw new NotImplementedException();
    }

    public async Task<Flower> UpdateFlower(Flower flower)
    {
        throw new NotImplementedException();
    }

    public async Task<Flower> DeleteFlower(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Flower> FindFlowerByName(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<Flower> FilterFlowerByPrice(double price)
    {
        throw new NotImplementedException();
    }
}