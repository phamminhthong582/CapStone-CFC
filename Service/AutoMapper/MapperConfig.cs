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
        AddFeedbackMapperConfig();
        AddFlowerMapperConfig();
        AddFlowerBasketMapperConfig();
        AddProductCustomMapperConfig();
        AddFlowerCustomMapperConfig();
        AddChatRoomMapperConfig();
        AddMessageMapperConfig();
    }

    partial void AddCategoryMapperConfig();
    partial void AddPromotionMapperConfig();
    partial void AddEmployeeMapperConfig();
    partial void AddCustomerMapperConfig();
    partial void AddCommentMapperConfig();
    partial void AddFeedbackMapperConfig();
    partial void AddFlowerMapperConfig();
    partial void AddFlowerBasketMapperConfig();
    partial void AddProductCustomMapperConfig();
    partial void AddFlowerCustomMapperConfig();
    partial void AddChatRoomMapperConfig();
    partial void AddMessageMapperConfig();
}