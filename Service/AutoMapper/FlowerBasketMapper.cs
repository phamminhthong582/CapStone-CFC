using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddFlowerBasketMapperConfig()
    {
        CreateMap<FlowerBasket, FlowerBasketResponse>().ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
        CreateMap<FlowerBasket, CreateFlowerBasketRequest>().ReverseMap();
        CreateMap<FlowerBasket, UpdateFlowerBasketRequest>().ReverseMap();
    }
}