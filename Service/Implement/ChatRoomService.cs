using AutoMapper;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;

namespace Service.Implement;

public class ChatRoomService : IChatRoomService
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMapper _mapper;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    
    

    public ChatRoomService(IChatRoomRepository chatRoomRepository,IOrderRepository orderRepository,ICustomerRepository customerRepository,IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _chatRoomRepository = chatRoomRepository;
        _mapper = mapper;
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }
    public async Task<List<ChatRoomResponse>> GetAllChatRoom()
    {
        var list = await _chatRoomRepository.GetAllChatRoom();
        return _mapper.Map<List<ChatRoomResponse>>(list);
    }
    public async Task<Result<object>> GetChatRoomDetailsById(Guid chatroomId)
    {
        var response = new Result<object>();

        
        var chatRoom = await _chatRoomRepository.GetChatRoomById(chatroomId);
    
        if (chatRoom.OrderId.HasValue)
        {
            var order = await _orderRepository.GetOrderById(chatRoom.OrderId.Value); 
            if (order == null)
            {
                response.Messages = new[] { "Order not found!" };
                response.ResultStatus = ResultStatus.NotFound.ToString();
            }
            else
            {
              
                var result = new
                {
                    customerid = chatRoom.CustomerId,
                    employeeid = chatRoom.EmployeeId,
                    chatRoomId = chatRoom.ChatRoomId,
                    customerEmail = order.Customer.Email,
                    employeeEmail = order.Staff.Email,
                    orderId = order.OrderId
                };

                response.Data = result;
                response.Messages = new[] { "Successfully!" };
                response.ResultStatus = ResultStatus.Success.ToString();
            }
        }
        else
        {
            response.Messages = new[] { "OrderId is null!" };
            response.ResultStatus = ResultStatus.NotFound.ToString();
        }

        return response;
    }
    

    public async Task<Result<ChatRoomResponse>> GetChatRoomById(Guid id)
    {
        var response = new Result<ChatRoomResponse>();
        var chatRoom = await _chatRoomRepository.GetChatRoomById(id);
        if (chatRoom == null)
        {
            response.Messages = ["chatRoom not found!"];
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        else
        {
            response.Data = _mapper.Map<ChatRoomResponse>(chatRoom);
            response.Messages = ["Successfully!"];
            response.ResultStatus = ResultStatus.Success.ToString();
            return response;
        }
    }

    public async Task<Result<ChatRoom>> CreateChatRoom(CreateChatRoomRequest request)
    {
        var response = new Result<ChatRoom>();
        if (request.OrderId == Guid.Empty)
        {
            response.Messages = new[] { "OrderId ID is invalid." };
            response.ResultStatus = ResultStatus.Invalid.ToString();
            return response;
        }
        var order =  await _orderRepository.GetOrderById(request.OrderId);
        var customerId = order.CustomerId;
        var employeeId = order.StaffId;
        
        
        var chatRoom = new ChatRoom
        {
            
            CustomerId = customerId,  
            EmployeeId = employeeId,  
            Status = ChatRoomStatus.Active.ToString(),
            CreateAt = DateTime.UtcNow
        };
        var createdChatRoom =  _chatRoomRepository.CreateChatRoom(chatRoom);
        if (createdChatRoom == null)
        {
            response.Messages = new[] { "Failed to create chat room." };
            response.ResultStatus = ResultStatus.Error.ToString();
            return response;
        }
        response.Messages = new[] { "Successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();

        return response;
    }

    public async Task<Result<ChatRoomResponse>> UpdateStatusChatRoom(Guid chatRoomId, string newStatus)
    {
        var response = new Result<ChatRoomResponse>();
        var chatRoom = await _chatRoomRepository.UpdateStatusChatRoom(chatRoomId, newStatus);
        if (chatRoom == null)
        {
            response.Messages = new[] { "ChatRoom not found!" };
            response.ResultStatus = ResultStatus.NotFound.ToString();
            return response;
        }
        response.Data = new ChatRoomResponse
        {
            ChatRoomId = chatRoom.ChatRoomId,
            Status = chatRoom.Status,
            CreateAt = chatRoom.CreateAt ?? DateTime.MinValue 
        };
        response.Messages = new[] { "ChatRoom updated successfully!" };
        response.ResultStatus = ResultStatus.Success.ToString();
        return response;
    }

    public async Task<Result<ChatRoom>> DeleteChatRoom(Guid id)
    {
        var chatRoom = await _chatRoomRepository.GetChatRoomById(id);
    
        if (chatRoom == null)
        {
            return new Result<ChatRoom>
            {
                ResultStatus = ResultStatus.NotFound.ToString(),
                Messages = new []{"ChatRoom not found."}
            };
        }

        await _chatRoomRepository.DeleteChatRoom(id);

        return new Result<ChatRoom>
        {
            ResultStatus = ResultStatus.Success.ToString(),
            Messages = new []{"ChatRoom deleted successfully."}
        };
    }

  
}