using BusinessObject.Entities;

namespace Repository.Interface;

public interface IChatRoomRepository
{
    Task<List<ChatRoom>> GetAllChatRoom();
    Task<ChatRoom> GetChatRoomById(Guid id);
    Task CreateChatRoom(ChatRoom chatRoom);
    Task<ChatRoom> DeleteChatRoom(Guid id);
    Task<ChatRoom> UpdateStatusChatRoom(Guid chatRoomId, string newStatus);
}