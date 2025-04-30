using BusinessObject.DTO.Chat;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.DesignCustom;
using BusinessObject.DTO.Message;
using BusinessObject.DTO.Order;
using BusinessObject.Entities;
using MailKit.Search;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Cryptography;
using Newtonsoft.Json;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Service.Implement
{
    public class DesignCustomService : IDesignCustomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IMessageService _messageService;   
        private readonly IChatRoomService _chatRoomService;
        private readonly INotiService _notiService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _googleApiKey;

        public DesignCustomService(IUnitOfWork unitOfWork,IConfiguration configuration, CloudinaryService cloudinaryService, IMessageService messageService, IChatRoomService chatRoomService, INotiService notiService, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _googleApiKey = configuration["GoogleMaps:ApiKey"]; // Lấy từ config

            _messageService = messageService;
            _chatRoomService = chatRoomService;
            _notiService = notiService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task CreateDesignCustomByCustomer(DesignCustomByCustomerRequest designCustomByCustomerRequest, Guid customerId)
        {
            var folderName = $"flowerBasket/{designCustomByCustomerRequest.RequestImage}";
            var url = designCustomByCustomerRequest.RequestImage != null
            ? await _cloudinaryService.UploadImageAsync(designCustomByCustomerRequest.RequestImage.OpenReadStream(), $"{folderName}")
            : null;
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var designCustom = new DesignCustom
            {
                CustomerId = customerId,
                RequestImage = url,
                RequestDescription = designCustomByCustomerRequest.RequestDescription,
                RequestPrice = designCustomByCustomerRequest.RequestPrice,
                Phone = designCustomByCustomerRequest.Phone,
                RequestOccasion = designCustomByCustomerRequest.RequestOccasion,
                RequestMainColor = designCustomByCustomerRequest.RequestMainColor,
                RequestFlowerType = designCustomByCustomerRequest.RequestFlowerType,
                RequestCard = designCustomByCustomerRequest.RequestCard,
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Status = "Send Request",
            };
            await _unitOfWork.Repository<DesignCustom>().AddAsync(designCustom);
            await _unitOfWork.CompleteAsync();
            var order = new Order()
            {
                CustomerId = customerId,
                StoreId = designCustomByCustomerRequest.StoreId,
                //DeliveryDistrict = designCustomByCustomerRequest.DeliveryDistrict,
                //DeliveryCity = designCustomByCustomerRequest.DeliveryCity,
                //DeliveryAddress = designCustomByCustomerRequest.DeliveryAddress,
                Note = designCustomByCustomerRequest.Note,
                Phone = designCustomByCustomerRequest.Phone,
                RecipientName = designCustomByCustomerRequest.RecipientName,
                CreateAt = vietnamTime,
                UpdateAt = vietnamTime,
                Refund = false,
                Status = "Pending Payment",
                DesignCustomId =designCustom.DesignCustomId,
            };
            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();
            designCustom.OrderId = order.OrderId;
            _unitOfWork.Repository<DesignCustom>().Update(designCustom);
            await _unitOfWork.CompleteAsync();
            var employee = await _unitOfWork.GetRepo<Employee>().Entities.Where(m => m.StoreId == order.StoreId).Include(n => n.User).Where(m => m.User.Role.RoleName == "Florist").ToListAsync();
            var random = new Random();
            var randomEmployee = employee[random.Next(employee.Count)];
            order.StaffId = randomEmployee.EmployeeId;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();

            var chatRoomRequest = new CreateChatRoomRequest { OrderId = order.OrderId };
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

            var messageRequest = new CreateMessageRequest
            {
                ChatRoomId = chatRoom.ChatRoomId,
                SenderId = order.StaffId,
                ReceiveId = order.CustomerId,
                MessageType = "text",
                Content = $"Hello, I am the employee with ID number: {order.StaffId}, I will serve your order. We will consult your order via this chat channel."
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
                    ToUserId = order.StaffId,
                    Message = $"bạn có một đơn hàng cần xử lý",
                    Type = "Order",
                    RelatedId = order.StaffId,
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



        public async Task UpdateDesignCustomByStaff(DesignCustomByStaffRequest designCustomByStaffRequest, Guid DesginCustom)
        {
            var folderName = $"flowerBasket/{designCustomByStaffRequest.ResponseImage}";
            var url = designCustomByStaffRequest.ResponseImage != null
            ? await _cloudinaryService.UploadImageAsync(designCustomByStaffRequest.ResponseImage.OpenReadStream(), $"{folderName}")
            : null;
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var desginCustom = await _unitOfWork.GetRepo<DesignCustom>().GetByIdAsync(DesginCustom);
            var Order = await _unitOfWork.GetRepo<Order>().Entities.FirstOrDefaultAsync(m => m.OrderId == desginCustom.OrderId);
            desginCustom.ResponseDescription = designCustomByStaffRequest.ResponseDescription;
            desginCustom.ResponseImage = url;
            desginCustom.ResponsePrice = designCustomByStaffRequest?.ResponsePrice;
            Order.OrderPrice = desginCustom.ResponsePrice;
            desginCustom.UpdateAt = vietnamTime;
            desginCustom.Status = "Send Response";
            _unitOfWork.Repository<DesignCustom>().Update(desginCustom);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<Guid> UpdateDesignCustomByCustomer(UpdateOrderDesignCustomByCustomer updateOrderDesignCustomByCustomer, Guid DesginCustom)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            var desginCustom = await _unitOfWork.GetRepo<DesignCustom>().GetByIdAsync(DesginCustom);
            var Order = await _unitOfWork.GetRepo<Order>().Entities.FirstOrDefaultAsync(m => m.OrderId == desginCustom.OrderId);
            Order.DeliveryDistrict= updateOrderDesignCustomByCustomer.DeliveryDistrict;
            Order.DeliveryCity = updateOrderDesignCustomByCustomer.DeliveryCity;
            Order.DeliveryAddress = updateOrderDesignCustomByCustomer.DeliveryAddress;
            Order.Wallet = updateOrderDesignCustomByCustomer.Wallet;
            Order.Transfer = updateOrderDesignCustomByCustomer.Transfer;
            Order.Delivery = updateOrderDesignCustomByCustomer.Delivery;
            Order.RecipientTime = updateOrderDesignCustomByCustomer.RecipientTime;
            _unitOfWork.Repository<Order>().Update(Order);
            await _unitOfWork.CompleteAsync();
            var design = await _unitOfWork.GetRepo<DesignCustom>().Entities.FirstOrDefaultAsync(m => m.DesignCustomId == DesginCustom);
            
            //design.Status = "Design Successfully";
            //if (Order.Status != "Arranging & Packing")
            //{
            //    design.Status = "Send Response";
            //}
            //_unitOfWork.Repository<DesignCustom>().Update(design);
            //await _unitOfWork.CompleteAsync();
            double? totalPrice = Order.OrderPrice;
            if (Order.Delivery == true)
            {
                string deliveryAddress = $"{Order.DeliveryAddress}, {Order.DeliveryDistrict}, {Order.DeliveryCity}";
                var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(Order.StoreId);
                string StoreAddress = store.Address + "," + store.District + "," + store.City;
                double distance = await GetDistanceFromGoogleAsync(StoreAddress, deliveryAddress);
                double? shipperMoney = 0.0;
                if (distance < 5)
                {
                    shipperMoney = 10000 + distance * 5000;
                    totalPrice = shipperMoney + totalPrice;
                    Order.OrderPrice = totalPrice;
                    _unitOfWork.Repository<Order>().Update(Order);
                    await _unitOfWork.CompleteAsync();
                }
                else if (distance > 5)
                {
                    totalPrice = totalPrice;
                    Order.OrderPrice = totalPrice;
                    _unitOfWork.Repository<Order>().Update(Order);
                    await _unitOfWork.CompleteAsync();
                }
            }
            return Order.OrderId;
        }

        public async Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByCustomer(Guid customer)
        {
            var designCustom = await _unitOfWork.GetRepo<DesignCustom>().Entities.Include(a => a.Order).ThenInclude(b =>b.Staff)
                                                                                   .Where(m => m.CustomerId == customer).OrderByDescending(n => n.CreateAt).ToListAsync();
            var result = designCustom.Select(dc => new DesignCustomBuCustomerResponse
            {
                DesignCustomId = dc.DesignCustomId,
                RequestImage = dc.RequestImage,
                RequestDescription = dc.RequestDescription,
                RequestPrice = dc.RequestPrice,
                RequestCard = dc.RequestCard,
                RequestFlowerType = dc.RequestFlowerType,
                RequestOccasion = dc.RequestOccasion,
                RequestMainColor = dc.RequestMainColor,
                ResponsePrice = dc.ResponsePrice,
                ResponseImage = dc.ResponseImage,
                ResponseDescription = dc.ResponseDescription,
                Phone = dc.Phone,
                CustomerId = dc.CustomerId,
                CreateAt = dc.CreateAt,
                UpdateAt = dc.UpdateAt,
                Status = dc.Status,
                OrderId = dc.OrderId,
                StaffId = dc.Order.StaffId,
                StaffName = dc.Order.Staff.FullName,
                StoreId = dc.Order.StoreId,
                Note = dc.Order.Note,
                RecipientName = dc.Order.RecipientName,
            });
            return result;


        }
        public async Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByStore(Guid StoreId)
        {
            
            var designCustom = await _unitOfWork.GetRepo<DesignCustom>().Entities.Include(a => a.Order).ThenInclude(b => b.Staff).Where(m => m.Order.StoreId == StoreId).OrderByDescending(n => n.CreateAt).ToListAsync();
            var result = designCustom.Select(dc => new DesignCustomBuCustomerResponse
            {
                DesignCustomId = dc.DesignCustomId,
                RequestImage = dc.RequestImage,
                RequestDescription = dc.RequestDescription,
                RequestPrice = dc.RequestPrice,
                RequestCard = dc.RequestCard,
                RequestFlowerType = dc.RequestFlowerType,
                RequestOccasion = dc.RequestOccasion,
                RequestMainColor = dc.RequestMainColor,
                ResponsePrice = dc.ResponsePrice,
                ResponseImage = dc.ResponseImage,
                ResponseDescription = dc.ResponseDescription,
                Phone = dc.Phone,
                CustomerId = dc.CustomerId,
                CreateAt = dc.CreateAt,
                UpdateAt = dc.UpdateAt,
                Status = dc.Status,
                OrderId = dc.OrderId,
                StaffId = dc.Order.StaffId,
                StaffName = dc.Order.Staff.FullName,
                StoreId = dc.Order.StoreId,
                Note = dc.Order.Note,
                RecipientName = dc.Order.RecipientName,
            });
            return result;


        }
        public async Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByStaff(Guid StaffId)
        {
            var designCustom = await _unitOfWork.GetRepo<DesignCustom>().Entities.Include(a => a.Order).ThenInclude(b => b.Staff).Where(m => m.Order.StaffId == StaffId).OrderByDescending(n => n.CreateAt).ToListAsync();
            var result = designCustom.Select(dc => new DesignCustomBuCustomerResponse
            {
                DesignCustomId = dc.DesignCustomId,
                RequestImage = dc.RequestImage,
                RequestDescription = dc.RequestDescription,
                RequestPrice = dc.RequestPrice,
                RequestCard = dc.RequestCard,
                RequestFlowerType = dc.RequestFlowerType,
                RequestOccasion = dc.RequestOccasion,
                RequestMainColor = dc.RequestMainColor,
                ResponsePrice = dc.ResponsePrice,
                ResponseImage = dc.ResponseImage,
                ResponseDescription = dc.ResponseDescription,
                Phone = dc.Phone,
                CustomerId = dc.CustomerId,
                CreateAt = dc.CreateAt,
                UpdateAt = dc.UpdateAt,
                Status = dc.Status,
                OrderId = dc.OrderId,
                StoreId = dc.Order.StoreId,
                StaffId = dc.Order.StaffId,
                StaffName = dc.Order.Staff.FullName,
                Note = dc.Order.Note,
                RecipientName = dc.Order.RecipientName,
            });
            return result;
        }

        public async Task<DesignCustomBuCustomerResponse> GetDesignCustomById(Guid id)
        {
            var designCustom = await _unitOfWork.GetRepo<DesignCustom>()
                .Entities
                .Include(a => a.Order).ThenInclude(b => b.Staff)
                .FirstOrDefaultAsync(m => m.DesignCustomId == id);

            if (designCustom == null)
                return null;

            return new DesignCustomBuCustomerResponse
            {
                DesignCustomId = designCustom.DesignCustomId,
                RequestImage = designCustom.RequestImage,
                RequestDescription = designCustom.RequestDescription,
                RequestPrice = designCustom.RequestPrice,
                RequestOccasion = designCustom.RequestOccasion,
                RequestMainColor = designCustom.RequestMainColor,
                RequestFlowerType = designCustom.RequestFlowerType,
                RequestCard = designCustom.RequestCard,
                ResponsePrice = designCustom.ResponsePrice,
                ResponseImage = designCustom.ResponseImage,
                ResponseDescription = designCustom.ResponseDescription,
                Phone = designCustom.Phone,
                CustomerId = designCustom.CustomerId,
                CreateAt = designCustom.CreateAt,
                UpdateAt = designCustom.UpdateAt,
                Status = designCustom.Status,
                OrderId = designCustom.OrderId,
                StaffId = designCustom.Order.StaffId,
                StaffName = designCustom.Order.Staff.FullName,
                Note = designCustom.Order.Note,
                StoreId = designCustom.Order.StoreId,
                RecipientName = designCustom.Order.RecipientName
            };
        }

        public async Task DeleteDesignCustom(Guid id)
        {
            var designCustom = await _unitOfWork.GetRepo<DesignCustom>()
                         .Entities
                         .Include(a => a.Order)
                         .FirstOrDefaultAsync(m => m.DesignCustomId == id);
            var Order = await _unitOfWork.GetRepo<Order>().Entities.FirstOrDefaultAsync(m => m.OrderId == designCustom.OrderId);
            designCustom.Status = "Design Failure";

            _unitOfWork.GetRepo<DesignCustom>().Update(designCustom);
            await _unitOfWork.CompleteAsync();
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
                    return 0;
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
                    return 0;
                }

                double meters = element.distance.value;
                return meters / 1000.0; // Convert to km
            }
            catch (Exception ex)
            {
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



    }
}
