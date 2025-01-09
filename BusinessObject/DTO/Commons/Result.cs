namespace BusinessObject.DTO.Commons;

public class Result<T>
{
    public T Data { get; set; }
    public string ResultStatus { get; set; }
    public string[] Messages { get; set; }
}

public enum RoleName
{
    Admin,
    Customer,
}
public enum ResultStatus
{
    Success,
    NotFound,
    Duplicated,
    Error,

}