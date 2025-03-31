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

    public async Task<string> GetProductCustomInfoAsync(string productName)
    {
        try
        {
            
            var productCustom = await _context.ProductCustoms
                .Where(pc => pc.ProductName.Contains(productName)) 
                .FirstOrDefaultAsync(); 
            if (productCustom != null)
            {
                return $"Product: {productCustom.ProductName}, " +
                       $"Description: {productCustom.Description}, " +
                       $"Quantity: {productCustom.Quantity}, " +
                       $"Price: {productCustom.TotalPrice} VND.";
            }
            return $"Sorry, we couldn't find any product with the name '{productName}'.";
        }
        catch (Exception ex)
        {
            return $"Error processing request: {ex.Message}";
        }
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