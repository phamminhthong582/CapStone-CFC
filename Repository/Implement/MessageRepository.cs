using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class MessageRepository : IMessageRepository
{
    private readonly CustomFlowerChainContext _context;

    public MessageRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<List<Message>> GetMessageByChatRoomId(Guid chatroomId)
    {
        return await _context.Messages
            .Where(m => m.ChatRoomId == chatroomId)  // Lọc tin nhắn theo chatRoomId
            .OrderBy(m => m.CreateAt)  // Sắp xếp theo thời gian tạo tin nhắn (nếu cần)
            .ToListAsync();  // Lấy danh sách tin nhắn
    }

    public async Task<Message> CreateMessage(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> UpdateStatusMessage(Guid messageId, string newStatus)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return null;
        message.Status = newStatus;
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<Message> DeleteMessage(Guid messageId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(x => x.MessageId == messageId);
        if (message == null)
        {
            return null;
        }
        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return message;
    }
}