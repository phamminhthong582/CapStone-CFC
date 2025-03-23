using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class CommentRepository : ICommentRepository
{
    private readonly CustomFlowerChainContext _context;

    public CommentRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<Comment>> GetAllComment()
    {
        var list = await _context.Comments.ToListAsync();
        return list;
    }
    

    public async Task<Comment> AddComment(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<List<Comment>> GetCommentByProductId(Guid productId)
    {
        return await _context.Comments
            .Where(c => c.ProductId == productId)
            .Include(c => c.Customer) 
            .ToListAsync();
    }

    public async Task<int> CountCommentsAsync()
    {
        return await _context.Comments.CountAsync();
    }

    public async Task<List<Comment>> GetCommentPaginatedAsync(int pageNumber, int pageSize)
    {
        return await _context.Comments
            .Skip((pageNumber - 1) * pageSize) 
            .Take(pageSize) 
            .ToListAsync(); 
    }
}