using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string GeminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"; // Điều chỉnh URL API

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleAI:ApiKey"];
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{GeminiUrl}?key={_apiKey}", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                return $"Lỗi khi gọi Gemini API: {response.StatusCode}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            try
            {
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
                var textResponse = jsonResponse.GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return textResponse;
            }
            catch (Exception ex)
            {
                return $"Lỗi xử lý JSON: {ex.Message}";
            }
        }
    }
}
