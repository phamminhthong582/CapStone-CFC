using AutoMapper;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    public MapperConfig()
    {
        AddCategoryMapperConfig();
        AddPromotionMapperConfig();
        AddEmployeeMapperConfig();
        AddCustomerMapperConfig();
    }

    partial void AddCategoryMapperConfig();
    partial void AddPromotionMapperConfig();
    partial void AddEmployeeMapperConfig();
    partial void AddCustomerMapperConfig();
}