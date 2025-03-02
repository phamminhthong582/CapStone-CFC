using BusinessObject.Context;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;

namespace Repository.Implement;

public class OrderRepository : IOrderRepository
{
    private readonly CustomFlowerChainContext _context;

    public OrderRepository(CustomFlowerChainContext context)
    {
        _context = context;
    }
    public async Task<Order> GetOrderById(Guid orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);
        return order;
    }
}