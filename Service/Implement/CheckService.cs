using BusinessObject.DTO.Check;
using BusinessObject.Entities;
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

        public CheckService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool isSuccess, double? distance)> CheckDeliveryByCheckOutProduct(Guid storeId, CheckProductRequest checkProductRequest)
        {
            var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(storeId);
            double? totalWeight = 0.0;

            if (checkProductRequest.CheckQuantityAndWeightProducts != null)
            {
                foreach (var item in checkProductRequest.CheckQuantityAndWeightProducts)
                {
                    var product = await _unitOfWork.GetRepo<Product>().GetByIdAsync(item.ProductId);
                    totalWeight += (product.Weight ?? 0) * item.Quantity; 
                }
            }
            string deliveryAddress = $"{checkProductRequest.DetailedAddress}, {checkProductRequest.District}, {checkProductRequest.City}";
            string StoreAddress = store.Address + "," + store.District + "," + store.City;
            var storeLocation = await GetCoordinatesAsync(StoreAddress);
            var deliveryLocation = await GetCoordinatesAsync(deliveryAddress);
            if (storeLocation == null || deliveryLocation == null)
                return (false,-1);
            double distance = CalculateDistance(storeLocation.Value.Latitude, storeLocation.Value.Longitude,
                                        deliveryLocation.Value.Latitude, deliveryLocation.Value.Longitude);
            double? shipperMoney = 0.0;
            if (totalWeight < 3)
            {
                if(distance < 5)
                {
                    shipperMoney = 10000 + distance * 5000;
                    return (true, shipperMoney);
                }
                else if (distance > 5)
                {
                    return (false, shipperMoney);
                }

            }
            else if(totalWeight > 5)
            {
                return (false, shipperMoney);
            }

            return (false, shipperMoney);
        }

        public async Task<(bool isSuccess, double? distance)> CheckDeliveryByProductCustom(Guid storeId, CheckProductFlowerRequest checkProductFlowerRequest)
        {
            var store = await _unitOfWork.GetRepo<Store>().GetByIdAsync(storeId);
            string deliveryAddress = $"{checkProductFlowerRequest.DetailedAddress}, {checkProductFlowerRequest.District}, {checkProductFlowerRequest.City}";
            string StoreAddress = store.Address + "," + store.District + "," + store.City;
            var storeLocation = await GetCoordinatesAsync(StoreAddress);
            var deliveryLocation = await GetCoordinatesAsync(deliveryAddress);
            if (storeLocation == null || deliveryLocation == null)
                return (false, -1);
            double distance = CalculateDistance(storeLocation.Value.Latitude, storeLocation.Value.Longitude,
                                        deliveryLocation.Value.Latitude, deliveryLocation.Value.Longitude);
            double? shipperMoney = 0.0;
            if (checkProductFlowerRequest.ProductQuantity < 2)
            {
                if (distance < 5)
                {
                    shipperMoney = 10000 + distance * 5000;
                    return (true, shipperMoney);
                }
                else if (distance > 5)
                {
                    return (false, shipperMoney);
                }

            }
            else if (checkProductFlowerRequest.ProductQuantity > 2)
            {
                return (false, shipperMoney);
            }

            return (false, shipperMoney);
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
