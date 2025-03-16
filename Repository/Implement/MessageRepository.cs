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
            .Where(m => m.ChatRoomId == chatroomId) 
            .OrderBy(m => m.CreateAt)  
            .ToListAsync();  
    }

    public async Task<List<Message>> GetMessageById(Guid orderId ,  Guid customerId , Guid employeeId)
    {
        var chatRoom = await _context.ChatRooms
            .Where(c => c.OrderId == orderId && c.CustomerId == customerId && c.EmployeeId == employeeId)
            .FirstOrDefaultAsync();

        if (chatRoom == null)
        {
            return new List<Message>(); 
        }
        return await _context.Messages
            .Where(m => m.ChatRoomId == chatRoom.ChatRoomId) 
            .OrderBy(m => m.CreateAt)  
            .ToListAsync();  
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