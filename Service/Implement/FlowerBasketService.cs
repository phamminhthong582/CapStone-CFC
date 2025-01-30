using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class FlowerBasketService : IFlowerBasketService
{
    private readonly IFlowerBasketRepository _flowerBasketRepository;
    private readonly IMapper _mapper;

    public FlowerBasketService(IFlowerBasketRepository flowerBasketRepository, IMapper mapper)
    {
        _flowerBasketRepository = flowerBasketRepository;
        _mapper = mapper;
    }
    public async Task<List<FlowerBasketResponse>> GetAllFlowerBasket()
    {
        var list = await _flowerBasketRepository.GetAllFlowerBasket();
        return _mapper.Map<List<FlowerBasketResponse>>(list);
    }

    public async Task<Result<FlowerBasket>> CreateFlowerBasket(CreateFlowerBasketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FlowerBasketName))
        {
            throw new ArgumentException("FlowerBasket name cannot be null or whitespace", nameof(request.FlowerBasketName));
        }

        var newFlowerBasket = new FlowerBasket
        {
            FlowerBasketName = request.FlowerBasketName,
            StoreId = request.StoreId ?? Guid.Empty,
            Decription = request.Description,
            Feature = request.Feature,
            Price = request.Price,
            Quantity = request.Quantity
            
        };

        await _flowerBasketRepository.CreateFlowerBasket(newFlowerBasket);

        return new Result<FlowerBasket>
        {
            Data = newFlowerBasket,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "FlowerBasket created successfully" }
        };
    }

    public async Task<Result<FlowerBasketResponse>> UpdateFlowerBasket(Guid id, UpdateFlowerBasketRequest request)
    {
        var flowerBasket = await _flowerBasketRepository.GetFlowerBasketById(id);
    
        if (flowerBasket == null)
        {
            return new Result<FlowerBasketResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { "FlowerBasket not found." }
            };
        }
        if (!string.IsNullOrWhiteSpace(request.FlowerBasketName))
            flowerBasket.FlowerBasketName = request.FlowerBasketName;

        if (request.StoreId.HasValue)
            flowerBasket.StoreId = request.StoreId.Value;

        if (!string.IsNullOrWhiteSpace(request.Description))
            flowerBasket.Decription = request.Description;

        if (request.Feature.HasValue)
            flowerBasket.Feature = request.Feature.Value;

        if (request.Status.HasValue)
            flowerBasket.Status = request.Status.Value;

        flowerBasket.UpdateAt = DateTime.UtcNow;

        await _flowerBasketRepository.UpdateFlowerBasket(flowerBasket);

        var response = new FlowerBasketResponse
        {
            FlowerBasketId = flowerBasket.FlowerBasketId,
            FlowerBasketName = flowerBasket.FlowerBasketName,
            Price = flowerBasket.Price ?? 0.0,  
            StoreId = flowerBasket.StoreId ?? Guid.Empty, 
            Description = flowerBasket.Decription,
            Feature = flowerBasket.Feature ?? false, 
            Status = flowerBasket.Status ?? false,   
            CreateAt = flowerBasket.CreateAt ?? DateTime.UtcNow, 
            UpdateAt = flowerBasket.UpdateAt ?? DateTime.UtcNow  
        };

        return new Result<FlowerBasketResponse>
        {
            Data = response,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Flowerbasket updated successfully" }
        };
    }

    public async Task<Result<FlowerBasket>> DeleteFlowerBasket(Guid id)
    {
        var flowerBasket = await _flowerBasketRepository.GetFlowerBasketById(id);
    
        if (flowerBasket == null)
        {
            return new Result<FlowerBasket>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"FlowerBasket not found."}
            };
        }

        await _flowerBasketRepository.DeleteFlowerBasket(id);

        return new Result<FlowerBasket>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"FlowerBasket deleted successfully."}
        };
    }

    public async Task<Result<FlowerBasketResponse>> GetFlowerBasketById(Guid id)
    {
        var response = new Result<FlowerBasketResponse>();
        var flowerBasket = await _flowerBasketRepository.GetFlowerBasketById(id);
        if (flowerBasket == null)
        {
            response.Messages = ["FlowerBasket not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<FlowerBasketResponse>(flowerBasket);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }
}