using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Repository.Implement;
using Repository.Interface;

namespace Service.Implement
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IFlowerRepository _flowerRepository;
        private readonly IFlowerBasketRepository _flowerBasketRepository;

        private const string GeminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"; // Điều chỉnh URL API

        public GeminiService(HttpClient httpClient, IConfiguration configuration , IFlowerRepository flowerRepository , IFlowerBasketRepository flowerBasketRepository)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleAI:ApiKey"];
            _flowerRepository = flowerRepository;
            _flowerBasketRepository = flowerBasketRepository;
        }

        public async Task<string> GenerateTextAsync(string userMessage)
        {
            string flowerInfo = "";
            string flowerBasketInfo = ""; 
            if (userMessage.Contains("flower"))
            {
                flowerInfo = await _flowerRepository.GetFlowerInfoAsync(userMessage);
            }
            if (userMessage.Contains("flowerbasket"))
            {
                flowerBasketInfo = await _flowerBasketRepository.GetFlowerBasketInfoAsync(userMessage);
            }
            string prompt = $"You are a chatbot assisting with a flower ordering system. The user asks: '{userMessage}'. Here is some information: {flowerInfo } {flowerBasketInfo}";

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
                return $"Error calling Gemini API: {response.StatusCode}";
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
                return $"Error processing JSON: {ex.Message}";
            }
        }
    }
} 