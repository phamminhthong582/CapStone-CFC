using BusinessObject.DTO.Message;
using Microsoft.AspNetCore.SignalR;

namespace BusinessObject.DTO.Chat;

public class ChatHub : Hub
{
    // Gửi tin nhắn
    public async Task SendMessage(MessageResponse message)
    {

        await Clients.Group(message.ChatRoomId.ToString()).SendAsync("ReceiveMessage", message);
    }
    // Tham gia phòng tin nhắn
    public async Task JoinChatRoom(string chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
    }
    // Rời phòng nhắn
    public async Task LeaveChatRoom(string chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
    }
}