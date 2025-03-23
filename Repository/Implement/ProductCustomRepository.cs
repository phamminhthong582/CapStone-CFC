using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class ProductCustomRepository : IProductCustomRepository
{
    private readonly CustomFlowerChainContext _context;

    public ProductCustomRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<ProductCustom>> GetAllProductCustom()
    {
        var list = await _context.ProductCustoms.ToListAsync();
        return list; 
    }

    public async Task<ProductCustom> CreateProductCustom(ProductCustom productCustom)
    {
        await _context.ProductCustoms.AddAsync(productCustom);
        await _context.SaveChangesAsync();
        return productCustom;
    }

    public async Task<ProductCustom> UpdateProductCustom(ProductCustom productCustom)
    {
        _context.ProductCustoms.Update(productCustom);
        await _context.SaveChangesAsync();
        return productCustom;
    }

    public async Task<ProductCustom> DeleteProductCustom(Guid id)
    {
        var productCustom = await _context.ProductCustoms.FirstOrDefaultAsync(x => x.ProductCustomId == id);
        if (productCustom == null)
        {
            return null;
        }
        _context.ProductCustoms.Remove(productCustom);
        await _context.SaveChangesAsync();
        return productCustom;
    }

    public async Task<ProductCustom> GetProductCustomById(Guid id)
    {
        var productCustom = await _context.ProductCustoms.FirstOrDefaultAsync(x => x.ProductCustomId == id);
        return productCustom;
    }

    public async Task<int> CountProductCustomsAsync()
    {
        return await _context.ProductCustoms.CountAsync();
    }

    public async Task<List<ProductCustom>> GetProductCustomsPaginatedAsync(int pageNumber, int pageSize)
    {
        return await _context.ProductCustoms
            .Skip((pageNumber - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync(); 
    }
}