using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.DTO.Order;
using BusinessObject.DTO.OrderDetails;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using Org.BouncyCastle.Asn1.X509;
using Repository.Implement;
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

        public async Task AutoUpdateOrder()
        {
            var orders = (await _unitOfWork.Repository<Order>().GetAllAsync())
                .Where(o => o.Status == "đặt hàng thành công" && o.StaffId == null)
                .ToList();

            if (!orders.Any())
                return;

            var staffRepo = _unitOfWork.Repository<Employee>();
            var staffList = await staffRepo.GetAllAsync();
            var staffIds = staffList.Select(s => s.EmployeeId).ToList();

            if (!staffIds.Any())
                return;

            var random = new Random();

            foreach (var order in orders)
            {
                order.StaffId = staffIds[random.Next(staffIds.Count)];
                order.UpdateAt = DateTime.UtcNow;
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<Order> ConvertCartToOrder(Guid customerId, OrderCartRequest orderRequest)
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
            return order;
        }
        
        public async Task<Order> CreateOrder(OrderRequest orderRequest, Guid customerId)
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
            return order;  // This returns the complete order object with its OrderId
        }

        public async Task<Order> CreateOrderCustom(Guid Customer, OrderCustomRequest orderCustomRequest)
        {
            Guid? PromotionID = orderCustomRequest.PromotionId;
            string? DeliveryDistrict = orderCustomRequest.DeliveryDistrict;
            string? DeliveryCity = orderCustomRequest.DeliveryCity;
            string? DeliveryAddress = orderCustomRequest.DeliveryAddress;
            string? Note = orderCustomRequest.Note;
            DateTime? DeliveryDateTime = orderCustomRequest.RecipientTime;
            string? Phone = orderCustomRequest.Phone;
            bool? Transfer = orderCustomRequest.Transfer;
            bool? Delivery = orderCustomRequest.Delivery;
            string? RecipientName = orderCustomRequest.RecipientName;
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(Customer);

            var order = new Order()
            {
                CustomerId = Customer,
                StoreId = orderCustomRequest.StoreId,
                ProductCustomId = orderCustomRequest.ProductCustomId,
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
            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();
            var productCustom = await _unitOfWork.GetRepo<ProductCustom>().GetByIdAsync(orderCustomRequest.ProductCustomId);
            double? totalPrice = productCustom.TotalPrice;
            if (order.PromotionId.HasValue)
            {
                var promotion = await _unitOfWork.Repository<Promotion>().GetByIdAsync(order.PromotionId.Value);
                if (promotion?.PromotionDiscount > 0) // Kiểm tra promotion không null và có giảm giá hợp lệ
                {
                    totalPrice *= (1 - (promotion.PromotionDiscount / 100.0));
                }
            }
            order.OrderPrice = totalPrice;
            _unitOfWork.Repository<Order>().Update(order);

           await _unitOfWork.CompleteAsync();
            
            return order; 
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
                    .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude( b => b.Category)
                    .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                    .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                .Where(order => order.CustomerId == CustomerID)
                .ToListAsync();

            var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                   .Include(fc => fc.Flower)
                                       .ThenInclude(f => f.Category)
                                   .ToListAsync();

            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();

            var orderResponse = orders.Select(order => new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = CustomerID,
                ProductCustomId = order.ProductCustomId,
                ProductCustomResponse = order.ProductCustom != null ? new ProductCustomResponse
                {
                    ProductCustomId = order.ProductCustomId,
                    ProductName = order.ProductCustom.ProductName,
                    Quantity = order.ProductCustom.Quantity,
                    TotalPrice = order.ProductCustom.TotalPrice,
                    CustomerId = order.ProductCustom.CustomerId,
                    CreateAt = order.ProductCustom.CreateAt,
                    UpdateAt = order.ProductCustom.UpdateAt,
                    Status = order.ProductCustom.Status,
                    flowerBasketResponse = order.ProductCustom.FlowerBasket != null ? new FlowerBasketResponse
                    {
                        FlowerBasketId = order.ProductCustom.FlowerBasket.FlowerBasketId,
                        FlowerBasketName = order.ProductCustom.FlowerBasket.FlowerBasketName,
                        MaxQuantity = order.ProductCustom.FlowerBasket.MaxQuantity,
                        MinQuantity = order.ProductCustom.FlowerBasket.MinQuantity,
                        Quantity = order.ProductCustom.FlowerBasket.Quantity,
                        Image = order.ProductCustom.FlowerBasket.Image,
                        CategoryName = order.ProductCustom.FlowerBasket.Category?.CategoryName,
                        Price = order.ProductCustom.FlowerBasket.Price,
                        Decription = order.ProductCustom.FlowerBasket.Decription,
                        Feature = order.ProductCustom.FlowerBasket.Feature,
                        Status = order.ProductCustom.FlowerBasket.Status,
                        Sold = order.ProductCustom.FlowerBasket.Sold,
                        CreateAt = order.ProductCustom.FlowerBasket.CreateAt,
                        UpdateAt = order.ProductCustom.FlowerBasket.UpdateAt,
                    } : null,
                    styleResponse = order.ProductCustom.Style != null ? new StyleResponse
                    {
                        StyleId = order.ProductCustom.Style.StyleId,
                        Name = order.ProductCustom.Style.Name,
                        Description = order.ProductCustom.Style.Description,
                        Note = order.ProductCustom.Style.Note,
                        CategoryName = order.ProductCustom.Style.Category?.CategoryName,
                        Image = order.ProductCustom.Style.Image,
                        CreateAt = order.ProductCustom.Style.CreateAt,
                        UpdateAt = order.ProductCustom.Style.UpdateAt,
                        Status = order.ProductCustom.Style.Status,
                        Feature = order.ProductCustom.Style.Feature,
                    } : null,
                    accessoryResponse = order.ProductCustom.Accessory != null ? new AccessoryResponse
                    {
                        AccessoryId = order.ProductCustom.Accessory.AccessoryId,
                        Name = order.ProductCustom.Accessory.Name,
                        Note = order.ProductCustom.Accessory.Note,
                        Price = order.ProductCustom.Accessory.Price,
                        CategoryName = order.ProductCustom.Accessory.Category?.CategoryName,
                        Description = order.ProductCustom.Accessory.Description,
                        Image = order.ProductCustom.Accessory.Image,
                        Status = order.ProductCustom.Accessory.Status,
                        Feature = order.ProductCustom.Accessory.Feature,
                    } : null,
                    flowerCustomResponses = flowerCustoms
                    .Where(a => a.ProductCustomId == order.ProductCustom.ProductCustomId)
                                    .Select(a => new FlowerCustomResponse
                                    {
                                        FlowerCustomId = a.FlowerCustomId,
                                        FlowerId = a.FlowerId,
                                        Quantity = a.Quantity,
                                        TotalPrice = a.Price,
                                        CreateAt = a.CreateAt,
                                        UpdateAt = a.UpdateAt,
                                        Status = a.Status,
                                        flowerResponse = a.Flower != null ? new FlowerResponse
                                        {
                                            FlowerId = a.Flower.FlowerId,
                                            FlowerName = a.Flower.FlowerName,
                                            Price = a.Flower.Price,
                                            Color = a.Flower.Color,
                                            Image = a.Flower.Image,
                                            Quantity = a.Flower.Quantity,
                                            CategoryName = a.Flower.Category?.CategoryName,
                                            Description = a.Flower.Description,
                                            Sold = a.Flower.Sold,
                                            Feature = a.Flower.Feature,
                                            Status = a.Flower.Status,
                                        } : null
                                    }).ToList()
                } : null,
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
                        Quantity = orderDetail.Quantity ?? 0,
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


            var order = await _unitOfWork.GetRepo<Order>().Entities
             .Include(d => d.Promotion)
             .Include(o => o.OrderDetails)
                 .ThenInclude(od => od.Product)
                 .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                 .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                 .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                   .Include(fc => fc.Flower)
                                       .ThenInclude(f => f.Category)
                                   .ToListAsync();
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
                ProductCustomResponse = order.ProductCustom != null ? new ProductCustomResponse
                {
                    ProductCustomId = order.ProductCustomId,
                    ProductName = order.ProductCustom.ProductName,
                    Quantity = order.ProductCustom.Quantity,
                    TotalPrice = order.ProductCustom.TotalPrice,
                    CustomerId = order.ProductCustom.CustomerId,
                    CreateAt = order.ProductCustom.CreateAt,
                    UpdateAt = order.ProductCustom.UpdateAt,
                    Status = order.ProductCustom.Status,
                    flowerBasketResponse = order.ProductCustom.FlowerBasket != null ? new FlowerBasketResponse
                    {
                        FlowerBasketId = order.ProductCustom.FlowerBasket.FlowerBasketId,
                        FlowerBasketName = order.ProductCustom.FlowerBasket.FlowerBasketName,
                        MaxQuantity = order.ProductCustom.FlowerBasket.MaxQuantity,
                        MinQuantity = order.ProductCustom.FlowerBasket.MinQuantity,
                        Quantity = order.ProductCustom.FlowerBasket.Quantity,
                        Image = order.ProductCustom.FlowerBasket.Image,
                        CategoryName = order.ProductCustom.FlowerBasket.Category?.CategoryName,
                        Price = order.ProductCustom.FlowerBasket.Price,
                        Decription = order.ProductCustom.FlowerBasket.Decription,
                        Feature = order.ProductCustom.FlowerBasket.Feature,
                        Status = order.ProductCustom.FlowerBasket.Status,
                        Sold = order.ProductCustom.FlowerBasket.Sold,
                        CreateAt = order.ProductCustom.FlowerBasket.CreateAt,
                        UpdateAt = order.ProductCustom.FlowerBasket.UpdateAt,
                    } : null,
                    styleResponse = order.ProductCustom.Style != null ? new StyleResponse
                    {
                        StyleId = order.ProductCustom.Style.StyleId,
                        Name = order.ProductCustom.Style.Name,
                        Description = order.ProductCustom.Style.Description,
                        Note = order.ProductCustom.Style.Note,
                        CategoryName = order.ProductCustom.Style.Category?.CategoryName,
                        Image = order.ProductCustom.Style.Image,
                        CreateAt = order.ProductCustom.Style.CreateAt,
                        UpdateAt = order.ProductCustom.Style.UpdateAt,
                        Status = order.ProductCustom.Style.Status,
                        Feature = order.ProductCustom.Style.Feature,
                    } : null,
                    accessoryResponse = order.ProductCustom.Accessory != null ? new AccessoryResponse
                    {
                        AccessoryId = order.ProductCustom.Accessory.AccessoryId,
                        Name = order.ProductCustom.Accessory.Name,
                        Note = order.ProductCustom.Accessory.Note,
                        Price = order.ProductCustom.Accessory.Price,
                        CategoryName = order.ProductCustom.Accessory.Category?.CategoryName,
                        Description = order.ProductCustom.Accessory.Description,
                        Image = order.ProductCustom.Accessory.Image,
                        Status = order.ProductCustom.Accessory.Status,
                        Feature = order.ProductCustom.Accessory.Feature,
                    } : null,
                    flowerCustomResponses = flowerCustoms
                   .Where(a => a.ProductCustomId == order.ProductCustom.ProductCustomId)
                                   .Select(a => new FlowerCustomResponse
                                   {
                                       FlowerCustomId = a.FlowerCustomId,
                                       FlowerId = a.FlowerId,
                                       Quantity = a.Quantity,
                                       TotalPrice = a.Price,
                                       CreateAt = a.CreateAt,
                                       UpdateAt = a.UpdateAt,
                                       Status = a.Status,
                                       flowerResponse = a.Flower != null ? new FlowerResponse
                                       {
                                           FlowerId = a.Flower.FlowerId,
                                           FlowerName = a.Flower.FlowerName,
                                           Price = a.Flower.Price,
                                           Color = a.Flower.Color,
                                           Image = a.Flower.Image,
                                           Quantity = a.Flower.Quantity,
                                           CategoryName = a.Flower.Category?.CategoryName,
                                           Description = a.Flower.Description,
                                           Sold = a.Flower.Sold,
                                           Feature = a.Flower.Feature,
                                           Status = a.Flower.Status,
                                       } : null
                                   }).ToList()
                } : null,
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
                       Quantity = orderDetail.Quantity ?? 0,
                       OrderId = orderDetail.OrderId,
                       CreateAt = orderDetail.CreateAt,
                       UpdateAt = orderDetail.UpdateAt,
                       Status = orderDetail.Status
                   })
                   .ToList() ?? new List<OrderDetailsResponse>()
            };

            return orderResponse;
        }

        public async Task<IEnumerable<OrderResponse>> GetOrderByStaffId(Guid StaffId)
        {
           
            var orders = await _unitOfWork.GetRepo<Order>().Entities
               .Include(d => d.Promotion)
               .Include(o => o.OrderDetails)
                   .ThenInclude(od => od.Product)
                   .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                   .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                   .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
               .Where(order => order.StaffId == StaffId)
               .ToListAsync();

            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                   .Include(fc => fc.Flower)
                                       .ThenInclude(f => f.Category)
                                   .ToListAsync();

            var orderResponse = orders.Select(order => new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = order.CustomerId,
                ProductCustomId = order.ProductCustomId,
                ProductCustomResponse = order.ProductCustom != null ? new ProductCustomResponse
                {
                    ProductCustomId = order.ProductCustomId,
                    ProductName = order.ProductCustom.ProductName,
                    Quantity = order.ProductCustom.Quantity,
                    TotalPrice = order.ProductCustom.TotalPrice,
                    CustomerId = order.ProductCustom.CustomerId,
                    CreateAt = order.ProductCustom.CreateAt,
                    UpdateAt = order.ProductCustom.UpdateAt,
                    Status = order.ProductCustom.Status,
                    flowerBasketResponse = order.ProductCustom.FlowerBasket != null ? new FlowerBasketResponse
                    {
                        FlowerBasketId = order.ProductCustom.FlowerBasket.FlowerBasketId,
                        FlowerBasketName = order.ProductCustom.FlowerBasket.FlowerBasketName,
                        MaxQuantity = order.ProductCustom.FlowerBasket.MaxQuantity,
                        MinQuantity = order.ProductCustom.FlowerBasket.MinQuantity,
                        Quantity = order.ProductCustom.FlowerBasket.Quantity,
                        Image = order.ProductCustom.FlowerBasket.Image,
                        CategoryName = order.ProductCustom.FlowerBasket.Category?.CategoryName,
                        Price = order.ProductCustom.FlowerBasket.Price,
                        Decription = order.ProductCustom.FlowerBasket.Decription,
                        Feature = order.ProductCustom.FlowerBasket.Feature,
                        Status = order.ProductCustom.FlowerBasket.Status,
                        Sold = order.ProductCustom.FlowerBasket.Sold,
                        CreateAt = order.ProductCustom.FlowerBasket.CreateAt,
                        UpdateAt = order.ProductCustom.FlowerBasket.UpdateAt,
                    } : null,
                    styleResponse = order.ProductCustom.Style != null ? new StyleResponse
                    {
                        StyleId = order.ProductCustom.Style.StyleId,
                        Name = order.ProductCustom.Style.Name,
                        Description = order.ProductCustom.Style.Description,
                        Note = order.ProductCustom.Style.Note,
                        CategoryName = order.ProductCustom.Style.Category?.CategoryName,
                        Image = order.ProductCustom.Style.Image,
                        CreateAt = order.ProductCustom.Style.CreateAt,
                        UpdateAt = order.ProductCustom.Style.UpdateAt,
                        Status = order.ProductCustom.Style.Status,
                        Feature = order.ProductCustom.Style.Feature,
                    } : null,
                    accessoryResponse = order.ProductCustom.Accessory != null ? new AccessoryResponse
                    {
                        AccessoryId = order.ProductCustom.Accessory.AccessoryId,
                        Name = order.ProductCustom.Accessory.Name,
                        Note = order.ProductCustom.Accessory.Note,
                        Price = order.ProductCustom.Accessory.Price,
                        CategoryName = order.ProductCustom.Accessory.Category?.CategoryName,
                        Description = order.ProductCustom.Accessory.Description,
                        Image = order.ProductCustom.Accessory.Image,
                        Status = order.ProductCustom.Accessory.Status,
                        Feature = order.ProductCustom.Accessory.Feature,
                    } : null,
                    flowerCustomResponses = flowerCustoms
                    .Where(a => a.ProductCustomId == order.ProductCustom.ProductCustomId)
                                    .Select(a => new FlowerCustomResponse
                                    {
                                        FlowerCustomId = a.FlowerCustomId,
                                        FlowerId = a.FlowerId,
                                        Quantity = a.Quantity,
                                        TotalPrice = a.Price,
                                        CreateAt = a.CreateAt,
                                        UpdateAt = a.UpdateAt,
                                        Status = a.Status,
                                        flowerResponse = a.Flower != null ? new FlowerResponse
                                        {
                                            FlowerId = a.Flower.FlowerId,
                                            FlowerName = a.Flower.FlowerName,
                                            Price = a.Flower.Price,
                                            Color = a.Flower.Color,
                                            Image = a.Flower.Image,
                                            Quantity = a.Flower.Quantity,
                                            CategoryName = a.Flower.Category?.CategoryName,
                                            Description = a.Flower.Description,
                                            Sold = a.Flower.Sold,
                                            Feature = a.Flower.Feature,
                                            Status = a.Flower.Status,
                                        } : null
                                    }).ToList()
                } : null,
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
                        Quantity = orderDetail.Quantity ?? 0,
                        OrderId = orderDetail.OrderId,
                        CreateAt = orderDetail.CreateAt,
                        UpdateAt = orderDetail.UpdateAt,
                        Status = orderDetail.Status
                    })
                    .ToList() ?? new List<OrderDetailsResponse>()
            });

            return orderResponse;
        }
        public async Task<IEnumerable<EmployeeResponse>> GetStaffForOrderId(Guid orderId)
        {
            // Lấy danh sách nhân viên có Role là "florist" và có trạng thái hoạt động
            var employees = await _unitOfWork.GetRepo<Employee>().Entities
                .Include(n => n.Role)
                .Where(m => m.Role.RoleName == "florist" && m.Status == true)
                .ToListAsync();

            var availableEmployees = new List<EmployeeResponse>();

            foreach (var employee in employees)
            {
                // Lấy danh sách đơn hàng thuộc về cửa hàng của nhân viên
                var orders = await _unitOfWork.GetRepo<Order>().Entities
                    .Where(a => a.StoreId == employee.StoreId && a.StaffId == employee.EmployeeId && a.RecipientTime != null && a.Status == "đặt hàng thành công")
                    .ToListAsync();

                // Nhóm các đơn hàng theo thời gian nhận hàng
                var groupedOrders = orders.GroupBy(o => o.RecipientTime.Value)
                                          .Where(g => g.Count() > 10) // Kiểm tra nếu có hơn 10 đơn trong cùng một khoảng thời gian
                                          .ToList();

                if (groupedOrders.Count == 0) // Chỉ lấy nhân viên chưa bị quá tải
                {
                    availableEmployees.Add(new EmployeeResponse
                    {
                        EmployeeId = employee.EmployeeId,
                        FullName = employee.FullName,
                        Address = employee.Address,
                        Email = employee.Email,
                        Phone = employee.Phone
                    });
                }
            }

            return availableEmployees;
        }
        public async Task<IEnumerable<OrderResponse>> GetOrderByStoreID(Guid StoreID)
        {
         
            var orders = await _unitOfWork.GetRepo<Order>().Entities
              .Include(d => d.Promotion)
              .Include(o => o.OrderDetails)
                  .ThenInclude(od => od.Product)
                  .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                  .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                  .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
              .Where(order => order.StoreId == StoreID && order.Status == "đặt hàng thành công")
              .ToListAsync();

            var orderDetail = await _unitOfWork.Repository<OrderDetail>().GetAllAsync();
            var productImage = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                  .Include(fc => fc.Flower)
                                      .ThenInclude(f => f.Category)
                                  .ToListAsync();
            var orderResponse = orders.Select(order => new OrderResponse
            {
                OrderId = order.OrderId,
                OrderPrice = order.OrderPrice,
                CustomerId = order.CustomerId,
                ProductCustomId = order.ProductCustomId,
                ProductCustomResponse = order.ProductCustom != null ? new ProductCustomResponse
                {
                    ProductCustomId = order.ProductCustomId,
                    ProductName = order.ProductCustom.ProductName,
                    Quantity = order.ProductCustom.Quantity,
                    TotalPrice = order.ProductCustom.TotalPrice,
                    CustomerId = order.ProductCustom.CustomerId,
                    CreateAt = order.ProductCustom.CreateAt,
                    UpdateAt = order.ProductCustom.UpdateAt,
                    Status = order.ProductCustom.Status,
                    flowerBasketResponse = order.ProductCustom.FlowerBasket != null ? new FlowerBasketResponse
                    {
                        FlowerBasketId = order.ProductCustom.FlowerBasket.FlowerBasketId,
                        FlowerBasketName = order.ProductCustom.FlowerBasket.FlowerBasketName,
                        MaxQuantity = order.ProductCustom.FlowerBasket.MaxQuantity,
                        MinQuantity = order.ProductCustom.FlowerBasket.MinQuantity,
                        Quantity = order.ProductCustom.FlowerBasket.Quantity,
                        Image = order.ProductCustom.FlowerBasket.Image,
                        CategoryName = order.ProductCustom.FlowerBasket.Category?.CategoryName,
                        Price = order.ProductCustom.FlowerBasket.Price,
                        Decription = order.ProductCustom.FlowerBasket.Decription,
                        Feature = order.ProductCustom.FlowerBasket.Feature,
                        Status = order.ProductCustom.FlowerBasket.Status,
                        Sold = order.ProductCustom.FlowerBasket.Sold,
                        CreateAt = order.ProductCustom.FlowerBasket.CreateAt,
                        UpdateAt = order.ProductCustom.FlowerBasket.UpdateAt,
                    } : null,
                    styleResponse = order.ProductCustom.Style != null ? new StyleResponse
                    {
                        StyleId = order.ProductCustom.Style.StyleId,
                        Name = order.ProductCustom.Style.Name,
                        Description = order.ProductCustom.Style.Description,
                        Note = order.ProductCustom.Style.Note,
                        CategoryName = order.ProductCustom.Style.Category?.CategoryName,
                        Image = order.ProductCustom.Style.Image,
                        CreateAt = order.ProductCustom.Style.CreateAt,
                        UpdateAt = order.ProductCustom.Style.UpdateAt,
                        Status = order.ProductCustom.Style.Status,
                        Feature = order.ProductCustom.Style.Feature,
                    } : null,
                    accessoryResponse = order.ProductCustom.Accessory != null ? new AccessoryResponse
                    {
                        AccessoryId = order.ProductCustom.Accessory.AccessoryId,
                        Name = order.ProductCustom.Accessory.Name,
                        Note = order.ProductCustom.Accessory.Note,
                        Price = order.ProductCustom.Accessory.Price,
                        CategoryName = order.ProductCustom.Accessory.Category?.CategoryName,
                        Description = order.ProductCustom.Accessory.Description,
                        Image = order.ProductCustom.Accessory.Image,
                        Status = order.ProductCustom.Accessory.Status,
                        Feature = order.ProductCustom.Accessory.Feature,
                    } : null,
                    flowerCustomResponses = flowerCustoms
                   .Where(a => a.ProductCustomId == order.ProductCustom.ProductCustomId)
                                   .Select(a => new FlowerCustomResponse
                                   {
                                       FlowerCustomId = a.FlowerCustomId,
                                       FlowerId = a.FlowerId,
                                       Quantity = a.Quantity,
                                       TotalPrice = a.Price,
                                       CreateAt = a.CreateAt,
                                       UpdateAt = a.UpdateAt,
                                       Status = a.Status,
                                       flowerResponse = a.Flower != null ? new FlowerResponse
                                       {
                                           FlowerId = a.Flower.FlowerId,
                                           FlowerName = a.Flower.FlowerName,
                                           Price = a.Flower.Price,
                                           Color = a.Flower.Color,
                                           Image = a.Flower.Image,
                                           Quantity = a.Flower.Quantity,
                                           CategoryName = a.Flower.Category?.CategoryName,
                                           Description = a.Flower.Description,
                                           Sold = a.Flower.Sold,
                                           Feature = a.Flower.Feature,
                                           Status = a.Flower.Status,
                                       } : null
                                   }).ToList()
                } : null,
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
                       Quantity = orderDetail.Quantity ?? 0,
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
