namespace BusinessObject.DTO.Commons;

public class Result<T>
{
    public T? Data { get; set; }
    public string? ResultStatus { get; set; }

    public string? UserStatus { get; set; }
    public string[]? Messages { get; set; }
}

public enum RoleName
{
    Admin,
    StoreManager,
    Staff,
    Customer,
}
public enum ResultStatus
{
    Success,
    NotFound,
    Duplicated,
    Error,

}
public enum UserStatus
{
    Active,
    Inactive,
    NotVerified
}
public enum CategoryStatus
{
    Available,
    Unavailable
}