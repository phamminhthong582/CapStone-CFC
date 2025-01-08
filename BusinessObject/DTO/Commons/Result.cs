namespace BusinessObject.DTO.Commons;

public class Result<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}

public enum Roles
{
    Admin,
    Customer,
}