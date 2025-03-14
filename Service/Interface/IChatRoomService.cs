using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.Entities;

namespace Service.Interface;

public interface IChatRoomService
{
    Task<List<ChatRoomResponse>> GetAllChatRoom();
    Task<Result<ChatRoomResponse>> GetChatRoomById(Guid id);
    Task<Result<ChatRoomResponse>> GetChatRoomByEmployeeId(Guid employeeId);
    Task<Result<ChatRoomResponse>> GetChatRoomByCustomerId(Guid customerId);
    Task<Result<ChatRoom>> CreateChatRoom(CreateChatRoomRequest request);
    Task<Result<ChatRoomResponse>> UpdateStatusChatRoom(Guid chatRoomId , string newStatus);
    Task<Result<ChatRoom>> DeleteChatRoom(Guid id);
    Task<Result<object>> GetChatRoomDetailsById(Guid chatroomId);
    Task SendMessageToClients(string chatRoomId, string user, string message);

}