using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;
using BusinessObject.Entities;
using Core.Infrastructures;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class FeedbackService : IFeedbackService
{
    private IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CloudinaryService _cloudinaryService;

    public FeedbackService(IMapper mapper, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public Task<List<FeedbackResponse>> GetAllFeedback()
    {
        throw new NotImplementedException();
    }

    public async Task CreateFeedbackByCustomer(Guid customerId, Guid orderId, CreateFeedbackRequest request)
    {
        var order = await _unitOfWork.GetRepo<Order>().GetByIdAsync(orderId);
        var customer = await _unitOfWork.GetRepo<Customer>().GetByIdAsync(customerId);
        if (order == null)
        {
            throw new ArgumentException("order name cannot be null or whitespace");
        }
        if (customer == null)
        {
            throw new ArgumentException("customer cannot be null or whitespace");
        }
        var folderName = $"flowerBasket/{request.FeedBackImageByCustomer}";
        var FeedBackImageUrl = request.FeedBackImageByCustomer != null
? await _cloudinaryService.UploadImageAsync(request.FeedBackImageByCustomer.OpenReadStream(), $"{folderName}")
: null;
        var newFeedBackByCustomer = new Feedback
        {
            CustomerId = customerId,
            OrderId = orderId,
            FeedbackByCustomer = request.FeedbackByCustomer ,
            FeedBackImageByCustomer = FeedBackImageUrl,
            RequestRefundByCustomer = request.RequestRefundByCustomer ,
            Rating = request.Rating,
            Status = "Send By Customer",
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now,

        };

    }

    public async Task UpdateFeedbackByStoreID(Guid storeID, Guid feedbackId, CreateFeedbackByStoreRequest request)
    {
        var feedback = await _unitOfWork.GetRepo<Feedback>().GetByIdAsync(feedbackId);
        if (feedback == null)
        {
            throw new ArgumentException("order name cannot be null or whitespace");
        }
        feedback.UpdateAt = DateTime.Now;
        feedback.StoreId = storeID;
        feedback.ResponseFeedBackStore = request.ResponseFeedBackStore;
        feedback.ResponseFeedImageByStore = request.ResponseFeedImageByStore;
        _unitOfWork.GetRepo<Feedback>().Update(feedback);
        await _unitOfWork.CompleteAsync();
    }

    public Task<FeedbackResponse> GetFeedBackByOrderId(Guid OrderId)
    {
        throw new NotImplementedException();
    }
}