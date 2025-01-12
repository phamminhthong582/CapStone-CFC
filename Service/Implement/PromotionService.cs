using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Promotion;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private IMapper _mapper;
    public  PromotionService(IPromotionRepository promotionRepository , IMapper mapper)
    {
        _promotionRepository = promotionRepository;
        _mapper = mapper;
        
    }
    public async Task<List<PromotionResponse>> GetAllPromotion()
    {
        var list = await _promotionRepository.GetAllPromotion();
        return _mapper.Map<List<PromotionResponse>>(list);
    }

    public async Task<Result<Promotion>> CreatePromotion(CreatePromotionRequest request)
    {
        if (string.IsNullOrEmpty(request.PromotionName))
        {
            return new Result<Promotion>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new [] {"Promotion name is required"}
            };
        }
        if (request.StartDate == null || request.EndDate == null)
        {
            return new Result<Promotion>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Start date and end date are required"}
            };
        }
        if (request.StartDate >= request.EndDate)
        {
            return new Result<Promotion>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new [] {"Start date must be earlier than end date"}
            };
        }
        bool status = DateTime.UtcNow <= request.EndDate;
        var promotion = new Promotion
        {
            PromotionName = request.PromotionName,
            Quantity = request.Quantity,
            PromotionDiscount = request.PromotionDiscount,
            PromotionCode = request.PromotionCode,
            StartDate = request.StartDate.Value,
            EndDate = request.EndDate.Value,
            CreateAt = DateTime.UtcNow,
            Status = status, 
        };
        await _promotionRepository.AddPromotion(promotion);
        return new Result<Promotion>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Promotion created successfully"},
            Data = promotion
        };
    }

    public async Task<Result<PromotionResponse>> UpdatePromotion(Guid id,UpdatePromotionRequest request)
    {
        var promotion = await _promotionRepository.GetPromotionById(id);
        if (promotion == null)
        {
            return new Result<PromotionResponse>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Promotion not found"} 
            };
        }
        if (request.Quantity.HasValue)
        {
            promotion.Quantity = request.Quantity.Value;
        }

        if (request.PromotionDiscount.HasValue)
        {
            promotion.PromotionDiscount = request.PromotionDiscount.Value;
        }
        if (promotion.EndDate <= DateTime.UtcNow)
        {
            promotion.Status = false;
        }
        await _promotionRepository.UpdatePromotion(promotion);

        return new Result<PromotionResponse>
        {
            Data = new PromotionResponse
            {
                PromtionId = promotion.PromtionId,
                Quantity = promotion.Quantity,
                PromotionDiscount = promotion.PromotionDiscount
            },
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Update successfully" }
        };
    }

    public async Task<Result<Promotion>> DeletePromotion(Guid id)
    {
        var promotion = await _promotionRepository.GetPromotionById(id);
    
        if (promotion == null)
        {
            return new Result<Promotion>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Promotion not found."}
            };
        }

        await _promotionRepository.DeletePromotion(id);

        return new Result<Promotion>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Promotion deleted successfully."}
        };
    }
}