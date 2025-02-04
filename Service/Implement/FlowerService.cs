using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class FlowerService : IFlowerService
{
    private readonly IFlowerRepository _flowerRepository;
    private readonly IMapper _mapper;

    public FlowerService(IFlowerRepository flowerRepository, IMapper mapper)
    {
        _flowerRepository = flowerRepository;
        _mapper = mapper;
    }
    public async Task<List<FlowerResponse>> GetAllFlower()
    {
        var list = await _flowerRepository.GetAllFlower();
        return _mapper.Map<List<FlowerResponse>>(list);
    }

    public async Task<Result<Flower>> CreateFlower(CreateFlowerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FlowerName))
        {
            throw new ArgumentException("Flower name cannot be null or whitespace", nameof(request.FlowerName));
        }

        var newFlower = new Flower
        {
            FlowerName = request.FlowerName,
            Price = request.Price ?? 0,
            Image = request.Image,
            Quantity = request.Quantity ?? 0,
            CategoryId = request.CategoryId ?? Guid.Empty,
            Description = request.Description,
            StoreId = request.StoreId ?? Guid.Empty,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow
        };

        await _flowerRepository.AddFlower(newFlower);

        return new Result<Flower>
        {
            Data = newFlower,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Flower created successfully" }
        };
    }

    public async Task<Result<FlowerResponse>> UpdateFlower(Guid id, UpdateFlowerRequest request)
    {
        var flower = await _flowerRepository.GetFlowerById(id);
        if (flower == null)
        {
            throw new KeyNotFoundException("Cannot find flower");
        }
        if (request.Price.HasValue) flower.Price = request.Price.Value;
        if (!string.IsNullOrWhiteSpace(request.Image)) flower.Image = request.Image;
        if (request.Quantity.HasValue) flower.Quantity = request.Quantity.Value;
        if (request.CategoryId.HasValue) flower.CategoryId = request.CategoryId.Value;
        if (request.StoreId.HasValue) flower.StoreId = request.StoreId.Value;
        if (!string.IsNullOrWhiteSpace(request.Description)) flower.Description = request.Description;
        flower.UpdateAt = DateTime.UtcNow;
        await _flowerRepository.UpdateFlower(flower);
        return new Result<FlowerResponse>
        {
            Data = new FlowerResponse
            {
                FlowerId = flower.FlowerId,
                FlowerName = flower.FlowerName,
                Price = flower.Price,
                Image = flower.Image,
                Quantity = flower.Quantity,
                CategoryId = flower.CategoryId,
                StoreId = flower.StoreId,
                Description = flower.Description,
            },
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Update successful" }
        };
    }
    public async Task<Result<Flower>> DeleteFlower(Guid id)
    {
        var flower = await _flowerRepository.GetFlowerById(id);
    
        if (flower == null)
        {
            return new Result<Flower>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Flower not found."}
            };
        }

        await _flowerRepository.DeleteFlower(id);

        return new Result<Flower>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Flower deleted successfully."}
        };
    }

    public async Task<Result<FlowerResponse>> GetFlowerById(Guid id)
    {
        var response = new Result<FlowerResponse>();
        var flower = await _flowerRepository.GetFlowerById(id);
        if (flower == null)
        {
            response.Messages = ["Flower not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<FlowerResponse>(flower);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }

    public async Task<Result<FlowerResponse>>GetFlowerByName(string name)
    {
        var flower = await _flowerRepository.FindFlowerByName(name);

        if (flower == null)
        {
            return new Result<FlowerResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { "No flower found with the given name." }
            };
        }
        var flowerResponse = new FlowerResponse
        {
            FlowerId = flower.FlowerId,
            FlowerName = flower.FlowerName,
            Price = flower.Price,
            Image = flower.Image,
            Quantity = flower.Quantity,
            CategoryId = flower.CategoryId,
            StoreId = flower.StoreId,
            Description = flower.Description,
        };
        return new Result<FlowerResponse>
        {
            Data = flowerResponse,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Flower retrieved successfully" }
        };
    }

    public async Task<Result<List<FlowerResponse>>> GetFlowerByPrice(double minPrice, double? maxPrice)
    {
        var flowers = await _flowerRepository.FilterFlowersByPrice(minPrice, maxPrice);

        if (flowers == null || !flowers.Any())
        {
            return new Result<List<FlowerResponse>>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { "No flowers found within the given price range." }
            };
        }

        var flowerResponses = flowers.Select(flower => new FlowerResponse
        {
            FlowerId = flower.FlowerId,
            FlowerName = flower.FlowerName,
            Price = flower.Price,
            Image = flower.Image,
            Quantity = flower.Quantity,
            CategoryId = flower.CategoryId,
            StoreId = flower.StoreId,
            Description = flower.Description,
        }).ToList();

        return new Result<List<FlowerResponse>>
        {
            Data = flowerResponses,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Flowers retrieved successfully" }
        };
    }
}