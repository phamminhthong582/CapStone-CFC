using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly CustomFlowerChainContext _context;

    public ChatRoomRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<ChatRoom>> GetAllChatRoom()
    {
        var list = await _context.ChatRooms.ToListAsync();
        return list;    
    }

    public async Task<ChatRoom> GetChatRoomById(Guid id)
    {
        var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(x => x.ChatRoomId == id);
        return chatRoom;
    }

    public async Task<ChatRoom> CreateChatRoom(ChatRoom chatRoom)
    {
        await _context.ChatRooms.AddAsync(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<ChatRoom> DeleteChatRoom(Guid id)
    {
        var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(x => x.ChatRoomId == id);
        if (chatRoom == null)
        {
            return null;
        }
        _context.ChatRooms.Remove(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<ChatRoom> UpdateStatusChatRoom(Guid chatRoomId, string newStatus)
    {
        var chatRoom = await _context.ChatRooms.FirstOrDefaultAsync(c => c.ChatRoomId == chatRoomId);

        if (chatRoom == null)
        {
            return null;
        }
        chatRoom.Status = newStatus;
        await _context.SaveChangesAsync();
        return chatRoom;
    }
}