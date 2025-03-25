namespace BusinessObject.DTO.Customer;

public class CustomerRequest
{
    public Guid CustomerId { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; }
}