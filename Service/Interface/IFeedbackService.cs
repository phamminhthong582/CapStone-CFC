using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;
using BusinessObject.DTO.Pagination;

namespace Service.Interface;

public interface IFeedbackService
{
    Task<List<FeedbackResponse>> GetAllFeedback();
    Task<Result<FeedbackResponse>> CreateFeedback(CreateFeedbackRequest request);
    Task<PaginationResponse<FeedbackResponse>>GetAllFeedbackPagination(int pageNumber, int pageSize);
}