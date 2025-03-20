using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Customer;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;

namespace Service.Interface;

public interface ICustomerService
{
    Task<PaginationResponse<CustomerResponse>> GetAllCustomer(int pageNumber, int pageSize);
    Task<Result<CustomerResponse>> GetCustomerById(Guid id);
    Task<Result<CustomerResponse>> UpdateCustomer(Guid id, UpdateCustomerRequest request);
    Task<Result<Customer>> DeleteCustomer(Guid id);
    Task<Result<CustomerResponse>> RegisterCustomer(CreateCustomerRequest request );
}