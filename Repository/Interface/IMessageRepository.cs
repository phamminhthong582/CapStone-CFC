using BusinessObject.Entities;

namespace Repository.Interface;

public interface IMessageRepository
{
    Task<List<Message>> GetMessageByChatRoomId(Guid chatroomId);
    Task<Message> CreateMessage(Message message);
    Task<Message> UpdateStatusMessage(Guid messageId, string newStatus);
    Task<Message> DeleteMessage(Guid messageId);
}