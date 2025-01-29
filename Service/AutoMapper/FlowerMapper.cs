using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.Flower;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddFlowerMapperConfig()
    {
        CreateMap<Flower, FlowerResponse>().ReverseMap();
        CreateMap<Flower, CreateFlowerRequest>().ReverseMap();
        CreateMap<Flower, UpdateFlowerRequest>().ReverseMap();

    }
}