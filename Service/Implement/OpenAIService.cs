using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string OpenAIUrl = "https://api.openai.com/v1/images/generations";

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API Key is missing");

            // Thiết lập header Authorization chỉ 1 lần
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateImageAsync(string prompt)
        {
            var requestBody = new
            {
                prompt = prompt,
                n = 1,
                size = "1024x1024"
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(OpenAIUrl, requestContent);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Lỗi khi gọi OpenAI: {response.StatusCode} - {responseString}";
            }

            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
            if (jsonResponse.TryGetProperty("data", out var dataArray) && dataArray.GetArrayLength() > 0)
            {
                return dataArray[0].GetProperty("url").GetString();
            }

            return "Không tìm thấy ảnh trong phản hồi OpenAI.";
        }
    }
}
