namespace BusinessObject.DTO.Commons;

public class Result<T>
{
    public T? Data { get; set; }
    public string? ResultStatus { get; set; }
    public string? RoleName { get; set; }

    public string? PromotionStatus { get; set; }
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

}
public enum PromotionStatus
{
    Expired,
    StillExpired,
    
    
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