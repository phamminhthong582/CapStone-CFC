using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;

namespace Service.Interface;

public interface IFeedbackService
{
    Task<List<FeedbackResponse>> GetAllFeedback();
    Task CreateFeedbackByCustomer(Guid customerId,Guid orderId,CreateFeedbackRequest request);
    Task UpdateFeedbackByStoreID(Guid feedbackId, CreateFeedbackByStoreRequest request);
    Task UpdateStatusFeedback(Guid OrderId, string status);

    Task<FeedbackResponse> GetFeedBackByOrderId(Guid OrderId);
    Task<bool> CheckFeedBack(Guid OrderId);

}