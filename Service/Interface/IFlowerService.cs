using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Flower;
using BusinessObject.Entities;

namespace Service.Interface
{
    public interface IFlowerService
    {
        Task<List<FlowerResponse>> GetAllFlower();
        Task<Result<Flower>> CreateFlower(CreateFlowerRequest request);
        Task<Result<FlowerResponse>> UpdateFlower(Guid id, UpdateFlowerRequest request);
        Task<Result<Flower>> DeleteFlower(Guid id);
        Task<Result<FlowerResponse>> GetFlowerById(Guid id);
        Task<Result<FlowerResponse>> GetFlowerByName(string name);
        Task<Result<List<FlowerResponse>>> GetFlowerByPrice(double minPrice, double? maxPrice = null);
    }
}
