using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Message;
using BusinessObject.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Service.Interface;

public interface IMessageService
{
    Task<Result<MessageResponse>> SendMessage(CreateMessageRequest request);
    Task<Result<List<MessageResponse>>> GetMessageByChatRoomId(Guid chatroomId);
    Task<Result<MessageResponse>> UpdateMessageStatus(Guid messageId, string newStatus);
    Task<Result<Message>> DeleteMessage(Guid messageId);
}