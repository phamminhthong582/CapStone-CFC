    using BusinessObject.DTO.Order;
using BusinessObject.DTO.OrderDetails;
using BusinessObject.Entities;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using Org.BouncyCastle.Asn1.X509;
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
        public async Task ConvertCartToOrder(Guid customerId, OrderRequest orderRequest)
        {
            // Lấy thông tin khách hàng
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            // Lấy tất cả sản phẩm trong giỏ hàng của khách hàng
            var cartItems = await _unitOfWork.Repository<Cart>()
                .Entities
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Product) // Load thông tin sản phẩm
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
            {
                throw new Exception("Cart is empty.");
            }

            // Tạo đơn hàng
            var order = new Order
            {
                CustomerId = customerId,
                StoreId = orderRequest.StoreId,
                DeliveryDistrict = orderRequest.DeliveryDistrict,
                DeliveryCity = orderRequest.DeliveryCity,
                DeliveryAddress = orderRequest.DeliveryAddress,
                Note = orderRequest.Note,
                RecipientTime = orderRequest.RecipientTime,
                Phone = orderRequest.Phone,
                Transfer = orderRequest.Transfer,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Refund = false,
                Status = "Chờ thành toán",
                PromotionId = orderRequest.PromotionId,
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Tạo danh sách chi tiết đơn hàng từ giỏ hàng và cập nhật số lượng sản phẩm đã bán
            var orderDetails = new List<OrderDetail>();
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product == null)
                {
                    throw new Exception($"Product with ID {cartItem.ProductId} not found.");
                }

                // Cập nhật số lượng sản phẩm đã bán
                cartItem.Product.Sold += cartItem.Quantity;
                _unitOfWork.Repository<Product>().Update(cartItem.Product);

                // Thêm OrderDetail
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    CreateAt = DateTime.Now,
                    Status = true,
                    ProductTotalPrice = cartItem.Quantity * cartItem.Product.Price -
                        (cartItem.Quantity * cartItem.Product.Price * cartItem.Product.Discount) / 100
                };
                orderDetails.Add(orderDetail);
            }

            // Lưu OrderDetails vào database
            await _unitOfWork.Repository<OrderDetail>().AddRangeAsync(orderDetails);
            await _unitOfWork.CompleteAsync();

            // Tính tổng giá đơn hàng
            double? totalPrice = orderDetails.Sum(od => od.ProductTotalPrice);
            if (order.PromotionId.HasValue)
            {
                var promotion = await _unitOfWork.Repository<Promotion>().GetByIdAsync(order.PromotionId.Value);
                if (promotion?.PromotionDiscount > 0) // Kiểm tra promotion không null và có giảm giá hợp lệ
                {
                    totalPrice *= (1 - (promotion.PromotionDiscount / 100.0));
                }
            }

            order.OrderPrice = totalPrice;

            // Cập nhật tổng giá đơn hàng
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            // Xóa giỏ hàng sau khi đã chuyển thành đơn hàng
            _unitOfWork.Repository<Cart>().DeleteRange(cartItems);
            await _unitOfWork.CompleteAsync();
        }

        public async Task CreateOrder(OrderRequest orderRequest, Guid customerId)
        {
            Guid? PromotionID = orderRequest.PromotionId;
            string? DeliveryDistrict = orderRequest.DeliveryDistrict;
            string? DeliveryCity = orderRequest.DeliveryCity;
            string? DeliveryAddress = orderRequest.DeliveryAddress;
            string? Note = orderRequest.Note;
            DateTime? DeliveryDateTime = orderRequest.RecipientTime;
            string? Phone = orderRequest.Phone;
            bool? Transfer = orderRequest.Transfer;
            bool? Delivery = orderRequest.Delivery;
            string? RecipientName = orderRequest.RecipientName;
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);

            var order = new Order()
            {
                CustomerId = customerId,
                StoreId  = orderRequest.StoreId,    
                DeliveryDistrict = DeliveryDistrict,
                DeliveryCity = DeliveryCity,
                DeliveryAddress = DeliveryAddress,
                Note = Note,
                RecipientTime = DeliveryDateTime,
                Phone = Phone,
                RecipientName = RecipientName,
                Transfer = Transfer,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Refund = false,
                Status = "Chờ thành toán",
                PromotionId = PromotionID,
                Delivery = Delivery,
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
                    product.Sold += orderDetailsRequest.Quantity;
                    _unitOfWork.Repository<Product>().Update(product);
                    await _unitOfWork.CompleteAsync();
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

                double? totalPrice = orderDetails.Sum(od => od.ProductTotalPrice);
                if (order.PromotionId.HasValue)
                {
                    var promotion = await _unitOfWork.Repository<Promotion>().GetByIdAsync(order.PromotionId.Value);
                    if (promotion?.PromotionDiscount > 0) // Kiểm tra promotion không null và có giảm giá hợp lệ
                    {
                        totalPrice *= (1 - (promotion.PromotionDiscount / 100.0));
                    }
                }
                order.OrderPrice = totalPrice;
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
            var orderDetails = (await _unitOfWork.Repository<OrderDetail>().GetAllAsync()).Where(d => d.OrderId == order.OrderId);
             _unitOfWork.Repository<OrderDetail>().DeleteRange(orderDetails);
            _unitOfWork.Repository<Order>().Delete(order);
            await _unitOfWork.CompleteAsync();

        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Include(d => d.Promotion)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(order => order.CustomerId == CustomerID)
                .ToListAsync();

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
                PromotionName = order.Promotion?.PromotionName,
                PromotionDiscount = order.Promotion?.PromotionDiscount ?? 0,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = order.StoreId,
                Note = order.Note,
                DeliveryDateTime = order.RecipientTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails?
                    .Where(orderDetail => orderDetail.OrderId == order.OrderId)
                    .Select(orderDetail => new OrderDetailsResponse
                    {
                        OrderDetailId = orderDetail.OrderDetailId,
                        ProductId = orderDetail.ProductId,
                        ProductName = orderDetail.Product?.ProductName,
                        ProductImage = productImage
                            .Where(pi => pi.ProductId == orderDetail.ProductId)
                            .Select(pi => pi.ProductImage1)
                            .FirstOrDefault(),
                        Price = orderDetail.Product?.Price ?? 0,
                        Discount = orderDetail.Product?.Discount ?? 0,
                        ProductTotalPrice = orderDetail.ProductTotalPrice,
                        Quantity = orderDetail.Product?.Quantity ?? 0,
                        OrderId = orderDetail.OrderId,
                        CreateAt = orderDetail.CreateAt,
                        UpdateAt = orderDetail.UpdateAt,
                        Status = orderDetail.Status
                    })
                    .ToList() ?? new List<OrderDetailsResponse>()
            });

            return orderResponse;
        }
        public async Task<OrderResponse> GetOrderById(Guid OrderId)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.OrderId == OrderId);
            if (order == null)
            {
                throw new Exception($"Order with ID {OrderId} not found.");
            }

            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var orderResponse = new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = order.CustomerId,
                ProductCustomId = order.ProductCustomId,
                StaffId = order.StaffId,
                PromotionId = order.PromotionId,
                PromotionName = order.Promotion?.PromotionName,
                PromotionDiscount = order.Promotion?.PromotionDiscount,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = order.StoreId,
                Note = order.Note,
                DeliveryDateTime = order.RecipientTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails.Select(orderDetail => new OrderDetailsResponse
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
                    Quantity = orderDetail.Quantity,
                    OrderId = orderDetail.OrderId,
                    CreateAt = orderDetail.CreateAt,
                    UpdateAt = orderDetail.UpdateAt,
                    Status = orderDetail.Status,
                }).ToList()
            };

            return orderResponse;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByStaffId(Guid StaffId)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Include(d => d.Promotion)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(order => order.StaffId == StaffId)
                .ToListAsync();

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
                PromotionName = order.Promotion?.PromotionName,
                PromotionDiscount = order.Promotion?.PromotionDiscount ?? 0, // Add default value if null
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = order.StaffId,
                Note = order.Note,
                DeliveryDateTime = order.RecipientTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails?
                    .Where(orderDetail => orderDetail.OrderId == order.OrderId)
                    .Select(orderDetail => new OrderDetailsResponse
                    {
                        OrderDetailId = orderDetail.OrderDetailId,
                        ProductId = orderDetail.ProductId,
                        ProductName = orderDetail.Product?.ProductName,
                        ProductImage = productImage
                            .Where(pi => pi.ProductId == orderDetail.ProductId)
                            .Select(pi => pi.ProductImage1)
                            .FirstOrDefault(),
                        Price = orderDetail.Product?.Price ?? 0,
                        Discount = orderDetail.Product?.Discount ?? 0,
                        ProductTotalPrice = orderDetail.ProductTotalPrice,
                        Quantity = orderDetail.Product?.Quantity ?? 0,
                        OrderId = orderDetail.OrderId,
                        CreateAt = orderDetail.CreateAt,
                        UpdateAt = orderDetail.UpdateAt,
                        Status = orderDetail.Status
                    })
                    .ToList() ?? new List<OrderDetailsResponse>() // Return empty list if null
            });

            return orderResponse;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByStoreID(Guid StoreID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Include(d => d.Promotion)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(order => order.StoreId == StoreID)
                .ToListAsync();

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
                PromotionName = order.Promotion?.PromotionName,
                PromotionDiscount = order.Promotion?.PromotionDiscount ?? 0,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = StoreID,
                Note = order.Note,
                DeliveryDateTime = order.RecipientTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                Refund = order.Refund,
                CreateAt = order.CreateAt,
                UpdateAt = order.UpdateAt,
                Status = order.Status,
                OrderDetails = order.OrderDetails?
                    .Where(orderDetail => orderDetail.OrderId == order.OrderId)
                    .Select(orderDetail => new OrderDetailsResponse
                    {
                        OrderDetailId = orderDetail.OrderDetailId,
                        ProductId = orderDetail.ProductId,
                        ProductName = orderDetail.Product?.ProductName,
                        ProductImage = productImage
                            .Where(pi => pi.ProductId == orderDetail.ProductId)
                            .Select(pi => pi.ProductImage1)
                            .FirstOrDefault(),
                        Price = orderDetail.Product?.Price ?? 0,
                        Discount = orderDetail.Product?.Discount ?? 0,
                        ProductTotalPrice = orderDetail.ProductTotalPrice,
                        Quantity = orderDetail.Product?.Quantity ?? 0,
                        OrderId = orderDetail.OrderId,
                        CreateAt = orderDetail.CreateAt,
                        UpdateAt = orderDetail.UpdateAt,
                        Status = orderDetail.Status
                    })
                    .ToList() ?? new List<OrderDetailsResponse>()
            });

            return orderResponse;
        }

        public async Task UpdateOrder(OrderRequest orderRequest,Guid orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            order.DeliveryDistrict = orderRequest.DeliveryDistrict ?? order.DeliveryDistrict;
            order.DeliveryCity = orderRequest.DeliveryCity ?? order.DeliveryCity;
            order.DeliveryAddress = orderRequest.DeliveryAddress ?? order.DeliveryAddress;
            order.Note= orderRequest.Note ?? order.Note;
            order.RecipientTime = orderRequest.RecipientTime ?? order.RecipientTime;
            order.Phone= orderRequest.Phone ?? order.Phone;
            order.Transfer = orderRequest.Transfer ?? order.Transfer;
            order.Status = orderRequest.Status ?? order.Status;
            order.UpdateAt = DateTime.Now;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateOrderByStoreId(Guid orderId, Guid StaffId)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
            order.StaffId = StaffId ;
            order.UpdateAt = DateTime.Now;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();


        }

        public async Task UpdateStatusOrderByStaffId(Guid orderId, string Status)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
            order.Status = Status ?? order.Status;
            order.UpdateAt = DateTime.Now;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
        }
        
    }
}
