using AutoMapper;
using BusinessObject.DTO.Category;
using BusinessObject.DTO.Chat;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddChatRoomMapperConfig()
    {
        CreateMap<ChatRoom, ChatRoomResponse>().ReverseMap();
        CreateMap<ChatRoom, CreateChatRoomRequest>().ReverseMap();
    }
}