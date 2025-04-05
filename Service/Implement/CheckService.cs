using BusinessObject.DTO.Check;
using BusinessObject.Entities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repository.Implement;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class CheckService : ICheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _googleApiKey;


        public CheckService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _googleApiKey = configuration["GoogleMaps:ApiKey"]; // Lấy từ config
        }

        public async Task<(bool isSuccess, double? distance)> CheckDeliveryByCheckOutProduct(Guid storeId, CheckProductRequest checkProductRequest)
        {
            try
            {
                var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(storeId);
                if (store == null) return (false, null);

                // Tính tổng trọng lượng
                double? totalWeight = 0.0;
                if (checkProductRequest.CheckQuantityAndWeightProducts != null)
                {
                    // Tạo danh sách các task
                    var tasks = checkProductRequest.CheckQuantityAndWeightProducts
                        .Select(async item =>
                        {
                            var product = await _unitOfWork.GetRepo<Product>().GetByIdAsync(item.ProductId);
                            return (product?.Weight ?? 0) * item.Quantity;
                        })
                        .ToList();

                    // Chờ tất cả task hoàn thành
                    var weights = await Task.WhenAll(tasks);
                    totalWeight = weights.Sum();
                }

                // Validate trọng lượng
                if (totalWeight > 5) return (false, null);
                if (totalWeight <= 0) return (false, null);

                string deliveryAddress = $"{checkProductRequest.DetailedAddress}, {checkProductRequest.District}, {checkProductRequest.City}";
                string storeAddress = $"{store.Address}, {store.District}, {store.City}";

                var distanceResult = await GetDistanceFromGoogleAsync(storeAddress, deliveryAddress);
                if (!distanceResult.isSuccess) return (false, null);

                double distance = distanceResult.distance.Value;

                // Tính phí vận chuyển
                if (distance < 5 && totalWeight < 3)
                {
                    double shipperMoney = 10000 + distance * 5000;
                    return (true, shipperMoney);
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                return (false, null );
            }
        }

        public async Task<(bool isSuccess, double? distance, string message)> GetDistanceFromGoogleAsync(string origin, string destination)
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
                    return (false, null, $"{API_CONNECTION_ERROR}. Status: {statusCode}. Response: {responseContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var distanceMatrix = JsonConvert.DeserializeObject<GoogleDistanceMatrixResponse>(content);

                // Kiểm tra response hợp lệ
                if (distanceMatrix?.rows == null ||
                    distanceMatrix.rows.Count == 0 ||
                    distanceMatrix.rows[0].elements == null ||
                    distanceMatrix.rows[0].elements.Count == 0)
                {
                    return (false, null, $"{INVALID_RESPONSE_FORMAT}. Response: {content}");
                }

                var element = distanceMatrix.rows[0].elements[0];
                if (element.status != "OK")
                {
                    string errorDetails = element.status;
                    if (distanceMatrix.error_message != null)
                    {
                        errorDetails += $". {distanceMatrix.error_message}";
                    }
                    return (false, null, $"{GOOGLE_API_ERROR}: {errorDetails}");
                }

                double meters = element.distance.value;
                return (true, meters / 1000.0, CALCULATION_SUCCESS);
            }
            catch (HttpRequestException httpEx)
            {
                return (false, null, $"{API_CONNECTION_ERROR}: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                return (false, null, $"{INVALID_RESPONSE_FORMAT}: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                // Log full exception details here
                return (false, null, $"{UNKNOWN_ERROR}: {ex.Message}");
            }
        }

        public class GoogleDistanceMatrixResponse
        {
            public List<Row> rows { get; set; }
            public string status { get; set; }
            public string error_message { get; set; } // Thêm trường error_message

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
                public string text { get; set; } // Ví dụ: "1.5 km"
                public int value { get; set; } // in meters
            }

            public class DurationInfo
            {
                public string text { get; set; } // Ví dụ: "5 phút"
                public int value { get; set; } // in seconds
            }
        }
        public async Task<(bool isSuccess, double? distance)> CheckDeliveryByProductCustom( Guid storeId, CheckProductFlowerRequest checkProductFlowerRequest)
        {
            try
            {
                // 1. Validate input
                if (checkProductFlowerRequest == null)
                    return (false, null);

                // 2. Get store information
                var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(storeId);
                if (store == null)
                    return (false, null);

                // 3. Validate product quantity
                if (checkProductFlowerRequest.ProductQuantity > 2)
                    return (false, null);

                // 4. Prepare addresses
                string deliveryAddress = $"{checkProductFlowerRequest.DetailedAddress}, {checkProductFlowerRequest.District}, {checkProductFlowerRequest.City}";
                string storeAddress = $"{store.Address}, {store.District}, {store.City}";

                // 5. Get distance from Google Maps API
                var distanceResult = await GetDistanceFromGoogleAsync(storeAddress, deliveryAddress);
                if (!distanceResult.isSuccess)
                    return (false, null );

                double distance = distanceResult.distance.Value;

                // 6. Check distance limit
                if (distance >= 5)
                    return (false, null);

                // 7. Calculate shipping fee
                double shippingFee = 10000 + distance * 5000;
                return (true, shippingFee);
            }
            catch (Exception ex)
            {
                // Log error here
                return (false, null);
            }
        }


        public async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address)
        {
            using (var httpClient = new HttpClient())
            {
                string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";
                httpClient.DefaultRequestHeaders.Add("User-Agent", "TripPlanner/1.0 (trinhbloc2003@email.com)");

                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<NominatimResponse>>(content);

                    if (results != null && results.Count > 0)
                    {
                        return (results[0].Lat, results[0].Lon);
                    }
                }
            }
            return null;
        }
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Khoảng cách tính bằng km
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

      
        public class NominatimResponse
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }
    }
}
