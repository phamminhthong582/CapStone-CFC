using AutoMapper;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    public MapperConfig()
    {
        AddCategoryMapperConfig();
        AddPromotionMapperConfig();
        AddEmployeeMapperConfig();
    }

    partial void AddCategoryMapperConfig();
    partial void AddPromotionMapperConfig();
    partial void AddEmployeeMapperConfig();
}