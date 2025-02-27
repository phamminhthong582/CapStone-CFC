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

    public ChatRoomService(IChatRoomRepository chatRoomRepository,ICustomerRepository customerRepository,IEmployeeRepository employeeRepository, IMapper mapper)
    {
        _chatRoomRepository = chatRoomRepository;
        _mapper = mapper;
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
    }
    public async Task<List<ChatRoomResponse>> GetAllChatRoom()
    {
        var list = await _chatRoomRepository.GetAllChatRoom();
        return _mapper.Map<List<ChatRoomResponse>>(list);
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
        if (request.CustomerId == Guid.Empty) {
            response.Messages = new[] { "Customer ID is invalid." };
            response.ResultStatus = ResultStatus.Invalid.ToString();
            return response;
        }

        if (request.EmployeeId == Guid.Empty) {
            response.Messages = new[] { "Employee ID is invalid." };
            response.ResultStatus = ResultStatus.Invalid.ToString();
            return response;
        }
        // var employee = await _employeeRepository.GetEmployeesById(request.EmployeeId);
        //  var customer = await _customerRepository.GetCustomerById(request.CustomerId);
        //
        //  if (employee == null || customer == null) {
        //      response.Messages = new[] { "Employee or Customer not found." };
        //      response.ResultStatus = ResultStatus.Error.ToString();
        //      return response;
        //  }
        var chatRoom = new ChatRoom
        {
            CustomerId = request.CustomerId,
            EmployeeId = request.EmployeeId,
            Status = ChatRoomStatus.Active.ToString(), 
            CreateAt = DateTime.UtcNow
        };
        var createdChatRoom = await _chatRoomRepository.CreateChatRoom(chatRoom);
        if (createdChatRoom == null)
        {
            response.Messages = new[] { "Failed to create chat room." };
            response.ResultStatus = ResultStatus.Error.ToString();
            return response; 
        }
        response.Data = createdChatRoom;
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