using BusinessObject.Entities;

namespace Repository.Interface;

public interface IOrderRepository
{
    Task<Order> GetOrderById(Guid orderId);
}