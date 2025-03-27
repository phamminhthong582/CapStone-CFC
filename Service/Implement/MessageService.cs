using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Message;

using BusinessObject.Entities;
using Microsoft.AspNetCore.SignalR;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class MessageService : IMessageService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRoomRepository _chatRoomRepository;

    public MessageService(IHubContext<ChatHub> hubContext,IMessageRepository messageRepository, IChatRoomRepository chatRoomRepository)
    {
        _messageRepository = messageRepository;
        _hubContext = hubContext;
        _chatRoomRepository = chatRoomRepository;
    }
    public async Task<Result<MessageResponse>> SendMessage(CreateMessageRequest request)
    {
        var response = new Result<MessageResponse>();
        if (request.SenderId == Guid.Empty || request.ReceiveId == Guid.Empty)
        {
            response.Messages = new[] { "Sender ID or Receiver ID is invalid." };
            response.ResultStatus = ResultStatus.Invalid.ToString();
            return response;
        }
        var message = new Message
        {
            ChatRoomId = request.ChatRoomId,
            SenderId = request.SenderId,
            ReceiveId = request.ReceiveId,
            MessageType = request.MessageType,
            Content = request.Content,
            Status = MessageStatus.Sent.ToString(),
            CreateAt = DateTime.UtcNow
        };
        var createdMessage = await _messageRepository.CreateMessage(message);

        if (createdMessage == null)
        {
            response.Messages = new[] { "Failed to create message." };
            response.ResultStatus = ResultStatus.Error.ToString();
            return response;
        }
        response.Data = new MessageResponse
        {
            MessageId = createdMessage.MessageId,
            ChatRoomId = createdMessage.ChatRoomId,
            SenderId = createdMessage.SenderId,
            ReceiveId = createdMessage.ReceiveId,
            MessageType = createdMessage.MessageType,
            Content = createdMessage.Content,
            Status = createdMessage.Status,
            CreateAt = createdMessage.CreateAt
        };


        response.Messages = new[] { "Successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }

    public async Task<Result<List<MessageResponse>>> GetMessageByChatRoomId(Guid chatroomId)
    {
        var response = new Result<List<MessageResponse>>();
        
        var messages = await _messageRepository.GetMessageByChatRoomId(chatroomId);

        if (!messages.Any())
        {
            response.Messages = new[] { "No messages found in this chat room." };
            response.ResultStatus = ResultStatus.NotFound.ToString(); 
            return response;
        }
        var messageResponses = messages.Select(m => new MessageResponse
        {
            MessageId = m.MessageId,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            ReceiveId = m.ReceiveId,
            MessageType = m.MessageType,
            Content = m.Content,
            Status = m.Status,
            CreateAt = m.CreateAt
        }).ToList();
        response.Data = messageResponses;
        response.Messages = new[] { "Messages retrieved successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<List<MessageResponse>>> GetMessagesByChatRoom(Guid orderId, Guid customerId, Guid employeeId)
    {
        var response = new Result<List<MessageResponse>>();
        var messages = await _messageRepository.GetMessageById(orderId, customerId, employeeId);
        if (!messages.Any())
        {
            response.Messages = new[] { "No messages found for the specified criteria." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        var messageResponses = messages.Select(m => new MessageResponse
        {
            MessageId = m.MessageId,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            ReceiveId = m.ReceiveId,
            MessageType = m.MessageType,
            Content = m.Content,
            Status = m.Status,
            CreateAt = m.CreateAt
        }).ToList();
        response.Data = messageResponses;
        response.Messages = new[] { "Messages retrieved successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }

    public async Task<Result<MessageResponse>> UpdateMessageStatus(Guid messageId, string newStatus)
    {
        var response = new Result<MessageResponse>();
        if (!Enum.IsDefined(typeof(MessageStatus), newStatus))
        {
            response.Messages = new[] { "Invalid status." };
            response.ResultStatus = ResultStatus.Invalid.ToString();
            return response;
        }
        var message = await _messageRepository.UpdateStatusMessage(messageId, newStatus);

        if (message == null)
        {
            response.Messages = new[] { "Message not found." };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }

        response.Data = new MessageResponse()
        {
            MessageId = message.MessageId,
            Status = message.Status,
            CreateAt = message.CreateAt
        };

        response.Messages = new[] { "Status updated successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<Message>> DeleteMessage(Guid messageId)
    {
        var message = await _messageRepository.GetMessageByChatRoomId(messageId);
    
        if (message == null)
        {
            return new Result<Message>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"Message not found."}
            };
        }

        await _messageRepository.DeleteMessage(messageId);

        return new Result<Message>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"Message deleted successfully."}
        };
    }
}