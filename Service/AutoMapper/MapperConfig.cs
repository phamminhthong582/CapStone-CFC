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
        AddCommentMapperConfig();
    }

    partial void AddCategoryMapperConfig();
    partial void AddPromotionMapperConfig();
    partial void AddEmployeeMapperConfig();
    partial void AddCustomerMapperConfig();
    partial void AddCommentMapperConfig();
}