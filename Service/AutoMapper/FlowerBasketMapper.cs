using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddFlowerBasketMapperConfig()
    {
        CreateMap<FlowerBasket, FlowerBasketResponse>().ReverseMap();
        CreateMap<FlowerBasket, CreateFlowerBasketRequest>().ReverseMap();
        CreateMap<FlowerBasket, UpdateFlowerBasketRequest>().ReverseMap();
    }
}