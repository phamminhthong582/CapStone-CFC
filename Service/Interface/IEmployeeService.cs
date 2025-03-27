using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.Entities;
using System.Collections.Generic;

namespace Service.Interface;

public interface IEmployeeService
{
    Task<List<EmployeeResponse>> GetAllEmployee();
    Task<EmployeeResponse> GetEmployeeById(Guid id);
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeeByStoreId(Guid StoreId);
    //Task<Result<EmployeeResponse>> UpdateEmployee(Guid id, UpdateEmployeeRequest request);
    Task<IEnumerable<EmployeeResponse>> GetFloristWithStoreIdWithStatusFalse(Guid storeId);
    Task<IEnumerable<CourierResponse>> GetCourierWithStoreIdWithStatusFalse(Guid storeid);
    Task<IEnumerable<EmployeeResponse>> GetFloristWithStoreIdWithStatusTrue(Guid storeId);
    Task<IEnumerable<CourierResponse>> GetCourierWithStoreIdWithStatusTrue(Guid storeid);
    //Task<Result<Employee>> DeleteEmployee(Guid id);
    Task CreateManagerStore(Guid storeid, CreateManagerStoreRequest createManagerStoreRequest);
    Task ApproveEmployee(Guid employeeId);
    Task Reject(Guid employeeid, string reason);
    //Task UpdateStatusEmloyee(Guid employeeid, bool status);


}