using AutoMapper;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    public MapperConfig()
    {
        AddCategoryMapperConfig();
        AddPromotionMapperConfig();
    }

    partial void AddCategoryMapperConfig();
    partial void AddPromotionMapperConfig();
}