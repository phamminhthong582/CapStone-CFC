using BusinessObject.DTO.Order;
using BusinessObject.DTO.OrderDetails;
using BusinessObject.Entities;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateOrder(OrderRequest orderRequest, Guid customerId)
        {
            Guid? PromotionID = orderRequest.PromotionId;
            string? DeliveryDistrict = orderRequest.DeliveryDistrict;
            string? DeliveryCity = orderRequest.DeliveryCity;
            string? DeliveryAddress = orderRequest.DeliveryAddress;
            string? Note = orderRequest.Note;
            DateTime? DeliveryDateTime = orderRequest.DeliveryDateTime;
            string? Phone = orderRequest.Phone;
            bool? Transfer = orderRequest.Transfer;

            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
            Guid? storeId = customer.StoreId;

            if (PromotionID == null)
            {
                throw new Exception("Hiện không có khuyến mãi này");
            }

            var order = new Order()
            {
                CustomerId = customerId,
                DeliveryDistrict = DeliveryDistrict,
                DeliveryCity = DeliveryCity,
                DeliveryAddress = DeliveryAddress,
                Note = Note,
                DeliveryDateTime = DeliveryDateTime,
                Phone = Phone,
                StoreId = storeId,
                Transfer = Transfer,
                CreateAt = DateTime.Now,
                Refund = false,
                Status = "Order thành công",
                PromotionId = PromotionID
            };

            // Add order to database
            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            if (orderRequest.OrderDetails != null && orderRequest.OrderDetails.Any())
            {
                var tasks = orderRequest.OrderDetails.Select(async orderDetailsRequest =>
                {
                    Guid productID = orderDetailsRequest.ProductId;
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productID);

                    if (product == null)
                    {
                        throw new Exception($"Product with ID {orderDetailsRequest.ProductId} not found.");
                    }

                    return new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = orderDetailsRequest.ProductId,
                        Quantity = orderDetailsRequest.Quantity,
                        CreateAt = DateTime.Now,
                        Status = true,
                        ProductTotalPrice = orderDetailsRequest.Quantity * product.Price - (orderDetailsRequest.Quantity * product.Price * product.Discount)/100
                    };
                });

                // Await the tasks and gather the results into a list
                var orderDetails = (await Task.WhenAll(tasks)).ToList();

                // Add the OrderDetails to the database
                await _unitOfWork.Repository<OrderDetail>().AddRangeAsync(orderDetails);
                await _unitOfWork.CompleteAsync();
                // Calculate the total price for the order
 

                var promotion = await _unitOfWork.Repository<Promotion>().GetByIdAsync(order.PromotionId);

                order.OrderPrice = orderDetails.Sum(od => od.ProductTotalPrice) - (orderDetails.Sum(od => od.ProductTotalPrice) * promotion.PromotionDiscount)/100 ;

                // Save the updated order with the total price
                 _unitOfWork.Repository<Order>().Update(order);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteOrder(Guid OrderID)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(OrderID);
            if(order == null)
            {
                throw new KeyNotFoundException("order not found");

            }
            _unitOfWork.Repository<Order>().Delete(order);
            var orderDetails = (await _unitOfWork.Repository<OrderDetail>().GetAllAsync()).Where(d => d.OrderId == order.OrderId);
             _unitOfWork.Repository<OrderDetail>().DeleteRange(orderDetails);
            await _unitOfWork.CompleteAsync();

        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).Where( order => order.CustomerId == CustomerID).ToListAsync();
            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var orderResponse = orders.Select(order => new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = CustomerID,
                ProductCustomId = order.ProductCustomId,    
                StaffId = order.StaffId,
                PromotionId = order.PromotionId,
                PromotionName = order.Promotion.PromotionName,
                PromotionDiscount = order.Promotion.PromotionDiscount,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = order.StoreId,
                Note = order.Note,
                DeliveryDateTime = order.DeliveryDateTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails.Where(orderDetail =>  orderDetail.OrderId == order.OrderId)
                                                   .Select(orderDetail => new OrderDetailsResponse
                                                   {
                                                       OrderDetailId = orderDetail.OrderDetailId,
                                                       ProductId = orderDetail.ProductId,   
                                                       ProductName = orderDetail.Product.ProductName,
                                                       ProductImage = productImage.Where(pi => pi.ProductId == orderDetail.ProductId)
                                                             .Select(pi => pi.ProductImage1)
                                                            .FirstOrDefault(),
                                                       Price = orderDetail.Product.Price,   
                                                       Discount = orderDetail.Product.Discount,
                                                       ProductTotalPrice = orderDetail.ProductTotalPrice,
                                                       Quantity = orderDetail.Product.Quantity,
                                                       OrderId = orderDetail.OrderId,   
                                                       CreateAt =  orderDetail.CreateAt,    
                                                       UpdateAt = orderDetail?.UpdateAt,    
                                                       Status = orderDetail?.Status,
                                                   }).ToList()
            });
            return orderResponse;


        }

        public Task<OrderResponse> GetOrderById(Guid OrderId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByStoreID(Guid StoreID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).Where(order => order.StoreId == StoreID).ToListAsync();
            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var orderResponse = orders.Select(order => new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = order.CustomerId,
                ProductCustomId = order.ProductCustomId,
                StaffId = order.StaffId,
                PromotionId = order.PromotionId,
                PromotionName = order.Promotion.PromotionName,
                PromotionDiscount = order.Promotion.PromotionDiscount,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = StoreID,
                Note = order.Note,
                DeliveryDateTime = order.DeliveryDateTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails.Where(orderDetail => orderDetail.OrderId == order.OrderId)
                                                   .Select(orderDetail => new OrderDetailsResponse
                                                   {
                                                       OrderDetailId = orderDetail.OrderDetailId,
                                                       ProductId = orderDetail.ProductId,
                                                       ProductName = orderDetail.Product.ProductName,
                                                       ProductImage = productImage.Where(pi => pi.ProductId == orderDetail.ProductId)
                                                             .Select(pi => pi.ProductImage1)
                                                            .FirstOrDefault(),
                                                       Price = orderDetail.Product.Price,
                                                       Discount = orderDetail.Product.Discount,
                                                       ProductTotalPrice = orderDetail.ProductTotalPrice,
                                                       Quantity = orderDetail.Product.Quantity,
                                                       OrderId = orderDetail.OrderId,
                                                       CreateAt = orderDetail.CreateAt,
                                                       UpdateAt = orderDetail?.UpdateAt,
                                                       Status = orderDetail?.Status,
                                                   }).ToList()
            });
            return orderResponse;
        }

        public async Task UpdateOrder(OrderRequest orderRequest,Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            order.DeliveryDistrict = orderRequest.DeliveryDistrict;
            order.DeliveryCity = orderRequest.DeliveryCity;
            order.DeliveryAddress = orderRequest.DeliveryAddress;
            order.Note= orderRequest.Note;
            order.DeliveryDateTime = orderRequest.DeliveryDateTime;
            order.Phone= orderRequest.Phone;
            order.Transfer = orderRequest.Transfer;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
                
        }
    }
}
