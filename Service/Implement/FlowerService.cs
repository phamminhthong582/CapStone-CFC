using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.Product;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class FlowerService : IFlowerService
{
    private readonly IFlowerRepository _flowerRepository;
    private readonly IMapper _mapper;
    private readonly CloudinaryService _cloudinaryService;
   

    public FlowerService(IFlowerRepository flowerRepository, IMapper mapper, CloudinaryService cloudinaryService)
    {
        _flowerRepository = flowerRepository;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
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
        var folderName = $"flower/{request.FlowerName}";
        var flowerUrl = request.Image != null 
  ? await _cloudinaryService.UploadImageAsync(request.Image.OpenReadStream(), $"{folderName}")
  : null;
        var newFlower = new Flower
        {
            FlowerName = request.FlowerName,
            Price = request.Price ?? 0,
            Color = request.Color,
            Image = flowerUrl,
            Quantity = request.Quantity ?? 0,
            CategoryId = request.CategoryId ?? Guid.Empty,
            Description = request.Description,
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
        try
        {
            var flower = await _flowerRepository.GetFlowerById(id);
            if (flower == null)
            {
                return new Result<FlowerResponse>
                {
                    ResultStatus = ResultStatus.Error.ToString(),
                    Messages = new[] { "Cannot find flower" }
                };
            }

            // Cập nhật các trường nếu có giá trị mới
            if (request.Price.HasValue) flower.Price = request.Price.Value;
            if (request.Quantity.HasValue) flower.Quantity = request.Quantity.Value;
            if (request.CategoryId.HasValue) flower.CategoryId = request.CategoryId.Value;
            if (!string.IsNullOrWhiteSpace(request.FlowerName)) flower.FlowerName = request.FlowerName;
            if (!string.IsNullOrWhiteSpace(request.Description)) flower.Description = request.Description;
            if (!string.IsNullOrWhiteSpace(request.Color)) flower.Color = request.Color;

            // Xử lý upload ảnh
            string? imageUrl = flower.Image;
            if (request.Image != null)
            {
                var folderName = $"flower/{flower.FlowerId}";
                imageUrl = await _cloudinaryService.UploadImageAsync(
                    request.Image.OpenReadStream(),
                    folderName
                );
            }
            flower.Image = imageUrl;

            // Cập nhật thời gian
            flower.UpdateAt = DateTime.UtcNow;

            // Lưu thay đổi
            await _flowerRepository.UpdateFlower(flower);

            // Lấy category name cho response
            var categoryName = flower.Category?.CategoryName;

            // Tạo response
            return new Result<FlowerResponse>
            {
                Data = new FlowerResponse
                {
                    FlowerId = flower.FlowerId,
                    FlowerName = flower.FlowerName,
                    Price = flower.Price,
                    Image = flower.Image,
                    Quantity = flower.Quantity,
                    CategoryName = categoryName,
                    Color = flower.Color,
                    Description = flower.Description,
                    Sold = flower.Sold
                },
                ResultStatus = ResultStatus.Success.ToString(),
                Messages = new[] { "Update successful" }
            };
        }
        catch (Exception ex)
        {
            return new Result<FlowerResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { $"Update failed: {ex.Message}" }
            };
        }
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
            CategoryName = flower.Category.CategoryName,
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
            CategoryName = flower.Category.CategoryName,
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