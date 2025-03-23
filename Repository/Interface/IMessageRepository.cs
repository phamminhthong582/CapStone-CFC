using BusinessObject.Entities;

namespace Repository.Interface;

public interface IMessageRepository
{
    Task<List<Message>> GetMessageByChatRoomId(Guid chatroomId);
    Task<List<Message>> GetMessageById(Guid orderId, Guid customerId, Guid employeeId);
    Task<Message> CreateMessage(Message message);
    Task<Message> UpdateStatusMessage(Guid messageId, string newStatus);
    Task<Message> DeleteMessage(Guid messageId);
    
}