using AutoMapper;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Message;
using BusinessObject.Entities;

namespace Service.AutoMapper;

public partial class MapperConfig : Profile
{
    partial void AddMessageMapperConfig()
    {
        CreateMap<Message, MessageResponse>().ReverseMap();
        CreateMap<Message, CreateMessageRequest>().ReverseMap();
        CreateMap<Message, UpdateMessageStatusRequest>().ReverseMap();
    }
}