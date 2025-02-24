using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.Entities;

namespace Service.Interface;

public interface ICommentService
{
    Task<List<CommentResponse>> GetAllComment();
    Task<Result<CommentResponse>> CreateComment(CreateCommentRequest request);
    Task<List<CommentResponse>> GetCommentByProductId(Guid productId);
}
