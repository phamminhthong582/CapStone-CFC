using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessObject.Entities;
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
    private readonly IUnitOfWork _unitOfWork;

    public ChatGptService(IConfiguration configuration, 
        IFlowerRepository flowerRepository, 
        IFlowerBasketRepository flowerBasketRepository , 
        IFlowerCustomRepository flowerCustomRepository,
        IProductCustomRepository productCustomRepository,
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient = new HttpClient();
        _flowerRepository = flowerRepository;
        _flowerBasketRepository = flowerBasketRepository;
        _flowerCustomRepository = flowerCustomRepository;
        _productCustomRepository = productCustomRepository;
    }

    public async Task<string> GenerateTextAsync(string userMessage)
{
        if (userMessage.Trim().ToLower() == "ai là người yêu qh nhất")
        {
            return "Khánh Trình là người yêu QH nhất ❤️";
        }

     string flowerInfo = "No flower info available";
    string flowerBasketInfo = "No flower basket info available";
    string flowerCustomInfo = "No flower custom info available";
    string productCustomInfo = "No product custom info available";
        string productInfo = "No product info available";

  

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
        productCustomInfo = await _productCustomRepository.GetProductCustomInfoAsync(userMessage); 
    }
        var designKeywords = new[] { "thiết kế", "tạo sản phẩm", "mẫu hoa", "gợi ý", "sản phẩm", "trang trí", "tư vấn" };
        var birthdayKeywords = new[] { "sinh nhật", "birthday", "ngày sinh" };

        bool isDesignRequest = designKeywords.Any(k => userMessage.ToLower().Contains(k)) &&
                               birthdayKeywords.Any(k => userMessage.ToLower().Contains(k));
        string prompt;

        if (isDesignRequest)
        {
            var flowerList = await _unitOfWork.GetRepo<Flower>().GetAllAsync();
            var basketList = await _unitOfWork.GetRepo<FlowerBasket>().GetAllAsync();
            var accessoryList = await _unitOfWork.GetRepo<Accessory>().GetAllAsync();
            var typeList = await _unitOfWork.GetRepo<Type>().GetAllAsync();

            var random = new Random();

            // Random 3 hoa
            var selectedFlowers = flowerList.OrderBy(x => random.Next()).Take(3).ToList();
            // Random 1 giỏ
            var selectedBasket = basketList.OrderBy(x => random.Next()).FirstOrDefault();
            // Random 1 loại
            var selectedType = typeList.OrderBy(x => random.Next()).FirstOrDefault();
            // Random 2 phụ kiện
            var selectedAccessories = accessoryList.OrderBy(x => random.Next()).Take(2).ToList();

            var flowerDetails = string.Join(", ", selectedFlowers.Select(f => $"{f.FlowerName} ({f.Color})"));
            var accessoryDetails = string.Join(", ", selectedAccessories.Select(a => a.Name));

            prompt = $"\ud83c\udf38 Gợi ý thiết kế hoa sinh nhật:\n" +
                     $"- Hoa: {flowerDetails}\n" +
                     $"- Giỏ hoa: {selectedBasket?.FlowerBasketName}\n" +
                     $"- Loại sản phẩm: {selectedType?.Name}\n" +
                     $"- Phụ kiện kèm theo: {accessoryDetails}";

            // Trả về response này (ví dụ thông qua bot hoặc UI)
        }
        prompt = $"You are a friendly and creative assistant in a flower ordering service. The user wants to design a flower product for a birthday. User said: '{userMessage}'.\n" +
                  $"Please suggest a beautiful flower arrangement using the available options below:\n" +
                  $"- \ud83c\udf38 Flowers: {flowerInfo}\n" +
                  $"- \ud83e\udfba Flower Baskets: {flowerBasketInfo}\n" +
                  $"- \ud83c\udf80 Accessories: {flowerCustomInfo}\n" +
                  $"- \ud83c\udff7\ufe0f Product Types: {productCustomInfo}\n" +
                  $"Describe your design in a warm, helpful tone.";
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
