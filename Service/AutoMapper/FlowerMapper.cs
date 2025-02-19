using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.Flower;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddFlowerMapperConfig()
    {
        CreateMap<Flower, FlowerResponse>()
     .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));
        CreateMap<Flower, CreateFlowerRequest>().ReverseMap();
        CreateMap<Flower, UpdateFlowerRequest>().ReverseMap();

    }
}