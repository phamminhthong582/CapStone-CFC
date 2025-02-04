using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddProductCustomMapperConfig()
    {
        CreateMap<ProductCustom, ProductCustomResponse>().ReverseMap();
        CreateMap<ProductCustom, CreateProductCustomRequest>().ReverseMap();
        CreateMap<ProductCustom, UpdateProductCustomRequest>().ReverseMap();
        
    }
}