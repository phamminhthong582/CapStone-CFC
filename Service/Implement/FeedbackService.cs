using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FeedBack;
using BusinessObject.Entities;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Core.Infrastructures;
using Repository.Interface;
using Service.Interface;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http.HttpResults;

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
            throw new ArgumentException("Order cannot be null or whitespace");
        }
        if (customer == null)
        {
            throw new ArgumentException("Customer cannot be null or whitespace");
        }

        string? feedBackVideoUrl = null;

        if (request.FeedBackVideoByCustomer != null)
        {
            var folderName = $"flowerBasket/videos/{Guid.NewGuid()}";
            feedBackVideoUrl = await _cloudinaryService.UploadVideoAsync(
                request.FeedBackVideoByCustomer.OpenReadStream(), folderName);
        }

        var newFeedback = new Feedback
        {
            StoreId = order.StoreId,
            CustomerId = customerId,
            OrderId = orderId,
            FeedbackByCustomer = request.FeedbackByCustomer,
            RequestRefundByCustomer = request.RequestRefundByCustomer,
            Rating = request.Rating,
            Status = "Sent By Customer",
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now,
            FeedBackVideoByCustomer = feedBackVideoUrl // Lưu URL video vào database
            
        };
        if(request.RequestRefundByCustomer == true)
        {
            order.Status = "Request refund";
            _unitOfWork.GetRepo<Order>().Update(order);
        }
       

        await _unitOfWork.GetRepo<Feedback>().AddAsync(newFeedback);
        await _unitOfWork.CompleteAsync();
    }



    public async Task UpdateFeedbackByStoreID(Guid feedbackId, CreateFeedbackByStoreRequest request)
    {
        try
        {
            var feedback = await _unitOfWork.Repository<Feedback>().GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                throw new ArgumentException("Feedback not found");
            }
            feedback.UpdateAt = DateTime.Now;
            feedback.ResponseFeedBackStore = request.ResponseFeedBackStore;
            feedback.Status = "Reply by store";
            _unitOfWork.Repository<Feedback>().Update(feedback);
            await _unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating feedback: {ex.Message}", ex);
        }
    }

    public async Task UpdateStatusFeedback(Guid OrderId, string status)
    {
        var Order = await _unitOfWork.GetRepo<Order>().Entities.FirstOrDefaultAsync(n => n.OrderId == OrderId);
        var wallet = await _unitOfWork.GetRepo<Wallet>().Entities.FirstOrDefaultAsync(n => n.CustomerId == Order.CustomerId);
        Order.Status = status;
        _unitOfWork.Repository<Order>().Update(Order);

        if (status == "Accept refund")
        {
            wallet.TotalPrice += Order.OrderPrice;
            _unitOfWork.Repository<Wallet>().Update(wallet);

            var refund = new Refund
            {
                OrderId = Order.OrderId,
                StoreId = Order.StoreId,
                Price = Order.OrderPrice,
                WallerId = wallet.WalletId,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Status = "Refund Order",
            };
             await _unitOfWork.Repository<Refund>().AddAsync(refund);
            var incomeWallet = new IncomeWallet
            {
                WalletID = wallet.WalletId,
                IncomePrice = Order.OrderPrice,
                Method = "Refund",
                Status = "Successfull",
                CreateAt =DateTime.Now,
                UpdateAt =DateTime.Now,
                OrderId = Order.OrderId,
            };
            await _unitOfWork.Repository<IncomeWallet>().AddAsync(incomeWallet);
        }
        await _unitOfWork.CompleteAsync();
    }
    public async Task<FeedbackResponse> GetFeedBackByOrderId(Guid orderId)
    {
        var order = await _unitOfWork.GetRepo<Order>().GetByIdAsync(orderId);

        if (order == null)
        {
            throw new ArgumentException("Order không tồn tại");
        }

        var feedback = await _unitOfWork.GetRepo<Feedback>()
            .Entities.FirstOrDefaultAsync(f => f.OrderId == orderId);

        if (feedback == null)
        {
            throw new ArgumentException("Không tìm thấy phản hồi cho đơn hàng này");
        }

        return new FeedbackResponse
        {
            FeedbackId = feedback.FeedbackId,
            CustomerId = feedback.CustomerId,
            OrderId = feedback.OrderId,
            FeedbackByCustomer = feedback.FeedbackByCustomer,
            RequestRefundByCustomer = feedback.RequestRefundByCustomer,
            Rating = feedback.Rating,
            FeedBackVideoByCustomer = feedback.FeedBackVideoByCustomer,
            ResponseFeedBackStore = feedback.ResponseFeedBackStore,
            Status = feedback.Status,
            CreateAt = feedback.CreateAt,
            UpdateAt = feedback.UpdateAt
        };
    }

    public async Task<bool> CheckFeedBack(Guid orderId)
    {
        // Kiểm tra xem Order có tồn tại không
        var order = await _unitOfWork.GetRepo<Order>().GetByIdAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException("Order không tồn tại");
        }

        // Kiểm tra xem có Feedback nào thuộc OrderId này hay không
        var feedbackExists = await _unitOfWork.GetRepo<Feedback>()
            .Entities.FirstOrDefaultAsync(f => f.OrderId == orderId);

        if (feedbackExists != null)
        {
            return true;
        }
        return false;
    }

 
}