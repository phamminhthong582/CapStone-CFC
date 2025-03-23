using BusinessObject.Entities;

namespace Repository.Interface;

public interface ICommentRepository
{
    Task<List<Comment>> GetAllComment();
    Task<Comment> AddComment(Comment comment);
    Task<List<Comment>> GetCommentByProductId(Guid productId);
    Task<int> CountCommentsAsync();
    Task<List<Comment>> GetCommentPaginatedAsync(int pageNumber, int pageSize);
}