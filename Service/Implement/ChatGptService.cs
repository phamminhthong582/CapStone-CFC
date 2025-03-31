using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Repository.Interface; 

public class ChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IFlowerRepository _flowerRepository;
    private readonly IFlowerBasketRepository _flowerBasketRepository;
    private readonly IFlowerCustomRepository _flowerCustomRepository;
    private readonly IProductCustomRepository _productCustomRepository;
    private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

    public ChatGptService(IConfiguration configuration, 
        IFlowerRepository flowerRepository, 
        IFlowerBasketRepository flowerBasketRepository , 
        IFlowerCustomRepository flowerCustomRepository,
        IProductCustomRepository productCustomRepository)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _flowerRepository = flowerRepository;
        _flowerBasketRepository = flowerBasketRepository;
        _flowerCustomRepository = flowerCustomRepository;
        _productCustomRepository = productCustomRepository;
    }

    public async Task<string> GenerateTextAsync(string userMessage)
{
    if (!userMessage.Contains("flower") && !userMessage.Contains("basket") && !userMessage.Contains("price") && !userMessage.Contains("delivery"))
    {
        return "Sorry, I can only answer questions related to flowers and flower baskets. Please ask something about flowers or baskets.";
    }
    
    string flowerInfo = "No flower info available";
    string flowerBasketInfo = "No flower basket info available";
    string flowerCustomInfo = "No flower custom info available";
    string productCustomInfo = "No product custom info available"; // Thêm thông tin cho sản phẩm tùy chỉnh

    if (userMessage.Contains("flower"))
    {
        flowerInfo = await _flowerRepository.GetFlowerInfoAsync(userMessage); 
    }
    if (userMessage.Contains("flowerbasket"))
    {
        flowerBasketInfo = await _flowerBasketRepository.GetFlowerBasketInfoAsync(userMessage); 
    }
    if (userMessage.Contains("flowercustom"))
    {
        flowerCustomInfo = await _flowerCustomRepository.GetFlowerCustomInfoAsync(userMessage); 
    }
    if (userMessage.Contains("productcustom"))
    {
        productCustomInfo = await _productCustomRepository.GetProductCustomInfoAsync(userMessage); // Thêm xử lý lấy thông tin sản phẩm tùy chỉnh
    }

    string prompt = $"You are a chatbot assisting with a flower ordering system. The user asks: '{userMessage}'. Here is some information: {flowerInfo} {flowerBasketInfo} {flowerCustomInfo} {productCustomInfo}"; // Cập nhật prompt với thông tin sản phẩm tùy chỉnh

    var requestBody = new
    {
        model = "gpt-3.5-turbo", 
        messages = new[] 
        {
            new { role = "system", content = "You are a helpful assistant who only answers questions related to flowers." },
            new { role = "user", content = prompt }
        }
    };

    var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

    try
    {
        var response = await _httpClient.PostAsync(OpenAiUrl, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            return $"Error calling OpenAI API: {response.StatusCode}";
        }

        var responseString = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseString);

        if (jsonResponse.TryGetProperty("choices", out var choices) && choices[0].TryGetProperty("message", out var message))
        {
            var content = message.GetProperty("content").GetString();
            return content ?? "No response content received.";
        }

        return "Error: Unable to parse OpenAI response.";
    }
    catch (Exception ex)
    {
        return $"Error processing request: {ex.Message}";
    }
}
}
