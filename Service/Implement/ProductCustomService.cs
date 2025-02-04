using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class ProductCustomService : IProductCustomService
{
    private readonly IProductCustomRepository _productCustomRepository;
    private readonly IMapper _mapper;

    public ProductCustomService(IProductCustomRepository productCustomRepository, IMapper mapper)
    {
        _productCustomRepository = productCustomRepository;
        _mapper = mapper;
    }
    public async Task<List<ProductCustomResponse>> GetAllProductCustom()
    {
        var list = await _productCustomRepository.GetAllProductCustom();
        return _mapper.Map<List<ProductCustomResponse>>(list);
    }

    public async Task<Result<ProductCustom>> CreateProductCustom(CreateProductCustomRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            throw new ArgumentException("FlowerBasket name cannot be null or whitespace", nameof(request.ProductName));
        }

        var productCustom = new ProductCustom
        {
            ProductName = request.ProductName,
            CustomerId = request.CustomerId ?? Guid.Empty,
            Description = request.Description,
            FlowerBasketId = request.FlowerBasketId,
            TotalPrice = request.TotalPrice,
            Quantity = request.Quantity
            
        };

        await _productCustomRepository.CreateProductCustom(productCustom);

        return new Result<ProductCustom>
        {
            Data = productCustom,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "ProductCustom created successfully" }
        };
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
            FlowerBasketId = productCustom.FlowerBasketId ?? Guid.Empty, 
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
                Messages = new []{"ProductCustom not found."}
            };
        }

        await _productCustomRepository.DeleteProductCustom(id);

        return new Result<ProductCustom>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"ProductCustom deleted successfully."}
        };
    }

    public async Task<Result<ProductCustomResponse>> GetProductCustomById(Guid id)
    {
        var response = new Result<ProductCustomResponse>();
        var productCustom = await _productCustomRepository.GetProductCustomById(id);
        if (productCustom == null)
        {
            response.Messages = ["ProductCustom not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<ProductCustomResponse>(productCustom);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }
}