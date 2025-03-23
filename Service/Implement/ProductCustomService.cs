using AutoMapper;
using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Repository.Implement;
using Repository.Interface;
using Service.Interface;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace Service.Implement;

public class ProductCustomService : IProductCustomService
{
    private readonly IProductCustomRepository _productCustomRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ImageService _imageService;

    public ProductCustomService(IProductCustomRepository productCustomRepository, IMapper mapper, IUnitOfWork unitOfWork, ImageService imageService)
    {
        _productCustomRepository = productCustomRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<IEnumerable<ProductCustomResponse>> GetAllProductCustom()
    {
        var productCustoms = await _unitOfWork.Repository<ProductCustom>().Entities
                                    .Include(n => n.FlowerBasket)
                                        .ThenInclude(fb => fb.Category)
                                    .Include(a => a.Style)
                                        .ThenInclude(s => s.Category)
                                    .Include(m => m.Accessory).ThenInclude(q => q.Category)
                                    .ToListAsync();

        var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                    .Include(fc => fc.Flower)
                                        .ThenInclude(f => f.Category)
                                    .ToListAsync();

        var productCustomResponse = productCustoms.Select(productCustom => new ProductCustomResponse
        {
            ProductCustomId = productCustom.ProductCustomId,
            ProductName = productCustom.ProductName,
            Quantity = productCustom.Quantity,
            TotalPrice = productCustom.TotalPrice,
            CustomerId = productCustom.CustomerId,
            CreateAt = productCustom.CreateAt,
            UpdateAt = productCustom.UpdateAt,
            Status = productCustom.Status,
            flowerBasketResponse = productCustom.FlowerBasket != null ? new FlowerBasketResponse
            {
                FlowerBasketId = productCustom.FlowerBasket.FlowerBasketId,
                FlowerBasketName = productCustom.FlowerBasket.FlowerBasketName,
                MaxQuantity = productCustom.FlowerBasket.MaxQuantity,
                MinQuantity = productCustom.FlowerBasket.MinQuantity,
                Quantity = productCustom.FlowerBasket.Quantity,
                Image = productCustom.FlowerBasket.Image,
                CategoryName = productCustom.FlowerBasket.Category?.CategoryName,
                Price = productCustom.FlowerBasket.Price,
                Decription = productCustom.FlowerBasket.Decription,
                Feature = productCustom.FlowerBasket.Feature,
                Status = productCustom.FlowerBasket.Status,
                Sold = productCustom.FlowerBasket.Sold,
                CreateAt = productCustom.FlowerBasket.CreateAt,
                UpdateAt = productCustom.FlowerBasket.UpdateAt,
            } : null,
            styleResponse = productCustom.Style != null ? new StyleResponse
            {
                StyleId = productCustom.Style.StyleId,
                Name = productCustom.Style.Name,
                Description = productCustom.Style.Description,
                Note = productCustom.Style.Note,
                CategoryName = productCustom.Style.Category?.CategoryName,
                Image = productCustom.Style.Image,
                CreateAt = productCustom.Style.CreateAt,
                UpdateAt = productCustom.Style.UpdateAt,
                Status = productCustom.Style.Status,
                Feature = productCustom.Style.Feature,
            } : null,
            accessoryResponse = productCustom.Accessory != null ? new AccessoryResponse
            {
                AccessoryId = productCustom.Accessory.AccessoryId,
                Name = productCustom.Accessory.Name,
                Note = productCustom.Accessory.Note,
                Price = productCustom.Accessory.Price,
                CategoryName = productCustom.Accessory.Category?.CategoryName,
                Description = productCustom.Accessory.Description,
                Image = productCustom.Accessory.Image,
                Status = productCustom.Accessory.Status,
                Feature= productCustom.Accessory.Feature,
            } : null,
            flowerCustomResponses = flowerCustoms
                                    .Where(a => a.ProductCustomId == productCustom.ProductCustomId)
                                    .Select(a => new FlowerCustomResponse
                                    {
                                        FlowerCustomId = a.FlowerCustomId,
                                        FlowerId = a.FlowerId,
                                        Quantity = a.Quantity,
                                        TotalPrice = a.Price,
                                        CreateAt = a.CreateAt,
                                        UpdateAt = a.UpdateAt,
                                        Status = a.Status,
                                        flowerResponse = a.Flower != null ? new FlowerResponse
                                        {
                                            FlowerId = a.Flower.FlowerId,
                                            FlowerName = a.Flower.FlowerName,
                                            Price = a.Flower.Price,
                                            Color = a.Flower.Color,
                                            Image = a.Flower.Image,
                                            Quantity = a.Flower.Quantity,
                                            CategoryName = a.Flower.Category?.CategoryName,
                                            Description = a.Flower.Description,
                                            Sold = a.Flower.Sold,
                                            Feature = a.Flower.Feature,
                                            Status = a.Flower.Status,
                                        } : null
                                    }).ToList()
        });

        return productCustomResponse;
    }
    public async Task<ProductCustomResponse> GetProductCustomById(Guid id)
    {
        var productCustom = await _unitOfWork.Repository<ProductCustom>().Entities
                                  .Include(n => n.FlowerBasket)
                                      .ThenInclude(fb => fb.Category)
                                  .Include(a => a.Style)
                                      .ThenInclude(s => s.Category)
                                  .Include(m => m.Accessory)
                                  .FirstOrDefaultAsync(n => n.ProductCustomId == id );

        var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                    .Include(fc => fc.Flower)
                                        .ThenInclude(f => f.Category)
                                    .ToListAsync();

        var productCustomResponse = new ProductCustomResponse
        {
            ProductCustomId = productCustom.ProductCustomId,
            ProductName = productCustom.ProductName,
            Quantity = productCustom.Quantity,
            TotalPrice = productCustom.TotalPrice,
            CustomerId = productCustom.CustomerId,
            CreateAt = productCustom.CreateAt,
            UpdateAt = productCustom.UpdateAt,
            Status = productCustom.Status,
            flowerBasketResponse = productCustom.FlowerBasket != null ? new FlowerBasketResponse
            {
                FlowerBasketId = productCustom.FlowerBasket.FlowerBasketId,
                FlowerBasketName = productCustom.FlowerBasket.FlowerBasketName,
                MaxQuantity = productCustom.FlowerBasket.MaxQuantity,
                MinQuantity = productCustom.FlowerBasket.MinQuantity,
                Quantity = productCustom.FlowerBasket.Quantity,
                Image = productCustom.FlowerBasket.Image,
                CategoryName = productCustom.FlowerBasket.Category?.CategoryName,
                Price = productCustom.FlowerBasket.Price,
                Decription = productCustom.FlowerBasket.Decription,
                Feature = productCustom.FlowerBasket.Feature,
                Status = productCustom.FlowerBasket.Status,
                Sold = productCustom.FlowerBasket.Sold,
                CreateAt = productCustom.FlowerBasket.CreateAt,
                UpdateAt = productCustom.FlowerBasket.UpdateAt,
            } : null,
            styleResponse = productCustom.Style != null ? new StyleResponse
            {
                StyleId = productCustom.Style.StyleId,
                Name = productCustom.Style.Name,
                Description = productCustom.Style.Description,
                Note = productCustom.Style.Note,
                CategoryName = productCustom.Style.Category?.CategoryName,
                Image = productCustom.Style.Image,
                CreateAt = productCustom.Style.CreateAt,
                UpdateAt = productCustom.Style.UpdateAt,
                Status = productCustom.Style.Status,
                Feature = productCustom.Style.Feature,
            } : null,
            accessoryResponse = productCustom.Accessory != null ? new AccessoryResponse
            {
                AccessoryId = productCustom.Accessory.AccessoryId,
                Name = productCustom.Accessory.Name,
                Note = productCustom.Accessory.Note,
                Price = productCustom.Accessory.Price,
                CategoryName = productCustom.Accessory.Category?.CategoryName,
                Description = productCustom.Accessory.Description,
                Image = productCustom.Accessory.Image,
                Status = productCustom.Accessory.Status,
                Feature = productCustom.Accessory.Feature,
            } : null,
            flowerCustomResponses = flowerCustoms
                                    .Where(a => a.ProductCustomId == productCustom.ProductCustomId)
                                    .Select(a => new FlowerCustomResponse
                                    {
                                        FlowerCustomId = a.FlowerCustomId,
                                        FlowerId = a.FlowerId,
                                        Quantity = a.Quantity,
                                        TotalPrice = a.Price,
                                        CreateAt = a.CreateAt,
                                        UpdateAt = a.UpdateAt,
                                        Status = a.Status,
                                        flowerResponse = a.Flower != null ? new FlowerResponse
                                        {
                                            FlowerId = a.Flower.FlowerId,
                                            FlowerName = a.Flower.FlowerName,
                                            Price = a.Flower.Price,
                                            Color = a.Flower.Color,
                                            Image = a.Flower.Image,
                                            Quantity = a.Flower.Quantity,
                                            CategoryName = a.Flower.Category?.CategoryName,
                                            Description = a.Flower.Description,
                                            Sold = a.Flower.Sold,
                                            Feature = a.Flower.Feature,
                                            Status = a.Flower.Status,
                                        } : null
                                    }).ToList()
        };
        return productCustomResponse;
    }
        public async Task<Result<ProductCustom>> CreateProductCustom(Guid customerId, CreateProductCustomRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            throw new ArgumentException("FlowerBasket name cannot be null or whitespace", nameof(request.ProductName));
        }

        var flowerBasket = await _unitOfWork.GetRepo<FlowerBasket>().GetByIdAsync(request.FlowerBasketId);
        var accessory = await _unitOfWork.GetRepo<Accessory>().GetByIdAsync(request.AccessoryId);

        var productCustom = new ProductCustom
        {
            ProductName = request.ProductName,
            CustomerId = customerId,
            Description = request.Description,
            Quantity = request.Quantity,
            FlowerBasketId = request.FlowerBasketId,
            StyleId = request.StyleId,
            AccessoryId = request.AccessoryId,
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now,
            Status = true // Added Status as noted in your comment
        };

        await _unitOfWork.Repository<ProductCustom>().AddAsync(productCustom);
        await _unitOfWork.CompleteAsync();

        if (request.createFlowerCustomRequests != null && request.createFlowerCustomRequests.Any())
        {
            var flowerCustoms = new List<FlowerCustom>();
            int flowerCustomIndex = 0;
            double totalFlowersPrice = 0;

            foreach (var flower in request.createFlowerCustomRequests)
            {
                var flowerById = await _unitOfWork.Repository<Flower>().GetByIdAsync(flower.FlowerId);
                flowerCustomIndex++;
                var flowerPrice = flowerById.Price * flower.Quantity;

                var flowerCustom = new FlowerCustom
                {
                    ProductCustomId = productCustom.ProductCustomId,
                    FlowerId = flower.FlowerId,
                    Price = flowerById.Price * flower.Quantity,
                    Quantity = flower.Quantity,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    Status = true,
                };

                totalFlowersPrice += flowerPrice ?? 0;
                flowerCustoms.Add(flowerCustom);
                Console.WriteLine($"Đã thêm hoa {flowerCustomIndex} vào list");
            }

            Console.WriteLine($"Tổng số hoa trong list: {flowerCustoms.Count}");
            productCustom.TotalPrice = ((flowerBasket?.Price ?? 0) + (accessory?.Price ?? 0) + totalFlowersPrice) * request.Quantity;

            await _unitOfWork.Repository<FlowerCustom>().AddRangeAsync(flowerCustoms);
            _unitOfWork.Repository<ProductCustom>().Update(productCustom);
            await _unitOfWork.CompleteAsync();
            Console.WriteLine("Đã lưu vào database");
        }
        else
        {
            // Set total price even if there are no flowers
            productCustom.TotalPrice = ((flowerBasket?.Price ?? 0) + (accessory?.Price ?? 0)) * request.Quantity;
            _unitOfWork.Repository<ProductCustom>().Update(productCustom);
            await _unitOfWork.CompleteAsync();
        }

        // Get a clean copy of the product without navigation properties loaded


        return new Result<ProductCustom>
        {
            Data = productCustom, // Include the entire productCustom object
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "ProductCustom created successfully" }
        };
    }
    public async Task<string> CreateImageProductCustom(Guid ProductCustomId)
    {
        var productCustom = await _unitOfWork.GetRepo<ProductCustom>()
            .Entities.Include(a => a.FlowerBasket)
            .Include(n => n.Style)
            .Include(n => n.Accessory)
            .FirstOrDefaultAsync(m => m.ProductCustomId == ProductCustomId);

        var flowerCustomList = await _unitOfWork.GetRepo<FlowerCustom>()
            .Entities.Include(m => m.Flower)
            .Where(a => a.ProductCustomId == productCustom.ProductCustomId)
            .ToListAsync();

        // Tạo danh sách hoa theo số lượng và tên
        string flowerDetails = string.Join(", ", flowerCustomList.Select(f => $"{f.Quantity} {f.Flower.FlowerName}"));

        // Chuỗi mô tả sản phẩm để gửi lên AI
        string description = $"I have one FlowerBasket like this image {productCustom.FlowerBasket.Image}. " +
                             $"Add {flowerDetails}. " +
                             $"Design follows the {productCustom.Style.Name} style. " +
                             $"Add accessories like this image: {productCustom.Accessory.Image}.";
        string image = await _imageService.GenerateImageAsync(description);
        return image;
    }

    public async Task<Result<ProductCustomResponse>> UpdateProductCustom(Guid id, UpdateProductCustomRequest request)
    {
        var productCustom = await _productCustomRepository.GetProductCustomById(id);
    
        if (productCustom == null)
        {
            return new Result<ProductCustomResponse>
            {
                ResultStatus = ResultStatus.Error.ToString(),
                Messages = new[] { "ProductCustom not found." }
            };
        }
        if (!string.IsNullOrWhiteSpace(request.ProductName))
            productCustom.ProductName = request.ProductName;

        if (request.FlowerBasketId.HasValue)
            productCustom.FlowerBasketId = request.FlowerBasketId.Value;

        if (!string.IsNullOrWhiteSpace(request.Description))
            productCustom.Description = request.Description;

        if (request.TotalPrice.HasValue)
            productCustom.TotalPrice = request.TotalPrice.Value;
        
        if (request.Quantity.HasValue)
            productCustom.Quantity = request.Quantity.Value;

        if (request.Status.HasValue)
            productCustom.Status = request.Status.Value;

        productCustom.UpdateAt = DateTime.UtcNow;

        await _productCustomRepository.UpdateProductCustom(productCustom);

        var response = new ProductCustomResponse()
        {
            ProductName = productCustom.ProductName,
            CustomerId = productCustom.CustomerId ?? Guid.Empty,
            TotalPrice = productCustom.TotalPrice ?? 0.0,  
            Description = productCustom.Description,
            Quantity = productCustom.Quantity ?? 0,
            Status = productCustom.Status ?? false,   
            CreateAt = productCustom.CreateAt ?? DateTime.UtcNow, 
            UpdateAt = productCustom.UpdateAt ?? DateTime.UtcNow  
        };

        return new Result<ProductCustomResponse>
        {
            Data = response,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "ProductCustom updated successfully" }
        };
    }

    public async Task<Result<ProductCustom>> DeleteProductCustom(Guid id)
    {
        var productCustom = await _productCustomRepository.GetProductCustomById(id);
        if (productCustom == null)
        {
            return new Result<ProductCustom>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new[] { "ProductCustom not found." }
            };
        }

        var flower =( await _unitOfWork.Repository<FlowerCustom>().GetAllAsync()).Where(n => n.ProductCustomId == productCustom.ProductCustomId);

        await _productCustomRepository.DeleteProductCustom(id);

        if (flower != null)
        {
             _unitOfWork.Repository<FlowerCustom>().DeleteRange(flower);
        }

        await _unitOfWork.CompleteAsync();

        return new Result<ProductCustom>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "ProductCustom deleted successfully." }
        };
    }

    
}