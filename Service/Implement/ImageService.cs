using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement
{
    public  class ImageService
    {
        private readonly HttpClient _httpClient;
        private const string StableDiffusionUrl = "https://stablediffusionapi.com/api/v3/text2img";
        private const string ApiKey = "fYSI6Vti9394uTsmfryATkdWspICEmH5snRIfgfDr68LxzbSulqQHPB9Msdz"; // Thay bằng API Key của bạn

        public ImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateImageAsync(string description)
        {
            var requestBody = new
            {
                key = ApiKey,
                prompt = description,
                width = 512,
                height = 512,
                samples = 1
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(StableDiffusionUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                return $"Lỗi khi gọi API Stable Diffusion: {response.StatusCode}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

            return jsonResponse.GetProperty("output")[0].GetString(); // URL ảnh
        }
    }
}
