using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;

namespace Service.Interface;

public interface ICommentService
{
    Task<List<CommentResponse>> GetAllComment();
    Task<PaginationResponse<CommentResponse>>GetAllCommentPagination(int pageNumber, int pageSize);
    Task<Result<CommentResponse>> CreateComment(CreateCommentRequest request);
    Task<List<CommentResponse>> GetCommentByProductId(Guid productId);
}
