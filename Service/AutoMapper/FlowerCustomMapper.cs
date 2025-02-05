using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddFlowerCustomMapperConfig()
    {
        CreateMap<FlowerCustom, FlowerCustomResponse>().ReverseMap();
        CreateMap<FlowerCustom, CreateFlowerCustomRequest>().ReverseMap();
        CreateMap<FlowerCustom, UpdateFlowerCustomRequest>().ReverseMap();
    }
}