namespace BusinessObject.DTO.Commons;

public class Result<T>
{
    public T? Data { get; set; }
    public string? ResultStatus { get; set; }
    public string? RoleName { get; set; }

    public string? ChatRoomStatus { get; set; }
    public string[]? Messages { get; set; }
}

public enum RoleName
{
    Admin,
    StoreManager,
    Florist,
    Customer,
    Courier
}
public enum ResultStatus
{
    Success,
    NotFound,
    Duplicated,
    Error,
    Invalid,
    Failed,
    NotVerified,
    Failure,

}

public enum NotificationStatus
{
    Unread,
    Read,
}
public enum ChatRoomStatus
{
    Active, 
    Closed,
}

public enum MessageStatus
{
    Sent, 
    Delivered, 
    Read
}

public enum CustomerStatus
{
    Active,
    Inactive,
    NotVerified,
}
public enum CategoryStatus
{
    Available,
    Unavailable
}

public enum OrderStatus
{
    Completed
}