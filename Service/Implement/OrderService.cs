using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Check;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.DesignCustom;
using BusinessObject.DTO.Employee;
using BusinessObject.DTO.FailOrder;
using BusinessObject.DTO.Flower;
using BusinessObject.DTO.FlowerBasket;
using BusinessObject.DTO.FlowerCustom;
using BusinessObject.DTO.Message;
using BusinessObject.DTO.Order;
using BusinessObject.DTO.OrderDetails;
using BusinessObject.DTO.Product;
using BusinessObject.DTO.ProductCustom;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using MailKit.Search;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit.Cryptography;
using Newtonsoft.Json;
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
using static Service.Implement.CheckService;
using static System.Net.Mime.MediaTypeNames;

namespace Service.Implement
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatRoomService _chatRoomService;
        private readonly IMessageService _messageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _googleApiKey;
        private readonly INotiService _notiService;
        private readonly ILogger<OrderService> _logger;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IWalletService _walletService;
        public OrderService(IUnitOfWork unitOfWork, IConfiguration configuration, IChatRoomService chatRoomService, IMessageService messageService, IHttpClientFactory httpClientFactory, INotiService notiService, ILogger<OrderService> logger, CloudinaryService cloudinaryService,IWalletService walletService)
        {
            _unitOfWork = unitOfWork;
            _chatRoomService = chatRoomService;
            _messageService = messageService;
            _httpClientFactory = httpClientFactory;
            _googleApiKey = configuration["GoogleMaps:ApiKey"]; // Lấy từ config
            _notiService = notiService;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _walletService = walletService;
        }



        public async Task AutoUpdateOrder()
        {
            var orders = (await _unitOfWork.Repository<Order>().GetAllAsync())
                .Where(o => o.Status == "Order Successfully" && o.StaffId == null)
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
                order.UpdateAt = DateTime.Now;
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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            // Tạo đơn hàng
            var order = new Order
            {
                RecipientName = orderRequest.RecipientName,
                CustomerId = customerId,
                StoreId = orderRequest.StoreId,
                DeliveryDistrict = orderRequest.DeliveryDistrict,
                DeliveryCity = orderRequest.DeliveryCity,
                DeliveryAddress = orderRequest.DeliveryAddress,
                Note = orderRequest.Note,
                RecipientTime = orderRequest.RecipientTime,
                Phone = orderRequest.Phone,
                Transfer = orderRequest.Transfer,
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Delivery = orderRequest.Delivery,
                Refund = false,
                Status = "Pending Payment",
                PromotionId = orderRequest.PromotionId,
                Wallet = orderRequest.Wallet,

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
                    CreateAt = vietnamTime,
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
            if (orderRequest.Delivery == true)
            {
                string deliveryAddress = $"{orderRequest.DeliveryAddress}, {orderRequest.DeliveryDistrict}, {orderRequest.DeliveryCity}";
                var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(orderRequest.StoreId);
                string StoreAddress = store.Address + "," + store.District + "," + store.City;
                double distance = await GetDistanceFromGoogleAsync(StoreAddress, deliveryAddress);
                double? shipperMoney = 0.0;
                if (distance < 5)
                {
                    shipperMoney = 10000 + distance * 5000;
                    totalPrice = shipperMoney + totalPrice;
                }
                else if (distance > 5)
                {
                    totalPrice = totalPrice;
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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
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
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Refund = false,
                Status = "Pending Payment",
                PromotionId = PromotionID,
                Delivery = Delivery,
                Wallet = orderRequest.Wallet,
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
                        CreateAt = vietnamTime,
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
                if (Delivery == true)
                {
                    string deliveryAddress = $"{DeliveryAddress}, {DeliveryDistrict}, {DeliveryCity}";
                    var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(orderRequest.StoreId);
                    string StoreAddress = store.Address + "," + store.District + "," + store.City;
                    double distance = await GetDistanceFromGoogleAsync(StoreAddress, deliveryAddress);
                    double? shipperMoney = 0.0;
                    if (distance < 5)
                    {
                        shipperMoney = 10000 + distance * 5000;
                        totalPrice = shipperMoney + totalPrice;
                    }
                    else if (distance > 5)
                    {
                        totalPrice = totalPrice;
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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
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
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Refund = false,
                Status = "Pending Payment",
                PromotionId = PromotionID,
                Delivery = Delivery,
                Wallet = orderCustomRequest.Wallet,

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
            if (Delivery == true)
            {
                string deliveryAddress = $"{DeliveryAddress}, {DeliveryDistrict}, {DeliveryCity}";
                var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(orderCustomRequest.StoreId);
                string StoreAddress = store.Address + "," + store.District + "," + store.City;
                double distance = await GetDistanceFromGoogleAsync(StoreAddress, deliveryAddress);
                double? shipperMoney = 0.0;
                if (distance < 5)
                {
                    shipperMoney = 10000 + distance * 5000;
                    totalPrice = shipperMoney + totalPrice;
                }
                else if (distance > 5)
                {
                    totalPrice = totalPrice;
                }


            }
            order.OrderPrice = totalPrice;
            productCustom.OrderId = order.OrderId;

            _unitOfWork.Repository<ProductCustom>().Update(productCustom);
            await _unitOfWork.CompleteAsync();

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return order;
        }
        private async Task<double> GetDistanceFromGoogleAsync(string origin, string destination)
        {
            const string API_CONNECTION_ERROR = "Lỗi kết nối với Google Maps API";
            const string INVALID_RESPONSE_FORMAT = "Định dạng phản hồi từ Google không hợp lệ";
            const string GOOGLE_API_ERROR = "Google Maps API trả về lỗi";
            const string CALCULATION_SUCCESS = "Tính khoảng cách thành công";
            const string UNKNOWN_ERROR = "Lỗi không xác định khi tính khoảng cách";
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                string url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                            $"?origins={Uri.EscapeDataString(origin)}" +
                            $"&destinations={Uri.EscapeDataString(destination)}" +
                            $"&key={_googleApiKey}" +
                            $"&region=vn&language=vi";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string statusCode = response.StatusCode.ToString();
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return  0;
                }
                var content = await response.Content.ReadAsStringAsync();
                var distanceMatrix = JsonConvert.DeserializeObject<GoogleDistanceMatrixResponse>(content);

                if (distanceMatrix?.rows == null ||
                    distanceMatrix.rows.Count == 0 ||
                    distanceMatrix.rows[0].elements == null ||
                    distanceMatrix.rows[0].elements.Count == 0)
                {
                    return (0);
                }

                var element = distanceMatrix.rows[0].elements[0];
                if (element.status != "OK")
                {
                    return  0;
                }

                double meters = element.distance.value;
                return  meters / 1000.0; // Convert to km
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting distance from Google Maps");
                return 0;
            }
        }
        public class GoogleDistanceMatrixResponse
        {
            public List<Row> rows { get; set; }
            public string status { get; set; }
            public string error_message { get; set; }

            public class Row
            {
                public List<Element> elements { get; set; }
            }

            public class Element
            {
                public DistanceInfo distance { get; set; }
                public DurationInfo duration { get; set; }
                public string status { get; set; }
            }

            public class DistanceInfo
            {
                public string text { get; set; }
                public int value { get; set; } // in meters
            }

            public class DurationInfo
            {
                public string text { get; set; }
                public int value { get; set; } // in seconds
            }
        }
        public async Task DeleteOrder(Guid orderID)
        {
            var order = await _unitOfWork.Repository<Order>().Entities.Include(m => m.ProductCustom)
                .Include(n => n.OrderDetails).FirstOrDefaultAsync(a => a.OrderId == orderID);
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found");
            }
            if (order.ProductCustomId == null)
            {

                // Lấy danh sách OrderDetail liên quan
                var orderDetails = (await _unitOfWork.Repository<OrderDetail>().GetAllAsync()).Where(a => a.OrderId == order.OrderId);
                _unitOfWork.Repository<OrderDetail>().DeleteRange(orderDetails);
                _unitOfWork.Repository<Order>().Delete(order);
                await _unitOfWork.CompleteAsync();
            }
            // Nếu có ProductCustomId, kiểm tra và xóa ProductCustom
            else if (order.ProductCustomId != null)
            {
                var productCustom = await _unitOfWork.Repository<ProductCustom>()
                    .Entities
                    .Include(n => n.FlowerCustoms)
                    .FirstOrDefaultAsync(m => m.ProductCustomId == order.ProductCustomId);

                var flowerCustom = await _unitOfWork.Repository<FlowerCustom>().Entities.Where(n => n.ProductCustomId == productCustom.ProductCustomId).ToListAsync();



                _unitOfWork.Repository<ProductCustom>().Delete(productCustom);
                _unitOfWork.Repository<FlowerCustom>().DeleteRange(flowerCustom);
                _unitOfWork.Repository<Order>().Delete(order);
                await _unitOfWork.CompleteAsync();
            }
            // Lưu thay đổi vào database
        }

        public async Task<IEnumerable<OrderResponse>> GetFailOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
               .Include(d => d.Promotion)
               .Include(o => o.OrderDetails)
                   .ThenInclude(od => od.Product)
                   .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                   .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                   .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                   .OrderByDescending(n => n.CreateAt)
               .Where(order => order.CustomerId == CustomerID &&  order.Status == "Pending Payment")
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
                Delivery = order.Delivery,
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
        public async Task<IEnumerable<OrderResponse>> GetCanelOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
           .Include(d => d.Promotion)
           .Include(o => o.OrderDetails)
               .ThenInclude(od => od.Product)
               .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
               .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
               .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                                  .OrderByDescending(n => n.UpdateAt)

           .Where(order => order.CustomerId == CustomerID && order.Status == "Cancel")
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
                Delivery = order.Delivery,
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
        public async Task<IEnumerable<OrderResponse>> GetRefundOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
           .Include(d => d.Promotion)
           .Include(o => o.OrderDetails)
               .ThenInclude(od => od.Product)
               .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
               .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
               .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                                  .OrderByDescending(n => n.CreateAt)

          .Where(order =>
    order.CustomerId == CustomerID
    && order.Status != "Cancel"
    && order.Status != "Pending Payment"
    && (
        order.Status == "Request refund"
        || order.Status == "Accept refund"
        || order.Status == "Refuse refund"
    )
).OrderByDescending(n => n.CreateAt)
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
                Delivery = order.Delivery,
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
        public async Task<IEnumerable<OrderResponse>> GetOrderByCustomerId(Guid CustomerID)
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
                .Include(d => d.Promotion)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude( b => b.Category)
                    .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                    .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                    .Include(p => p.DesignCustom)
                                       .OrderByDescending(n => n.CreateAt)

                .Where(order => order.CustomerId == CustomerID  && order.Status != "Pending Payment" && order.Status != "Request refund" && order.Status != "Accept refund" && order.Status != "Refuse refund" && order.Status != "Cancel")
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
                Delivery = order.Delivery,
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
                    productCustomImage = order.ProductCustom.productCustomImage,
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
                DesignCustomBuCustomerResponse = order.DesignCustom != null ? new DesignCustomBuCustomerResponse
                {
                    DesignCustomId = order.DesignCustom.DesignCustomId,
                    RequestImage = order.DesignCustom.RequestImage,
                    RequestDescription = order.DesignCustom.RequestDescription,
                    RequestPrice = order.DesignCustom.RequestPrice,
                    RequestOccasion = order.DesignCustom.RequestOccasion,
                    RequestMainColor = order.DesignCustom.RequestMainColor,
                    RequestFlowerType = order.DesignCustom.RequestFlowerType,
                    RequestCard = order.DesignCustom.RequestCard,
                    ResponsePrice = order.DesignCustom.ResponsePrice,
                    ResponseImage = order.DesignCustom.ResponseImage,
                    ResponseDescription = order.DesignCustom.ResponseDescription,
                    Status = order.DesignCustom.Status,
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
                .Include(m => m.ProductCustom)
    .ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
.Include(c => c.ProductCustom)
    .ThenInclude(a => a.Style).ThenInclude(l => l.Category)
.Include(e => e.ProductCustom)
    .ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                 .Include(p => p.Staff).Include(p => p.DesignCustom)
                                    .OrderByDescending(n => n.CreateAt)

                .FirstOrDefaultAsync(o => o.OrderId == OrderId);
            var delivery = await _unitOfWork.GetRepo<Delivery>().Entities.Include(m => m.Shipper).Where(n => n.OrderId == OrderId).ToListAsync();
            var payment = await _unitOfWork.GetRepo<Payment>().Entities.FirstOrDefaultAsync(m => m.OrderId == OrderId);
            var flowerCustoms = await _unitOfWork.Repository<FlowerCustom>().Entities
                                   .Include(fc => fc.Flower)
                                       .ThenInclude(f => f.Category)
                                   .ToListAsync();
            var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(order.StoreId);

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
                Delivery = order.Delivery,
                DeliveryId = delivery.FirstOrDefault()?.DeliveryId, // Hiển thị DeliveryId
                ShipperId =delivery.FirstOrDefault()?.Shipper.EmployeeId,
                ShipperName = delivery.FirstOrDefault()?.Shipper.FullName,
                ShipperEmail = delivery.FirstOrDefault()?.Shipper.Email,
                ShipperPhone = delivery.FirstOrDefault()?.Shipper.Phone,
                NumberMoto =delivery.FirstOrDefault()?.Shipper.NumberMoto,
                ColorMoto = delivery.FirstOrDefault()?.Shipper.ColorMoto,
                MotoType = delivery.FirstOrDefault()?.Shipper.MotoType,

                ProductCustomResponse = order.ProductCustom != null ? new ProductCustomResponse
                {
                    ProductCustomId = order.ProductCustomId,
                    ProductName = order.ProductCustom.ProductName,
                    Description = order.ProductCustom.Description,
                    Quantity = order.ProductCustom.Quantity,
                    TotalPrice = order.ProductCustom.TotalPrice,
                    CustomerId = order.ProductCustom.CustomerId,
                    CreateAt = order.ProductCustom.CreateAt,
                    UpdateAt = order.ProductCustom.UpdateAt,
                    Status = order.ProductCustom.Status,
                    productCustomImage = order.ProductCustom.productCustomImage,

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
                DesignCustomBuCustomerResponse = order.DesignCustom != null ? new DesignCustomBuCustomerResponse
                {
                    DesignCustomId = order.DesignCustom.DesignCustomId,
                    RequestImage = order.DesignCustom.RequestImage,
                    RequestDescription = order.DesignCustom.RequestDescription,
                    RequestPrice = order.DesignCustom.RequestPrice,
                    RequestOccasion = order.DesignCustom.RequestOccasion,
                    RequestMainColor = order.DesignCustom.RequestMainColor,
                    RequestFlowerType = order.DesignCustom.RequestFlowerType,
                    RequestCard = order.DesignCustom.RequestCard,
                    ResponsePrice = order.DesignCustom.ResponsePrice,
                    ResponseImage = order.DesignCustom.ResponseImage,
                    ResponseDescription = order.DesignCustom.ResponseDescription,
                    Status = order.DesignCustom.Status,
                } : null,
                StaffId = order.StaffId,
                StaffFullName = order.Staff?.FullName ?? "N/A",
                StaffEmail = order.Staff?.Email ?? "N/A",
                StaffPhone = order.Staff?.Phone ?? "N/A",   
                PromotionId = order.PromotionId,
                PromotionName = order.Promotion?.PromotionName,
                PromotionDiscount = order.Promotion?.PromotionDiscount ?? 0,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDistrict = order.DeliveryDistrict,
                DeliveryCity = order.DeliveryCity,
                StoreId = order.StoreId,
                StoreName =  store.StoreName,
                StoreAddress = store.Address +"," + store.District + "," + store.City,
                Note = order.Note,
                DeliveryDateTime = order.RecipientTime,
                Phone = order.Phone,
                Transfer = order.Transfer,
                PaymentId = payment?.PaymentId ,
                PaymentCreateAt = payment?.CreateAt,
                PaymentPrice = payment?.TotalPrice,
                PaymentStatus = payment?.Status,
                PaymentMethod = payment?.Method,
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
                 .Where(order => order.StaffId == StaffId && order.Status != "Pending Payment" && order.Status != "Request refund" && order.Status != "Accept refund" && order.Status != "Refuse refund")
                                 .OrderByDescending(n => n.CreateAt)
                                  .OrderByDescending(n => n.CreateAt)

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
                Delivery = order.Delivery,

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
        public async Task<IEnumerable<OrderResponse>> GetRefundOrderByStaffId(Guid StaffId)
        {

            var orders = await _unitOfWork.GetRepo<Order>().Entities
               .Include(d => d.Promotion)
               .Include(o => o.OrderDetails)
                   .ThenInclude(od => od.Product)
                   .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                   .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                   .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)

 .Where(order =>
    order.StaffId == StaffId
    && order.Status != "Pending Payment"
    && (
        order.Status == "Request refund"
        || order.Status == "Accept refund"
        || order.Status == "Refuse refund"
    )
)
.OrderByDescending(n => n.CreateAt)
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
                Delivery = order.Delivery,

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
            // Lấy thông tin đơn hàng dựa vào orderId
            var order = await _unitOfWork.GetRepo<Order>().Entities
                .Where(o => o.OrderId == orderId)
                .Select(o => new { o.StoreId })
                .FirstOrDefaultAsync();

            if (order == null)
                return new List<EmployeeResponse>();

            // Lấy danh sách nhân viên của cửa hàng thuộc đơn hàng đó
            var employees = await _unitOfWork.GetRepo<Employee>().Entities
                .Include(n => n.User.Role)
                .Where(m => m.User.Role.RoleName == "florist" && m.Status == true && m.StoreId == order.StoreId)
                .ToListAsync();

            var availableEmployees = new List<EmployeeResponse>();

            foreach (var employee in employees)
            {
                // Lấy danh sách đơn hàng thuộc về cửa hàng của nhân viên đó
                var orders = await _unitOfWork.GetRepo<Order>().Entities
                    .Where(a => a.StoreId == employee.StoreId && a.StaffId == employee.EmployeeId && a.RecipientTime != null && a.Status != "Received")
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

        public async Task<IEnumerable<EmployeeResponse>> GetDeliveryForOrderId(Guid orderId)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities
              .Where(o => o.OrderId == orderId)
              .Select(o => new { o.StoreId })
              .FirstOrDefaultAsync();

            if (order == null)
                return new List<EmployeeResponse>();

            // Lấy danh sách nhân viên của cửa hàng thuộc đơn hàng đó
            var employees = await _unitOfWork.GetRepo<Employee>().Entities
                .Include(n => n.User.Role)
                .Where(m => m.User.Role.RoleName == "Courier" && m.Status == true && m.StoreId == order.StoreId)
                .ToListAsync();

            var availableEmployees = new List<EmployeeResponse>();

            foreach (var employee in employees)
            {
                // Lấy danh sách đơn hàng thuộc về cửa hàng của nhân viên đó
                var orders = await _unitOfWork.GetRepo<Order>().Entities
                    .Where(a => a.StoreId == employee.StoreId && a.StaffId == employee.EmployeeId && a.RecipientTime != null && a.Status != "Received")
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
              .Where(order => order.StoreId == StoreID && order.Status != "Pending Payment" && order.Status != "Request refund" && order.Status != "Accept refund" && order.Status != "Refuse refund")
                                 .OrderByDescending(n => n.CreateAt)

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
                Delivery = order.Delivery,

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

        public async Task<IEnumerable<OrderResponse>> GetRefundOrderByStoreID(Guid StoreID)
        {

            var orders = await _unitOfWork.GetRepo<Order>().Entities
              .Include(d => d.Promotion)
              .Include(o => o.OrderDetails)
                  .ThenInclude(od => od.Product)
                  .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                  .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                  .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
              .Where(order =>
    order.StoreId == StoreID &&
    order.Status != "Pending Payment" &&
    (
        order.Status == "Request refund" ||
        order.Status == "Accept refund" ||
        order.Status == "Refuse refund"
    )
)
.OrderByDescending(n => n.CreateAt)
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
                Delivery = order.Delivery,

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

        public async Task<IEnumerable<OrderResponse>> GetFailOrderByStoreID(Guid StoreID)
        {

            var orders = await _unitOfWork.GetRepo<Order>().Entities
              .Include(d => d.Promotion)
              .Include(o => o.OrderDetails)
                  .ThenInclude(od => od.Product)
                  .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                  .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                  .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
              .Where(order => order.StoreId == StoreID && order.Status == "Fail")
                                 .OrderByDescending(n => n.CreateAt)

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
                Delivery = order.Delivery,

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
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            order.DeliveryDistrict = orderRequest.DeliveryDistrict ?? order.DeliveryDistrict;
            order.DeliveryCity = orderRequest.DeliveryCity ?? order.DeliveryCity;
            order.DeliveryAddress = orderRequest.DeliveryAddress ?? order.DeliveryAddress;
            order.Note= orderRequest.Note ?? order.Note;
            order.RecipientTime = orderRequest.RecipientTime ?? order.RecipientTime;
            order.Phone= orderRequest.Phone ?? order.Phone;
            order.Transfer = orderRequest.Transfer ?? order.Transfer;
            order.Status = orderRequest.Status ?? order.Status;
            order.UpdateAt = vietnamTime;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateOrderByStoreId(Guid orderId, Guid StaffId)
        {
            var order = await _unitOfWork.GetRepo<Order>()
               .Entities
               .Include(d => d.Promotion)
               .Include(o => o.OrderDetails)
               .ThenInclude(od => od.Product)
               .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new Exception($"Order {orderId} not found.");
            }
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            order.StaffId = StaffId;
            order.UpdateAt = vietnamTime;
            order.Status = "Arranging & Packing";

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            // Tạo ChatRoom
            var chatRoomRequest = new CreateChatRoomRequest { OrderId = orderId };
            var chatRoomResult = await _chatRoomService.CreateChatRoom(chatRoomRequest);

            if (chatRoomResult.ResultStatus != ResultStatus.Success.ToString())
            {
                throw new Exception($"Failed to create chat room: {string.Join(", ", chatRoomResult.Messages)}");
            }

            if (chatRoomResult.Data == null)
            {
                return; // Không ném Exception ngay, tránh làm lỗi toàn bộ hàm.
            }

            var chatRoom = chatRoomResult.Data;

            // Tạo tin nhắn
            var messageRequest = new CreateMessageRequest
            {
                ChatRoomId = chatRoom.ChatRoomId,
                SenderId = StaffId,
                ReceiveId = order.CustomerId,
                MessageType = "text",
                Content = $"Hello, I am the employee with ID number: {StaffId}, I will serve your order. We will consult your order via this chat channel."
            };

            var messageResult = await _messageService.SendMessage(messageRequest);

            if (messageResult.ResultStatus != ResultStatus.Success.ToString() || messageResult.Data == null)
            {
                return;
            }
            await _unitOfWork.CompleteAsync();
            try
            {
                var notification = new Noti
                {
                    ToUserId = StaffId,
                    Message = $"bạn có một đơn hàng cần xử lý",
                    Type = "Order",
                    RelatedId = orderId,
                };

                // Giả sử có _notiService được inject vào class
                await _notiService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không ảnh hưởng đến luồng chính
                Console.WriteLine($"Failed to send notification: {ex.Message}");
                // Hoặc sử dụng ILogger nếu có
                // _logger.LogError(ex, "Failed to send notification");
            }

        }



        public async Task UpdateStatusOrderByStaffId(Guid orderId, string Status)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities.Include(d => d.Promotion).Include(o => o.OrderDetails).ThenInclude(od => od.Product).FirstOrDefaultAsync(o => o.OrderId == orderId);
            order.Status = Status ?? order.Status;
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            order.UpdateAt = vietnamTime;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<OrderResponse>> GetOrder()
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
           .Include(d => d.Promotion)
           .Include(o => o.OrderDetails)
               .ThenInclude(od => od.Product)
               .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
               .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
               .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
           .Where(order =>  order.Status != "Pending Payment" && order.Status != "Request refund" && order.Status != "Accept refund" && order.Status != "Refuse refund")
                              .OrderByDescending(n => n.CreateAt)

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
                Delivery = order.Delivery,

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

        public async Task<IEnumerable<OrderResponse>> GetRefundOrder()
        {
            var orders = await _unitOfWork.GetRepo<Order>().Entities
              .Include(d => d.Promotion)
              .Include(o => o.OrderDetails)
                  .ThenInclude(od => od.Product)
                  .Include(m => m.ProductCustom).ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                  .Include(c => c.ProductCustom).ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                  .Include(e => e.ProductCustom).ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
              .Where(order =>
    order.Status != "Pending Payment" &&
    (
        order.Status == "Request refund" ||
        order.Status == "Accept refund" ||
        order.Status == "Refuse refund"
    )
)
.OrderByDescending(n => n.CreateAt)
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
                Delivery = order.Delivery,

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

        public async Task UpdateFailOrder(FailOrderRequest failOrderRequest, Guid orderId)
        {
            var order = await _unitOfWork.GetRepo<Order>().Entities
             .Include(d => d.Promotion)
             .Include(o => o.OrderDetails)
                 .ThenInclude(od => od.Product)
                .Include(m => m.ProductCustom)
                 .ThenInclude(a => a.FlowerBasket).ThenInclude(b => b.Category)
                .Include(c => c.ProductCustom)
                 .ThenInclude(a => a.Style).ThenInclude(l => l.Category)
                .Include(e => e.ProductCustom)
                .ThenInclude(f => f.Accessory).ThenInclude(g => g.Category)
                 .Include(p => p.Staff)
                                    .OrderByDescending(n => n.CreateAt)

                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            var walletAdmin = await _unitOfWork.Repository<Wallet>().Entities
             .FirstOrDefaultAsync(n => n.WalletId == Guid.Parse("55d9964b-8543-4b74-96d6-e0ab2ce86d3f"));
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(order.CustomerId);
            var wallet = (await _unitOfWork.Repository<Wallet>().GetAllAsync()).FirstOrDefault(n => n.CustomerId == customer.CustomerId);
            var delivery = await _unitOfWork.Repository<Delivery>().Entities.Where(k => k.OrderId == order.OrderId).FirstOrDefaultAsync();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var folderName = $"Product";
          
            string Url = null;
            if (failOrderRequest != null && failOrderRequest.ImageFail != null)
            {
                using var stream = failOrderRequest.ImageFail.OpenReadStream();
                if (stream != null)
                {
                    Url = await _cloudinaryService.UploadImageAsync(stream, folderName);
                }
            }
            order.Status = "Fail";
            var payment = (await _unitOfWork.Repository<Payment>().GetAllAsync()).FirstOrDefault(n => n.OrderId == orderId);

            _unitOfWork.GetRepo<Order>().Update(order);
            double? price = order.OrderPrice;
            if (order.Transfer == false)
            {
                price = price / 2;
            }
            bool a = await _walletService.CheckWallet(customer.CustomerId);
           
            var failOrder = new FailOrder
            {
                OrderId = order.OrderId,
                ReasonFail = failOrderRequest.ReasonFail,
                TimeDelay = failOrderRequest.TimeDelay,
                ImageFail = Url,
                Wallet = failOrderRequest.Wallet,
                RefundPrice = price,
                StaffId = order.StaffId,
                DeliveryId = delivery?.DeliveryId, // Gán luôn nếu có
                ShipperId = delivery?.ShipperId ,   // Gán luôn nếu có
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Status = "Successfull",
            };
            if (a == false)
            {
                failOrder.Wallet = false;
            }
            if (failOrder.Wallet == true)
            {
                var refund = new Refund
                {
                    OrderId = order.OrderId,
                    WallerId = wallet.WalletId,
                    Price = price,
                    CreateAt = vietnamTime,
                    Status = "Refund Successfull",
                    StoreId = order.StoreId,
                };
                await _unitOfWork.Repository<Refund>().AddAsync(refund);
                var inComWallet = new IncomeWallet
                {
                    WalletID = wallet.WalletId,
                    IncomePrice = price,
                    Method = "Refund",
                    Status = "Successfull",
                    CreateAt = vietnamTime,
                    UpdateAt = vietnamTime,
                    OrderId = order.OrderId,

                };
                await _unitOfWork.Repository<IncomeWallet>().AddAsync(inComWallet);
                await _unitOfWork.CompleteAsync();
                walletAdmin.TotalPrice -= inComWallet.IncomePrice;
                _unitOfWork.Repository<Wallet>().Update(walletAdmin);
                var incomeWallet = new IncomeWallet
                {
                    WalletID = walletAdmin.WalletId,
                    IncomePrice = -inComWallet.IncomePrice,
                    Method = "Refund",
                    Status = "Successfull",
                    CreateAt = vietnamTime,
                    UpdateAt = vietnamTime,
                };
                await _unitOfWork.GetRepo<IncomeWallet>().AddAsync(incomeWallet);
                order.Refund = true;
                payment.Status = "refund";
                _unitOfWork.Repository<Payment>().Update(payment);
                wallet.TotalPrice += order.OrderPrice;
                _unitOfWork.Repository<Wallet>().Update(wallet);
                await _unitOfWork.CompleteAsync();
            }
           await _unitOfWork.GetRepo<FailOrder>().AddAsync(failOrder);
            await _unitOfWork.CompleteAsync();

            

        }
    }
}
