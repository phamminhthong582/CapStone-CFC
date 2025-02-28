using AutoMapper;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class FlowerCustomService : IFlowerCustomService
{
    private readonly IFlowerCustomRepository _flowerCustomRepository;
    private readonly IMapper _mapper;

    public FlowerCustomService(IFlowerCustomRepository flowerCustomRepository, IMapper mapper)
    {
        _flowerCustomRepository = flowerCustomRepository;
        _mapper = mapper;
    }
    public async Task<List<FlowerCustomResponse>> GetAllFlowerCustom()
    {
        var list = await _flowerCustomRepository.GetAllFlowerCustom();
        return _mapper.Map<List<FlowerCustomResponse>>(list);
    }

    public async Task<Result<FlowerCustom>> CreateFlowerCustom(CreateFlowerCustomRequest request)
    {
      
      
        var newFlowerCustom = new FlowerCustom
        {
            FlowerCustomId = Guid.NewGuid(),
            
            Quantity = request.Quantity,
            CreateAt = DateTime.UtcNow,
            UpdateAt = DateTime.UtcNow
        };
        await _flowerCustomRepository.CreateFlowerCustom(newFlowerCustom);
        return new Result<FlowerCustom>
        {
            Data = newFlowerCustom,
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new[] { "Flower custom created successfully" }
        };
    }

    public async Task<Result<FlowerCustomResponse>> UpdateFlowerCustom(Guid id, UpdateFlowerCustomRequest request)
    {
        if (id == Guid.Empty)
        {
            return new Result<FlowerCustomResponse>
            {
                ResultStatus = ResultStatus.Invalid.ToString(),
                Messages = new []{"FlowerCustomId cannot be empty"} 
            };
        }
        var existingFlowerCustom = await _flowerCustomRepository.GetFlowerCustomById(id);
        if (existingFlowerCustom == null)
        {
            return new Result<FlowerCustomResponse>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"FlowerCustom not found"} 
            };
        }
        existingFlowerCustom.Quantity = request.Quantity ?? existingFlowerCustom.Quantity;
        existingFlowerCustom.Price = request.Price ?? existingFlowerCustom.Price;
        existingFlowerCustom.Status = request.Status ?? existingFlowerCustom.Status;
        existingFlowerCustom.UpdateAt = DateTime.UtcNow;

        await _flowerCustomRepository.UpdateFlowerCustom(existingFlowerCustom);

        var response = new FlowerCustomResponse
        {
            FlowerCustomId = existingFlowerCustom.FlowerCustomId,
            Quantity = existingFlowerCustom.Quantity ?? 0,
            Status = existingFlowerCustom.Status ?? true, 
            UpdateAt = existingFlowerCustom.UpdateAt
        };

        return new Result<FlowerCustomResponse>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Flower custom updated successfully"} ,
            Data = response
        };
    }

    public async Task<Result<FlowerCustom>> DeleteFlowerCustom(Guid id)
    {
        var flowerCustom = await _flowerCustomRepository.GetFlowerCustomById(id);
    
        if (flowerCustom == null)
        {
            return new Result<FlowerCustom>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"FlowerCustom not found."}
            };
        }

        await _flowerCustomRepository.DeleteFlowerCustom(id);

        return new Result<FlowerCustom>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"FlowerCustom deleted successfully."}
        };
    }

    public async Task<Result<FlowerCustomResponse>> GetFlowerCustomById(Guid id)
    {
        var response = new Result<FlowerCustomResponse>();
        var flowerCustom = await _flowerCustomRepository.GetFlowerCustomById(id);
        if (flowerCustom == null)
        {
            response.Messages = ["FlowerCustom not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<FlowerCustomResponse>(flowerCustom);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }
}